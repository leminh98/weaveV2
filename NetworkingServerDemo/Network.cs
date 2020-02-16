using System;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace NetworkingDemo
{
   
    public static class Network // A Basics Network class
    {
        public static NetServer Server; //the Server

        public static NetPeerConfiguration Config; //the Server config

        /*public*/
        static NetIncomingMessage incmsg; //the incoming messages that server can read from clients

        public static NetOutgoingMessage outmsg; //the outgoing messages that clients can receive and read

        /*public*/
        static bool playerRefresh; //below for explanation...

        public static void Update()
        {
            while ((incmsg = Server.ReadMessage()) != null) //while the message is received, and is not equal to null...
            {
                
                switch (incmsg.MessageType) 
                    //There are several types of messages (see the Lidgren Basics tutorial), but it is easier to just use it the most important thing the "Data".
                {
                    case NetIncomingMessageType.Data:
                    {
                        // Now this example I'm use to string (yes you can saving the bandwidth with all messages, if you use integer)
                        string headStringMessage = incmsg.ReadString(); 
                        
                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
                            case "connect":
                            {
                                #region connect
                                Console.WriteLine("connect message receive");
                                string name = incmsg.ReadString();
                                string spriteType = incmsg.ReadString(); 

                                #region checking duplicate
                                playerRefresh = true; 
                                Console.WriteLine("Checking for duplicate");
                                // Now check to see if you have at least one of our players, the subsequent attempts to connect with the same name. 
                                for (int i = 0; i < Player.players.Count; i++)
                                {
                                    if (Player.players[i].name.Equals(name)) //If its is True...
                                    {
                                        outmsg = Server.CreateMessage(); 
                                        outmsg.Write( "deny"); 

                                        //Sending the deny message
                                        Server.SendMessage(outmsg, incmsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                                        //a little pause the current process to make sure the message is sent to the client
                                        // before the server break down contact with the client. 
                                        System.Threading.Thread .Sleep( 100); 
                                        incmsg.SenderConnection.Disconnect(  "bye"); 
                                        playerRefresh = false; //Now the "if" its is True, we disable the playerRefhres bool
                                        break;
                                    }
                                }
                                #endregion

                                // if not duplicate connection
                                if (playerRefresh == true)
                                {
                                    //A little pause to make sure you connect the client before performing further operations
                                    System.Threading.Thread.Sleep(50); 
                                    
                                    var isAuthoritative = Player.players.Count == 0;
                                    //Add to player messages received as a parameter
                                    Player.players.Add(new Player(name, new Vector2(0, 0),
                                        0, spriteType, isAuthoritative)); 
                                    Console.WriteLine(name + " connected.");

                                    for (int i = 0; i < Player.players.Count; i++)
                                    {
                                            Console.WriteLine("sending " + name + " to " + Player.players[i].name);
                                            // Write a new message with incoming parameters, and send the all connected clients.
                                            outmsg = Server.CreateMessage();

                                            outmsg.Write("connect");
                                            outmsg.Write(Player.players[i].name);
                                            outmsg.Write(Player.players[i].spriteType);

                                            Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                                NetDeliveryMethod.ReliableOrdered, 0);
                                    }
                                }

                                Console.WriteLine("Number of players: " + Player.players.Count);
                                #endregion
                            }
                                break;
                            case "startGame": //if the firs message/data is "connect"
                            {
                                #region startGame
                                Console.WriteLine("startGame Message receive");
                                string name = incmsg.ReadString(); 
                                int x = incmsg.ReadInt32(); //Reading the x position
                                int y = incmsg.ReadInt32(); // y position

                                foreach (var player in Player.players)
                                {
                                    if (player.name.Equals(name))
                                    {
                                        player.pozition = new Vector2(x, y);
                                        player.timeOut = 0; //below for explanation (Player class)...
                                        break;
                                    }
                                }

                                foreach (var player in Player.players)
                                {
                                    // Write a new message with incoming parameters, and send the all connected clients.
                                    outmsg = Server.CreateMessage();

                                    outmsg.Write("startGame");
                                    outmsg.Write(player.name);
                                    outmsg.Write((int) player.pozition.X);
                                    outmsg.Write((int) player.pozition.Y);

                                    Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                        NetDeliveryMethod.ReliableOrdered, 0);
                                }
                                #endregion
                            }
                                break;
                            case "mapSelect":
                            {
                                #region mapSelect

                                if (Map.isSet == false)
                                {
                                    string mapName = incmsg.ReadString();
                                    Map.isSet = Int32.TryParse(mapName.Replace("map", ""), out Map.chosenMapNum);
                                    //send it to everyone the first time it is set
                                    // Write a new message with incoming parameters, and send the all connected clients.
                                    outmsg = Server.CreateMessage();

                                    outmsg.Write("mapSelect");
                                    outmsg.Write("map" + Map.chosenMapNum);
                                    Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                        NetDeliveryMethod.ReliableOrdered, 0);
                                }
                                else
                                {
                                    //if someomne is trying to set the map again after the map is set, just send them the map
                                    outmsg = Server.CreateMessage();

                                    outmsg.Write("mapSelect");
                                    outmsg.Write("map" + Map.chosenMapNum);
                                    Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                        NetDeliveryMethod.ReliableOrdered, 0);
                                }

                                #endregion
                            }
                                break;
                            case "move": //The moving messages
                            {
                                #region move
                                try
                                {
                                    string name = incmsg.ReadString();
                                    int x = incmsg.ReadInt32();
                                    int y = incmsg.ReadInt32();
                                    int deltaX = incmsg.ReadInt32();
                                    int deltaY = incmsg.ReadInt32();
                                    bool fired = incmsg.ReadBoolean();
                                    int health = incmsg.ReadInt32();
                                    foreach (var player in Player.players)
                                    {
                                        if (player.name.Equals(name))
                                        {
                                            player.pozition = new Vector2(x, y);
                                            player.velocity = new Vector2(deltaX, deltaY);
                                            player.fired = fired;
                                            player.health = health;
                                            player.timeOut = 0; //below for explanation (Player class)...
                                            break;
                                        }
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                                #endregion
                            }
                                break;

                            case "disconnect": //If the client want to disconnect from server at manually
                            {
                                #region disconnect
                                string name = incmsg.ReadString();

                                for (int i = 0; i < Player.players.Count; i++)
                                {
                                    if (Player.players[i].name.Equals(name)
                                    ) //If the [index].name equaled the incoming message name...
                                    {
                                        Server.Connections[i]
                                            .Disconnect("bye"); //The server disconnect the correct client with index
                                        System.Threading.Thread
                                            .Sleep(
                                                100); //Again a small pause, the server disconnects the client actually
                                        Console.WriteLine(name + " disconnected.");

                                        if (Server.ConnectionsCount != 0) //After if clients count not 0
                                        {
                                            //Sending the disconnected client name to all online clients
                                            outmsg = Server.CreateMessage();
                                            outmsg.Write("disconnect");
                                            outmsg.Write(name);
                                            Server.SendMessage(Network.outmsg, Server.Connections,
                                                NetDeliveryMethod.ReliableOrdered, 0);
                                        }

                                        Player.players.RemoveAt(i); //And remove the player object
                                        i--;
                                        break;
                                    }
                                }

                                Console.WriteLine("Players: " + Player.players.Count);
                                #endregion
                            }
                                break;
                        }
                    }
                        break;
                }

                Server.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
        }

        public static void Shutdown()
        {
            for (int i = 0; i < Player.players.Count; i++)
            {
                // Server.Connections[i]
                //     .Disconnect("bye"); //The server disconnect the correct client with index
                // System.Threading.Thread
                //     .Sleep(
                //         100); //Again a small pause, the server disconnects the client actually
                // Console.WriteLine(Player.players[i].name + " disconnected.");

                if (Server.ConnectionsCount != 0) //After if clients count not 0
                {
                    //Sending the disconnected client name to all online clients
                    outmsg = Server.CreateMessage();
                    outmsg.Write("disconnect");
                    outmsg.Write(Player.players[i].name);
                    Server.SendMessage(Network.outmsg, Server.Connections,
                        NetDeliveryMethod.ReliableOrdered, 0);
                }

                Player.players.RemoveAt(i); //And remove the player object
                i--;
            }
            Server.Shutdown("server shutting down");
        }
    }

}