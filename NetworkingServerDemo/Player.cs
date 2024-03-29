using System;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace NetworkingDemo
{ 
    class Player //The Player class and instant constructor
    {
        public static int maxNumPlayer = 4;
        public string name;
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 projectileDir;
        public bool fired = false;
        public int killCounts = 0;
        public int projectileType = 1;
        public string spriteType;
        public int mana = 5;
        public bool isAuthoritative = false;

        private static int netId = 0;
        public int
            timeOut; //This disconnects the client, even if no message from him within a certain period of time and not been reset value.

        public static List<Player> players = new List<Player>();

        public Player(string name,
            Vector2 position,
            int timeOut, 
            string spriteType,
            bool isAuthoritative)
        {
            this.name = name;
            this.position = position;
            this.velocity = Vector2.Zero;
            this.timeOut = timeOut;
            this.spriteType = spriteType;
            this.isAuthoritative = isAuthoritative;
            this.projectileDir = new Vector2(0,0);
            this.projectileType = 1;
        }

        public static void ResetForSinglePlayerGamePhase()
        {
            foreach (var player in players)
            {
                player.position = Vector2.Zero;
                player.velocity = Vector2.Zero;
                
                player.projectileDir = Vector2.Zero;
                player.fired = false;
                player.killCounts = 0;
                player.projectileType = 1;
                player.mana = 5;
            }
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
                    Network.outmsg.Write(players[i].position.X);
                    Network.outmsg.Write(players[i].position.Y);
                    Network.outmsg.Write( players[i].velocity.X);
                    Network.outmsg.Write( players[i].velocity.Y);
                    Network.outmsg.Write((bool) players[i].fired);
                    Network.outmsg.Write((int) players[i].projectileType);
                    Network.outmsg.Write(players[i].projectileDir.X);
                    Network.outmsg.Write(players[i].projectileDir.Y);
                    Network.outmsg.Write((int) players[i].killCounts);
                    Network.outmsg.Write((int) players[i].mana);
                    
                    // Console.WriteLine(netId);
                    Network.outmsg.Write((int) netId);
                    netId++;

                    Network.Server.SendMessage(Network.outmsg, Network.Server.Connections, NetDeliveryMethod.Unreliable,
                        0);

                    // if (players[i].timeOut > 600000) //If this is true, so that is the player not sent information with himself
                    // {
                    //     //The procedure will be the same as the above when "disconnect" message
                    //     Network.Server.Connections[i].Disconnect("bye");
                    //     Console.WriteLine(players[i].name + " is timed out.");
                    //     System.Threading.Thread.Sleep(100);
                    //
                    //     if (Network.Server.ConnectionsCount != 0)
                    //     {
                    //         Network.outmsg = Network.Server.CreateMessage();
                    //
                    //         Network.outmsg.Write("disconnect");
                    //         Network.outmsg.Write(players[i].name);
                    //
                    //         Network.Server.SendMessage(Network.outmsg, Network.Server.Connections,
                    //             NetDeliveryMethod.ReliableOrdered, 0);
                    //     }
                    //
                    //     players.RemoveAt(i);
                    //     i--;
                    //     Console.WriteLine("Players: " + players.Count);
                    //     break;
                    // }
                    System.Threading.Thread.Sleep(1);
                }
            }
        }
    }
 }
  