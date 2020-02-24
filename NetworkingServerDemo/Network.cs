using System;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace NetworkingDemo
{
    public static class Network
    {
        public static NetServer Server;
        public static NetPeerConfiguration Config;
        static NetIncomingMessage incmsg; //the incoming messages that server can read from clients
        public static NetOutgoingMessage outmsg; //the outgoing messages that clients can receive and read
        static bool playerRefresh;

        public static bool connectPhaseDone = false;
        // private static bool connectionMessageSent = false; //Signal that the server has sent connection message, so we only send it once

        public static bool playerSelectionPhaseDone = false;
        private static int numSpriteSelected = 0; //if this equal to Numplayer, then the selection phase is done
        
        public static bool mapSelectionPhaseDone = false;
        private static int numPlayerReceivedMap = 0;
        public static bool singleGamePhaseDone = false;
        public static bool postSingleGamePhaseDone = false;
        public static bool gameOver = false;

        /**
         * Wait for all Program.Numplayer to connect to the server
         * Once Numbplayer players have connected, send connect message to every client.
         * Ignore all other messages.
         */
        public static void connectionPhase()
        {
            while ((incmsg = Server.ReadMessage()) != null) //while the message is received, and is not equal to null...
            {
                switch (incmsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                    {
                        string headStringMessage = incmsg.ReadString();

                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
                            case "connect":
                            {
                                #region connect

                                Console.WriteLine("connect message receive");
                                string name = incmsg.ReadString();

                                #region checking duplicate

                                playerRefresh = true;
                                Console.WriteLine("Checking for duplicate");
                                // Now check to see if you have at least one of our players, the subsequent attempts to connect with the same name. 
                                for (int i = 0; i < Player.players.Count; i++)
                                {
                                    if (Player.players[i].name.Equals(name)) //If its is True...
                                    {
                                        outmsg = Server.CreateMessage();
                                        outmsg.Write("deny");

                                        //Sending the deny message
                                        Server.SendMessage(outmsg, incmsg.SenderConnection,
                                            NetDeliveryMethod.ReliableOrdered, 0);

                                        //a little pause the current process to make sure the message is sent to the client
                                        // before the server break down contact with the client. 
                                        System.Threading.Thread.Sleep(100);
                                        incmsg.SenderConnection.Disconnect("bye");
                                        playerRefresh =
                                            false; //Now the "if" its is True, we disable the playerRefhres bool
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
                                        0, "", isAuthoritative));
                                    Console.WriteLine(name + " connected.");
                                }

                                Console.WriteLine("Number of players: " + Player.players.Count);

                                #endregion
                            }
                                break;
                            default:
                            {
                                //Just ignore the message
                            }
                                break;
                        }
                    }
                        break;
                }

                Server.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }

            if (Player.players.Count < Program.NumPlayer)
                return;
            //Once every one is connected to the server
            for (var i = 0; i < Player.players.Count; i++)
            {
                Console.WriteLine("sending connect message to clients");

                outmsg = Server.CreateMessage();
                outmsg.Write("connect");
                outmsg.Write(Player.players[i].name);
                outmsg.Write((int) i);
                outmsg.Write((int) Program.NumPlayer);

                Server.SendMessage(Network.outmsg, Network.Server.Connections,
                    NetDeliveryMethod.ReliableOrdered, 0);
            }

            connectPhaseDone = true;
        }

        public static void playerSelectionPhase()
        {
       
            while ((incmsg = Server.ReadMessage()) != null) 
            {
                switch (incmsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                    {
                        string headStringMessage = incmsg.ReadString();

                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
                            case "charCursorPositionUpdate":
                            {
                                #region charCursorPositionUpdate

                                string name = incmsg.ReadString();
                                int x = incmsg.ReadInt32();
                                int y = incmsg.ReadInt32();

                                //Immediately relay this message to everyone
                                outmsg = Server.CreateMessage();

                                outmsg.Write("charCursorPositionUpdate");
                                outmsg.Write(name);
                                outmsg.Write((int) x);
                                outmsg.Write((int) y);

                                Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                    NetDeliveryMethod.Unreliable, 0);

                                #endregion
                            }
                                break;
                            case "charSelect":
                            {
                                #region charSelect

                                string name = incmsg.ReadString();
                                string spriteType = incmsg.ReadString();

                                // Update the player with the right sprite
                                foreach (var player in Player.players.Where(player => player.name.Equals(name)))
                                {
                                    player.spriteType = spriteType;
                                    player.timeOut = 0;
                                    numSpriteSelected++;
                                    break;
                                }

                                //Immediately relay this message to everyone
                                outmsg = Server.CreateMessage();

                                outmsg.Write("charSelect");
                                outmsg.Write(name);
                                outmsg.Write(spriteType);

                                Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                    NetDeliveryMethod.Unreliable, 0);

                                #endregion
                            }
                                break;
                            default:
                            {
                                //Just ignore the message
                            }
                                break;
                        }
                    }
                        break;
                }

                Server.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
            if (numSpriteSelected == Program.NumPlayer)
            {
                playerSelectionPhaseDone = true;
                outmsg = Server.CreateMessage();

                outmsg.Write("proceedToMapSelection");

                Server.SendMessage(Network.outmsg, Network.Server.Connections,
                    NetDeliveryMethod.ReliableOrdered, 0);

                foreach (var player in Player.players)
                {
                    Console.WriteLine(player.spriteType);
                }
            }
                
        }

        public static void mapSelectionPhase()
        {
            while ((incmsg = Server.ReadMessage()) != null) 
            {
                switch (incmsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                    {
                        string headStringMessage = incmsg.ReadString();

                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
                            case "mapSelect":
                            {
                                #region charSelect

                                if (!Map.isSet)
                                {
                                    try
                                    {
                                        string name = incmsg.ReadString();
                                        string mapType = incmsg.ReadString();

                                        // Update the player with the right sprite
                                        Map.isSet = true;
                                        if (mapType.Equals("mapRandom"))
                                        {
                                            Random randomNumGen = new Random();
                                            var mapNum = randomNumGen.Next(0, Map.NumRandomMap);
                                            Map.chosenMap = "mapRandom" + mapNum.ToString();
                                        }
                                        else
                                        {
                                            Map.chosenMap = mapType;
                                        }
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }

                                #endregion
                            }
                                break;
                            case "hasReceivedMap":
                            {
                                //Each player only send this one, so technically should not be a problem and don't have to name check
                                numPlayerReceivedMap++;
                            }        
                                break;
                            default:
                            {
                                //Just ignore the message
                            }
                                break;
                        }
                    }
                        break;
                }

                Server.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
            
            if (Map.isSet)
            {
                // Once the map is set, tell everyone
                outmsg = Server.CreateMessage();

                outmsg.Write("mapSelect");
                outmsg.Write(Map.chosenMap);

                Server.SendMessage(Network.outmsg, Network.Server.Connections,
                    NetDeliveryMethod.Unreliable, 0);
            }

            if (numPlayerReceivedMap == Program.NumPlayer)
            {
                // Now that everyone has their map sent, time to move on the next phase.
                mapSelectionPhaseDone = true;
            }
        }

        public static void singleGamePhase()
        {
            while ((incmsg = Server.ReadMessage()) != null) 
            {
                switch (incmsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                    {
                        string headStringMessage = incmsg.ReadString();

                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
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
                                            if (player.health >= health)
                                            {
                                                player.health = health; //only update if our health drops
                                            }

                                            player.timeOut = 0;
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
                            case "dealDamageToOther":
                            {
                                #region dealDamageToOther

                                try
                                {
                                    string personWhoShootName = incmsg.ReadString();
                                    string targetName = incmsg.ReadString();
                                    int targetNewHealth = incmsg.ReadInt32();
                                    foreach (var player in Player.players)
                                    {
                                        if (player.name.Equals(targetName))
                                        {
                                            if (player.health >= targetNewHealth)
                                            {
                                                player.health = targetNewHealth; //only update if our health drops
                                            }
                                            else
                                            {
                                                foreach (var shooter in Player.players)
                                                {
                                                    if (shooter.name.Equals(personWhoShootName) &&
                                                        shooter.isAuthoritative)
                                                    {
                                                        //if target mew health is higher, but the shooter is authoritative
                                                        player.health = targetNewHealth;
                                                        //TODO: Handle the case where no one is authoritative
                                                        break;
                                                    }
                                                }
                                                
                                            }

                                            player.timeOut = 0;
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
                            case "win":
                            {
                                #region charSelect

                                string name = incmsg.ReadString();
                                string spriteType = incmsg.ReadString();

                                // Update the player with the right sprite
                                foreach (var player in Player.players.Where(player => !player.name.Equals(name)))
                                {
                                    outmsg = Server.CreateMessage();

                                    outmsg.Write("lose");
                                    outmsg.Write(name);
                                    outmsg.Write(spriteType);

                                    Server.SendMessage(Network.outmsg, Network.Server.Connections, 
                                        NetDeliveryMethod.Unreliable, 0);

                                }

                                #endregion
                            }
                                break;
                            default:
                            {
                                //Just ignore the message
                            }
                                break;
                        }
                    }
                        break;
                }

                Server.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
            Player.Update();
        }

        public static void postSingleGamePhase()
        {
        }

        public static void gameOverPhase()
        {
        }

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
                                        outmsg.Write("deny");

                                        //Sending the deny message
                                        Server.SendMessage(outmsg, incmsg.SenderConnection,
                                            NetDeliveryMethod.ReliableOrdered, 0);

                                        //a little pause the current process to make sure the message is sent to the client
                                        // before the server break down contact with the client. 
                                        System.Threading.Thread.Sleep(100);
                                        incmsg.SenderConnection.Disconnect("bye");
                                        playerRefresh =
                                            false; //Now the "if" its is True, we disable the playerRefhres bool
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

                                // if (Map.isSet == false)
                                // {
                                //     string mapName = incmsg.ReadString();
                                //     Map.isSet = Int32.TryParse(mapName.Replace("map", ""), out Map.chosenMapNum);
                                //     //send it to everyone the first time it is set
                                //     // Write a new message with incoming parameters, and send the all connected clients.
                                //     outmsg = Server.CreateMessage();
                                //
                                //     outmsg.Write("mapSelect");
                                //     outmsg.Write("map" + Map.chosenMapNum);
                                //     Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                //         NetDeliveryMethod.ReliableOrdered, 0);
                                // }
                                // else
                                // {
                                //     //if someomne is trying to set the map again after the map is set, just send them the map
                                //     outmsg = Server.CreateMessage();
                                //
                                //     outmsg.Write("mapSelect");
                                //     outmsg.Write("map" + Map.chosenMapNum);
                                //     Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                //         NetDeliveryMethod.ReliableOrdered, 0);
                                // }

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
                                            if (player.health >= health)
                                            {
                                                player.health = health; //only update if our health drops
                                            }

                                            player.timeOut = 0;
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
                            case "dealDamageToOther":
                            {
                                #region dealDamageToOther

                                try
                                {
                                    string personWhoShootName = incmsg.ReadString();
                                    string targetName = incmsg.ReadString();
                                    int targetNewHealth = incmsg.ReadInt32();
                                    foreach (var player in Player.players)
                                    {
                                        if (player.name.Equals(targetName))
                                        {
                                            if (player.health >= targetNewHealth)
                                            {
                                                player.health = targetNewHealth; //only update if our health drops
                                            }
                                            else
                                            {
                                                foreach (var shooter in Player.players)
                                                {
                                                    if (shooter.name.Equals(personWhoShootName) &&
                                                        shooter.isAuthoritative)
                                                    {
                                                        //if target mew health is higher, but the shooter is authoritative
                                                        player.health = targetNewHealth;
                                                        //TODO: Handle the case where no one is authoritative
                                                        break;
                                                    }
                                                }
                                            }

                                            player.timeOut = 0;
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
                            default:
                            {
                                //Just ignore the message
                            }
                                break;
                        }
                    }
                        break;
                }

                Server.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
            Player.Update();
        }

        public static void Shutdown()
        {
            for (int i = 0; i < Player.players.Count; i++)
            {
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