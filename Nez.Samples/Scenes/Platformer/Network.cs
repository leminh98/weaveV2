using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Nez.Samples
{
    public class Network: SceneComponent, IUpdatable
    {
        public static NetClient Client;

        private static NetPeerConfiguration Config;

        /*public*/
        static NetIncomingMessage incmsg;
        public static NetOutgoingMessage outmsg;

        public static void Initialize(string playerName, string serverIp, string spriteName)
        {
            Network.Config = new NetPeerConfiguration("Weave"); //Same as the Server, so the same name to be used.
            Network.Client = new NetClient(Network.Config);

            Network.Client.Start(); //Starting the Network Client
            System.Console.WriteLine("Within initialize " + LoginScene. _serverIp);
            Network.Client.Connect(serverIp, 14242); //And Connect the Server with IP (string) and host (int) parameters

            //Sleep a little bit to guaranteed being connect
            System.Threading.Thread.Sleep(300);
            
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("connect");
            Network.outmsg.Write(playerName);
            Network.outmsg.Write(spriteName);
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
        }
        public override void OnEnabled()
        {
            var spawnPos = Scene.FindEntity("player").Position;
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("startGame");
            Network.outmsg.Write(LoginScene._playerName);
            Network.outmsg.Write(spawnPos.X);
            Network.outmsg.Write(spawnPos.Y);
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
        }


        public override void OnDisabled()
        {
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("disconnect");
            Network.outmsg.Write(LoginScene._playerName);
        }
        


        public override void OnRemovedFromScene()
        {
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("disconnect");
            Network.outmsg.Write(LoginScene._playerName);
        }


        public override void Update()
        {
            //The biggest difference is that the client side of things easier, 
            //since we will only consider the amount of player object is created, 
            //so there is no keeping track of separate "Server.Connections" as the server side.
            while ((incmsg = Client.ReadMessage()) != null)
            {
                switch (incmsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                    {
                        string headStringMessage = incmsg.ReadString();

                        switch (headStringMessage)
                        {
                            case "connect":
                                goto case "startGame"   //startGame and connect is the same
;                           case "startGame":
                            {
                                string name = incmsg.ReadString();
                                int x = incmsg.ReadInt32();
                                int y = incmsg.ReadInt32();
            
                                bool duplicate = false;
                                
                                if (name.Equals(LoginScene._playerName))
                                {
                                    duplicate = true; ; //make sure it's not duplicating our name 
                                }
                                else
                                {
                                    // Resolve duplicate by first adding it to the players list and then remove any duplication
                                    OtherPlayer.players.Add(name);
                                    for (int i1 = 0; i1 < OtherPlayer.players.Count; i1++)
                                    {
                                        for (int i2 = /*0*/i1 + 1; i2 < OtherPlayer.players.Count; i2++)
                                        {
                                            if (i1 != i2 && OtherPlayer.players[i1].Equals(OtherPlayer.players[i2]))
                                            {
                                                OtherPlayer.players.RemoveAt(i1);
                                                i1--;
                                                duplicate = true;
                                                System.Console.WriteLine("Found duplicate: " );
                                                break;
                                            }
                                        }
                                    }
                                }
                                
                                if (!duplicate)
                                {
                                    System.Console.WriteLine("Creating other player: " + name);
                                    var platformerScene = Scene as PlatformerScene;
                                    platformerScene.CreateNewPlayer(name, new Vector2(x, y));
                                }
                               
                            }
                                break;

                            case "move":
                            {
                                try
                                {
                                    // System.Console.WriteLine("recieve a move message");
                                    string name = incmsg.ReadString();
                                    int x = incmsg.ReadInt32();
                                    int y = incmsg.ReadInt32();
                                    int deltaX = incmsg.ReadInt32();
                                    int deltaY = incmsg.ReadInt32();
                                    bool fired = incmsg.ReadBoolean();
                                    int health = incmsg.ReadInt32();
                                    
                                    System.Console.WriteLine("recieve a move message");
                                    System.Console.WriteLine(OtherPlayer.players.Count);
                                    // System.Threading.Thread.Sleep(300);
                                    for (int i = 0; i < OtherPlayer.players.Count; i++)
                                    {
                                        //It is important that you only set the value of the player, if it is not yours, 
                                        //otherwise it would cause lagg (because you'll always be first with yours, and there is a slight delay from server-client).
                                        //Of course, sometimes have to force the server to the actual position of the player, otherwise could easily cheat.
                                        if (OtherPlayer.players[i].Equals(name) && (!OtherPlayer.players[i].Equals(LoginScene._playerName))) 
                                        {
                                            System.Console.WriteLine("Updating player: " + name);
                                            var platformerScene = Scene as PlatformerScene;
                                            platformerScene.UpdateOtherPlayerMovement(name, new Vector2(x, y), 
                                                new Vector2(deltaX, deltaY), fired, health);
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

                            case "disconnect": //Clear enough :)
                            {
                                string name = incmsg.ReadString();

                                for (int i = 0; i < OtherPlayer.players.Count; i++)
                                {
                                    if (OtherPlayer.players[i].Equals(name))
                                    {
                                        OtherPlayer.players.RemoveAt(i);
                                        //TODO: REMOVE THE PLAYER FROM THE ENTITY
                                        i--;
                                        break;
                                    }
                                }
                            }
                                break;

                            case "deny": //If the name on the message is the same as ours
                            {
                                // PlatformerScene.HeadText = "This name is already taken:";
                                // Weave.TextCanWrite = true;
                                OtherPlayer.players.Clear();
                            }
                                break;
                        }
                    }
                        break;
                }

                Client.Recycle(incmsg);
            }
        }
    }
}