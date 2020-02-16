using System;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Nez.Tweens;

namespace Nez.Samples
{
    public class Network : GlobalManager, IUpdatable
    {
        public static NetClient Client;

        private static NetPeerConfiguration Config;

        /*public*/
        static NetIncomingMessage incmsg;
        public static NetOutgoingMessage outmsg;

        public void InitializeGameplay()
        {
            // var spawnPos = Core.Scene.FindEntity("player").Position;
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("startGame");
            Network.outmsg.Write(LoginScene._playerName);
            Network.outmsg.Write(50);
            Network.outmsg.Write(50);
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
            System.Threading.Thread.Sleep(50);
        }

        public override void OnEnabled()
        {
            Network.Config = new NetPeerConfiguration("Weave"); //Same as the Server, so the same name to be used.
            Network.Client = new NetClient(Network.Config);

            Network.Client.Start(); //Starting the Network Client
            System.Console.WriteLine("Within initialize " + LoginScene._serverIp);
            Network.Client.Connect(LoginScene._serverIp,
                14242); //And Connect the Server with IP (string) and host (int) parameters

            //Sleep a little bit to guaranteed being connect
            System.Threading.Thread.Sleep(300);

            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("connect");
            Network.outmsg.Write(LoginScene._playerName);
            Network.outmsg.Write(LoginScene._characterSpriteType);
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
        }


        public override void OnDisabled()
        {
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("disconnect");
            Network.outmsg.Write(LoginScene._playerName);
        }


        public int UpdateOrder { get; }

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
                        // System.Console.WriteLine("recieve message: " + headStringMessage);
                        switch (headStringMessage)
                        {
                            case "connect":
                            {
                                #region connect

                                string name = incmsg.ReadString();
                                string spriteType = incmsg.ReadString(); //TODO: CHANGE SPRITE
                                
                                bool duplicate = false;

                                if (name.Equals(LoginScene._playerName))
                                {
                                    duplicate = true;
                                    ; //make sure it's not duplicating our name 
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
                                                System.Console.WriteLine("Found duplicate: ");
                                                break;
                                            }
                                        }
                                    }
                                }

                                #endregion
                            }
                                break;
                            case "startGame":
                            {
                                #region startGame

                                string name = incmsg.ReadString();
                                int x = incmsg.ReadInt32();
                                int y = incmsg.ReadInt32();

                                for (int i = 0; i < OtherPlayer.players.Count; i++)
                                {
                                    //It is important that you only set the value of the player, if it is not yours, 
                                    //otherwise it would cause lagg (because you'll always be first with yours, and there is a slight delay from server-client).
                                    //Of course, sometimes have to force the server to the actual position of the player, otherwise could easily cheat.
                                    if (OtherPlayer.players[i]
                                            .Equals(
                                                name) && (!OtherPlayer.players[i].Equals(LoginScene._playerName)))
                                    {
                                        System.Console.WriteLine("Creating other player: " + name);
                                        var platformerScene = Core.Scene as PlatformerScene;
                                        platformerScene.CreateNewPlayer(name, new Vector2(x, y));
                                    }
                                }

                                #endregion
                            }
                                break;
                            case "move":
                            {
                                #region player move message
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

                                    // System.Console.WriteLine("recieve a move message");
                                    // System.Console.WriteLine(OtherPlayer.players.Count);
                                    // System.Threading.Thread.Sleep(300);
                                    for (int i = 0; i < OtherPlayer.players.Count; i++)
                                    {
                                        //It is important that you only set the value of the player, if it is not yours, 
                                        //otherwise it would cause lagg (because you'll always be first with yours, and there is a slight delay from server-client).
                                        //Of course, sometimes have to force the server to the actual position of the player, otherwise could easily cheat.
                                        if (OtherPlayer.players[i]
                                            .Equals(
                                                name) && (!OtherPlayer.players[i].Equals(LoginScene._playerName)))
                                        {
                                            // System.Console.WriteLine("Updating player: " + name);
                                            var platformerScene = Core.Scene as PlatformerScene;
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
                                #endregion
                            }
                                break;
                            case "mapSelect":
                            {
                                #region mapSelect
                                if (!MapSelectionScene.mapSelected)
                                {
                                    string mapName = incmsg.ReadString();
                                    MapSelectionScene.chosenMap = mapName;
                                    MapSelectionScene.mapSelected = true;
                                    TweenManager.StopAllTweens();
                                    Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(PlatformerScene)) as Scene));
                                    
                                }
                                #endregion
                            }
                                break;
                            case "disconnect": //Clear enough :)
                            {
                                #region player disconnect message
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
                                #endregion
                            }
                                break;
                            case "deny": //If the name on the message is the same as ours
                            {
                                #region deny
                                // PlatformerScene.HeadText = "This name is already taken:";
                                // Weave.TextCanWrite = true;
                                OtherPlayer.players.Clear();
                                #endregion
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