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

        public override void OnEnabled()
        {
            Network.Config = new NetPeerConfiguration("Weave"); //Same as the Server, so the same name to be used.
            Network.Client = new NetClient(Network.Config);

            Network.Client.Start(); //Starting the Network Client
            System.Console.WriteLine("Within intialize" + LoginScene. _serverIp);
            Network.Client.Connect(LoginScene._serverIp, 14242); //And Connect the Server with IP (string) and host (int) parameters

            //The causes are shown below pause for a bit longer. 
            //On the client side can be a little time to properly connect to the server before the first message you send us. 
            //The second one is also a reason. The client does not manually force the quick exit until it received a first message from the server. 
            //If the client connect to trying one with the same name as that already exists on the server, 
            //and you attempt to exit Esc-you do not even arrived yet reject response ("deny"), the underlying visible event is used, 
            //so you can disconnect from the other player from the server because the name he applied for the existing exit button. 
            //Therefore, this must be some pause. 

            System.Threading.Thread.Sleep(300);
            // Console.WriteLine("Sending connect message");
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("connect");
            Network.outmsg.Write(LoginScene._playerName);
            Network.outmsg.Write(48);
            Network.outmsg.Write(240);
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
                                                System.Console.WriteLine("FOund duplicate: " );
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