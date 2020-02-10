using Lidgren.Network;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace NetworkingDemo
{
    // public class ServerModel
    // {
    //     private static NetServer server;
    //     private List<NetClient> clients;
    //     public static void Start()
    //     {
    //
    //         NetPeerConfiguration config = new NetPeerConfiguration("MyExampleName");
    //         config.Port = 14242;
    //
    //         server = new NetServer(config);
    //         server.Start();
    //     }
    //
    //     public static void ReceivingMsg()
    //     {
    //
    //
    //         NetIncomingMessage msg;
    //         while ((msg = server.ReadMessage()) != null)
    //         {
    //             switch (msg.MessageType)
    //             {
    //                 case NetIncomingMessageType.VerboseDebugMessage:
    //                 case NetIncomingMessageType.DebugMessage:
    //                 case NetIncomingMessageType.WarningMessage:
    //                 case NetIncomingMessageType.ErrorMessage:
    //                     Console.WriteLine(msg.ReadString());
    //                     break;
    //                 default:
    //                     Console.WriteLine("Unhandled type: " + msg.MessageType);
    //                     break;
    //             }
    //
    //             server.Recycle(msg);
    //         }
    //     }
    //     
    //     
    //     public static void SendMsg()
    //     {
    //         NetOutgoingMessage sendMsg = server.CreateMessage();
    //         sendMsg.Write("Hello from server");
    //         sendMsg.Write(42);
    //
    //         //server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
    //     }
    //
    //     static void RelayMsg()
    //     {
    //         
    //     }
    //
    //     public static void Shutdown()
    //     {
    //         server.Shutdown("Shutting down the server");
    //     }
    // }
    //
    public class ServerModel
    {
        public static void Start()
        {
            Network.Config =
                new NetPeerConfiguration(
                    "Weave"); // The server and the client program must also use this name, so that can communicate with each other.
            Network.Config.Port = 14242; //one port, if your PC it not using yet
            Network.Server = new NetServer(Network.Config);
            Network.Server.Start();

            Console.WriteLine("Server started!");
            Console.WriteLine("Waiting for connections...");
        }

        public static void Update() //updating the Network and the Player Method with timer1 (Tick interval 16 ≈ 60FPS)
        {
            System.Threading.Thread.Sleep(10);
            Network.Update();
            Player.Update();
        }

        public static void Shutdown()
        {
            Network.Shutdown();
        }
    }

    public class Network // A Basics Network class
    {
        public static NetServer Server; //the Server

        public static NetPeerConfiguration Config; //the Server config

        /*public*/
        static NetIncomingMessage incmsg; //the incoming messages that server can read from clients

        public static NetOutgoingMessage outmsg; //the outgoing messages that clients can receive and read

        /*public*/
        static bool playerRefhresh; //below for explanation...

        public static void Update()
        {
            while ((incmsg = Server.ReadMessage()) != null) //while the message is received, and is not equal to null...
            {
                switch (incmsg.MessageType) 
                    //There are several types of messages (see the Lidgren Basics tutorial), but it is easier to just use it the most important thing the "Data".
                {
                    case NetIncomingMessageType.Data:
                    {
                        //////////////////////////////////////////////////////////////
                        // You must create your own custom protocol with the        //
                        // server-client communication, and data transmission.      //
                        //////////////////////////////////////////////////////////////


                        // 1. step: The first data/message (string or int) tells the program to what is going on, that is what comes to doing.
                        // 2. step: The second tells by name (string) or id (int) which joined client(player) or object(bullets, or other dynamic items) to work.
                        // 3. step: The other data is the any some parameters you want to use, and this is setting and refhreshing the object old (player or item) state.

                        // Now this example I'm use to string (yes you can saving the bandwidth with all messages, if you use integer)
                        string headStringMessage = incmsg.ReadString(); //the first data (1. step)

                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
                            case "connect": //if the firs message/data is "connect"
                            {
                                Console.Write("connect message receive");
                                string
                                    name = incmsg
                                        .ReadString(); //Reading the 2. message who included the name (you can use integer, if you want to store the players in little data)
                                int x = incmsg.ReadInt32(); //Reading the x position
                                int y = incmsg.ReadInt32(); // -||- y postion

                                playerRefhresh = true; //just setting this "True"
                                
                                Console.WriteLine("Checking for duplicate");
                                // Now check to see if you have at least one of our players, the subsequent attempts to connect with the same name. 
                                for (int i = 0; i < Player.players.Count; i++)
                                {
                                    if (Player.players[i].name.Equals(name)) //If its is True...
                                    {
                                        outmsg = Server.CreateMessage(); //The Server creating a new outgoing message
                                        outmsg.Write( "deny"); //and this moment writing this to one message "deny" (the rest of the ClientAplication in interpreting)

                                        //Sending the message
                                        //parameters:
                                        //1. the message which we have written
                                        //2. whom we send (Now just only the person who sent the message to the server)
                                        //3. delivery reliability (Since this is an important message, so be sure to be delivered)
                                        //4. The channel on which the message is sent (I do not deal with it, just give the default value)
                                        Server.SendMessage(outmsg, incmsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                                        System.Threading.Thread
                                            .Sleep(
                                                100); //a little pause the current process to make sure the message is sent to the client before they break down in contact with him.
                                        incmsg.SenderConnection.Disconnect(  "bye"); //ends the connection with the client who sent the message, and you can writing the any string message if you want
                                        playerRefhresh =
                                            false; //Now the "if" its is True, we disable the playerRefhres bool
                                        break;
                                    }
                                }

                                // but if the above check is false, then the following happens:
                                if (playerRefhresh == true)
                                {
                                    Console.WriteLine("Now attempting to connect");
                                    System.Threading.Thread.Sleep(50); //A little pause to make sure you connect the client before performing further operations
                                    Player.players.Add(new Player(name, new Vector2(x, y),
                                        0)); //Add to player messages received as a parameter
                                    Console.WriteLine(name + " connected.");

                                    for (int i = 0; i < Player.players.Count; i++)
                                    {
                                            Console.WriteLine("sending " + name + " to " + Player.players[i].name);
                                            // Write a new message with incoming parameters, and send the all connected clients.
                                            outmsg = Server.CreateMessage();

                                            outmsg.Write("connect");
                                            outmsg.Write(Player.players[i].name);
                                            outmsg.Write((int) Player.players[i].pozition.X);
                                            outmsg.Write((int) Player.players[i].pozition.Y);

                                            Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                                NetDeliveryMethod.ReliableOrdered, 0);
                                    }
                                }

                                Console.WriteLine("Players: " + Player.players.Count);
                            }
                                break;

                            case "move": //The moving messages
                            {
                                //This message is treated as plain UDP (NetDeliveryMethod.Unreliable)
                                //The motion is not required to get clients in every FPS.
                                //The exception handling is required if the message can not be delivered in full, 
                                //just piece, so this time the program does not freeze.
                                try
                                {
                                    string name = incmsg.ReadString();
                                    int x = incmsg.ReadInt32();
                                    int y = incmsg.ReadInt32();
                                    int deltaX = incmsg.ReadInt32();
                                    int deltaY = incmsg.ReadInt32();
                                    bool fired = incmsg.ReadBoolean();
                                    for (int i = 0; i < Player.players.Count; i++)
                                    {
                                        if (Player.players[i].name.Equals(name))
                                        {
                                            Player.players[i].pozition = new Vector2(x, y);
                                            Player.players[i].velocity = new Vector2(deltaX, deltaY);
                                            Player.players[i].fired = fired;
                                            Player.players[i].timeOut = 0; //below for explanation (Player class)...
                                            break;
                                        }
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                                break;

                            case "disconnect": //If the client want to disconnect from server at manually
                            {
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

    class Player //The Player class and instant constructor
    {
        public string name;
        public Vector2 pozition;
        public Vector2 velocity;
        public bool fired = false;

        public int
            timeOut; //This disconnects the client, even if no message from him within a certain period of time and not been reset value.

        public static List<Player> players = new List<Player>();

        public Player(string name,
            Vector2 pozition,
            int timeOut)
        {
            this.name = name;
            this.pozition = pozition;
            this.velocity = Vector2.Zero;
            this.timeOut = timeOut;
        }

        public static void Update()
        {
            if (Network.Server.ConnectionsCount == players.Count)
                //If the number of the player object actually corresponds to the number of connected clients.
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].timeOut++; //This data member continuously counts up with every frame/tick.

                    //The server simply always sends data to the all players current position of all clients.
                    Network.outmsg = Network.Server.CreateMessage();

                    Network.outmsg.Write("move");
                    Network.outmsg.Write(players[i].name);
                    Network.outmsg.Write((int) players[i].pozition.X);
                    Network.outmsg.Write((int) players[i].pozition.Y);
                    Network.outmsg.Write((int) players[i].velocity.X);
                    Network.outmsg.Write((int) players[i].velocity.Y);
                    Network.outmsg.Write((bool) players[i].fired);

                    Network.Server.SendMessage(Network.outmsg, Network.Server.Connections, NetDeliveryMethod.Unreliable,
                        0);

                    if (players[i].timeOut > 600000) //If this is true, so that is the player not sent information with himself
                    {
                        //The procedure will be the same as the above when "disconnect" message
                        Network.Server.Connections[i].Disconnect("bye");
                        Console.WriteLine(players[i].name + " is timed out.");
                        System.Threading.Thread.Sleep(100);

                        if (Network.Server.ConnectionsCount != 0)
                        {
                            Network.outmsg = Network.Server.CreateMessage();

                            Network.outmsg.Write("disconnect");
                            Network.outmsg.Write(players[i].name);

                            Network.Server.SendMessage(Network.outmsg, Network.Server.Connections,
                                NetDeliveryMethod.ReliableOrdered, 0);
                        }

                        players.RemoveAt(i);
                        i--;
                        Console.WriteLine("Players: " + players.Count);
                        break;
                    }
                }
            }
        }
    }
}