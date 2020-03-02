using System;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Nez.Samples.Scenes.CharacterSelection;
using Nez.Samples.Scenes.EndGame;
using Nez.Samples.Scenes.Intro;
using Nez.Tweens;

namespace Nez.Samples
{
    public class Network : GlobalManager, IUpdatable
    {
        public static NetClient Client;
        private static NetPeerConfiguration Config;
        static NetIncomingMessage incmsg;
        public static NetOutgoingMessage outmsg;
        
        public static bool connectPhaseDone = false;
        public static bool playerSelectionPhaseDone = false;
        public static bool mapSelectionPhaseDone = false;
        public static bool singleGamePhaseDone = false;
        public static bool postSingleGamePhaseDone = false;
        public static bool gameOver = false;

        public void Start()
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
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
        }
        
        /**
         * Connection phase: only receive "coonect" or  "deny" message
         */
        public static void connectionPhase()
        {
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
                                #region connect

                                string name = incmsg.ReadString();
                                int playerIndex = incmsg.ReadInt32();
                                int numPlayer = incmsg.ReadInt32();
                                LoginScene.numPlayer = numPlayer;
                                
                                // bool duplicate = false;
                                //
                                // if (name.Equals(LoginScene._playerName))
                                // {
                                //     duplicate = true;
                                //     //make sure it's not duplicating our name 
                                // }
                                // else
                                // {
                                //     // Resolve duplicate by first adding it to the players list and then remove any duplication
                                OtherPlayer.players.Add(new OtherPlayerListItem(name, playerIndex));
                                if (name.Equals(LoginScene._playerName))
                                    LoginScene.playerIndex = playerIndex;
                                //     for (int i1 = 0; i1 < OtherPlayer.players.Count; i1++)
                                //     {
                                //         for (int i2 = /*0*/i1 + 1; i2 < OtherPlayer.players.Count; i2++)
                                //         {
                                //             if (i1 != i2 && OtherPlayer.players[i1].name.Equals(OtherPlayer.players[i2].name))
                                //             {
                                //                 OtherPlayer.players.RemoveAt(i1);
                                //                 i1--;
                                //                 duplicate = true;
                                //                 System.Console.WriteLine("Found duplicate: ");
                                //                 break;
                                //             }
                                //         }
                                //     }
                                // }


                                #endregion
                            }
                                break;
                            case "deny": //If the name on the message is the same as ours
                            {
                                #region deny
                                //TODO: Decide what to do in the case of a deny message
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

        public static void playerSelectionPhase()
        {
            while ((incmsg = Client.ReadMessage()) != null) 
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

                                try
                                {

                                    string name = incmsg.ReadString();
                                    int x = incmsg.ReadInt32();
                                    int y = incmsg.ReadInt32();

                                    if (name.Equals(LoginScene._playerName)) //this is just ourself, skip
                                        continue;
                                    
                                    foreach (var cursor in OtherPlayer.players.Where(cursor =>
                                        cursor.name.Equals(name)))
                                    {
                                        // System.Console.WriteLine("FSASDr");
                                        var characterSelectionScene = Core.Scene as CharacterSelectionScene;
                                        var cursorEntity = characterSelectionScene.FindEntity("charCursor_" + name);
                                        cursorEntity.GetComponent<OtherCharacterSelectionCursor>().Update(new Vector2(x, y));
                                    }
                                }
                                catch
                                {
                                    continue;
                                }

                                #endregion
                            }
                                break;
                            case "charSelect":
                            {
                                #region charSelect

                                string name = incmsg.ReadString();
                                string spriteType = incmsg.ReadString();

                                if (name.Equals(LoginScene._playerName)) //this is just ourself, skip
                                    continue;

                                for (int i = 0; i < OtherPlayer.players.Count; i++)
                                {
                                    // var otherPlayerStruct = OtherPlayer.players[i];
                                    if (OtherPlayer.players[i].name.Equals(name))
                                    {
                                        var characterSelectionScene = Core.Scene as CharacterSelectionScene;
                                        var cursorEntity = characterSelectionScene.FindEntity("charCursor_" + name);
                                        OtherPlayer.players[i].playerSprite = spriteType;
                                        cursorEntity.GetComponent<OtherCharacterSelectionCursor>().DisableCharacterSelectionForSprite(spriteType);
                                    }
                                }
                                
                                #endregion
                            }
                                break;
                            case "proceedToMapSelection":
                            {
                                var characterSelectionScene = Core.Scene as CharacterSelectionScene;
                                characterSelectionScene.continueButton.SetDisabled(false);
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

                Client.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
        }

        public static void mapSelectionPhase()
        {
            while ((incmsg = Client.ReadMessage()) != null) 
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
                                if (!MapSelectionScene.mapSelected)
                                {
                                    string mapName = incmsg.ReadString();
                                    MapSelectionScene.chosenMap = mapName;
                                    MapSelectionScene.mapSelected = true;
                                    
                                    Network.outmsg = Network.Client.CreateMessage();
                                    Network.outmsg.Write("hasReceivedMap");
                                    //The server has to received this
                                    Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
                                    mapSelectionPhaseDone = true;
                                    
                                    TweenManager.StopAllTweens();
                                    Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(PlatformerScene)) as Scene));
                                }
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

                Client.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
        }
        
        public static void singleGamePhase()
        {
            while ((incmsg = Client.ReadMessage()) != null) 
            {
                switch (incmsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                    {
                        string headStringMessage = incmsg.ReadString();

                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
                             case "move":
                                {
                                    #region player move message
                                    try
                                    {
                                        // System.Console.WriteLine("recieve a move message");
                                        string name = incmsg.ReadString();
                                        float x = incmsg.ReadFloat();
                                        float y = incmsg.ReadFloat();
                                        float deltaX = incmsg.ReadFloat();
                                        float deltaY = incmsg.ReadFloat();
                                        bool fired = incmsg.ReadBoolean();
                                        int projType = incmsg.ReadInt32();
                                        float projX = incmsg.ReadFloat();
                                        float projY = incmsg.ReadFloat();
                                        int killCount = incmsg.ReadInt32();
                                        // if (fired)
                                        //     System.Console.WriteLine(projX + " " + projY);
                                
                                        if (LoginScene._playerName.Equals(name))
                                        {
                                            // var platformerScene = Core.Scene as PlatformerScene;
                                            // platformerScene.UpdatePlayerHealth(health);
                                        }
                                        else
                                        {
                                            foreach (var player in OtherPlayer.players)
                                            {
                                                if (!player.name.Equals(
                                                            name) || (player.name.Equals(LoginScene._playerName)))
                                                    continue;
                                                // System.Console.WriteLine(name);
                                                var platformerScene = Core.Scene as PlatformerScene;
                                                platformerScene.UpdateOtherPlayerMovement(name, new Vector2(x, y),
                                                    new Vector2(deltaX, deltaY), fired, projType, new Vector2(projX, projY),killCount);
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
                             case "lose":
                             {
                                 string playerName = incmsg.ReadString();
                                 System.Console.WriteLine("receive a lose message");
                                 if (playerName.Equals(LoginScene._playerName))
                                 {
                                     singleGamePhaseDone = true;
                                     TweenManager.StopAllTweens();
                                     Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(LoseScene)) as Scene));
                                 }

                             }
                                 break;
                             case "win":
                             {
                                 singleGamePhaseDone = true;
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

                Client.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
        }

        public static void postSingleGamePhase()
        {
            while ((incmsg = Client.ReadMessage()) != null) 
            {
                switch (incmsg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                    {
                        string headStringMessage = incmsg.ReadString();

                        switch (headStringMessage) //and I'm think this is can easyli check what comes to doing
                        {
                            case "restart":
                            {
                                System.Console.WriteLine("receive restart messgae");
                                ResetMapSelectionPhase();
                                ResetSingleGamePhase();
                                ResetPostSingleGamePhase();
                                TweenManager.StopAllTweens();
                                Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(MapSelectionScene)) as Scene));
                           
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

                Client.Recycle(incmsg); //All messages processed at the end of the case, delete the contents.
            }
        }

        public static void gameOverPhase()
        {
            
        }

        public override void OnEnabled()
        {
            
        }


        public override void OnDisabled()
        {
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("disconnect");
            Network.outmsg.Write(LoginScene._playerName);
        }


        public int UpdateOrder { get; }

        private static void ResetMapSelectionPhase()
        {
            MapSelectionScene.mapSelected = false;
            MapCursor.hasSentMap = false;
            mapSelectionPhaseDone = false;
        }
        
        private static void ResetSingleGamePhase()
        {
            singleGamePhaseDone = false;
        }

        

        private static void ResetPostSingleGamePhase()
        {
            postSingleGamePhaseDone = false;
        }
        
        public override void Update()
        {
            if (!Network.connectPhaseDone)
            {
                connectionPhase();
                if (OtherPlayer.players.Count == LoginScene.numPlayer)
                {
                    connectPhaseDone = true;
                    //Enable the proceed button in Instruction scene,
                    // the connect phase done boolean will be set during the transition
                    var instructionScene = Core.Scene as InstructionScene;
                    instructionScene.button.SetDisabled(false);
                }
                return;
            }

            if (!Network.playerSelectionPhaseDone)
            {
                playerSelectionPhase();
                return;
            }

            if (!Network.mapSelectionPhaseDone)
            {
                System.Console.WriteLine("in map selection phase");
                mapSelectionPhase();
                return;
            }

            if (!Network.singleGamePhaseDone)
            {
                singleGamePhase();
                return;
            }

            if (!Network.postSingleGamePhaseDone)
            {
                postSingleGamePhase();
                return;
            }

            if (!Network.gameOver)
            {
                return;
            }
            
            //
            // while ((incmsg = Client.ReadMessage()) != null)
            // {
            //     switch (incmsg.MessageType)
            //     {
            //         case NetIncomingMessageType.Data:
            //         {
            //             string headStringMessage = incmsg.ReadString();
            //             // System.Console.WriteLine("recieve message: " + headStringMessage);
            //             switch (headStringMessage)
            //             {
            //                 case "connect":
            //                 {
            //                     #region connect
            //
            //                     string name = incmsg.ReadString();
            //                     string spriteType = incmsg.ReadString(); 
            //                     
            //                     bool duplicate = false;
            //
            //                     if (name.Equals(LoginScene._playerName))
            //                     {
            //                         duplicate = true;
            //                         //make sure it's not duplicating our name 
            //                     }
            //                     else
            //                     {
            //                         // Resolve duplicate by first adding it to the players list and then remove any duplication
            //                         OtherPlayer.players.Add(new Tuple<string, string>(name, spriteType));
            //                         for (int i1 = 0; i1 < OtherPlayer.players.Count; i1++)
            //                         {
            //                             for (int i2 = /*0*/i1 + 1; i2 < OtherPlayer.players.Count; i2++)
            //                             {
            //                                 if (i1 != i2 && OtherPlayer.players[i1].Equals(OtherPlayer.players[i2]))
            //                                 {
            //                                     OtherPlayer.players.RemoveAt(i1);
            //                                     i1--;
            //                                     duplicate = true;
            //                                     System.Console.WriteLine("Found duplicate: ");
            //                                     break;
            //                                 }
            //                             }
            //                         }
            //                     }
            //
            //                     #endregion
            //                 }
            //                     break;
            //                 case "startGame":
            //                 {
            //                     #region startGame
            //
            //                     string name = incmsg.ReadString();
            //                     int x = incmsg.ReadInt32();
            //                     int y = incmsg.ReadInt32();
            //
            //                     for (int i = 0; i < OtherPlayer.players.Count; i++)
            //                     {
            //                         //.name is name, Item2 is spriteType 
            //                         if (OtherPlayer.players[i].name
            //                                 .Equals(
            //                                     name) && (!OtherPlayer.players[i].name.Equals(LoginScene._playerName)))
            //                         {
            //                             System.Console.WriteLine("Creating other player: " + name);
            //                             var platformerScene = Core.Scene as PlatformerScene;
            //                             platformerScene.CreateNewPlayer(name, OtherPlayer.players[i].Item2, new Vector2(x, y));
            //                         }
            //                     }
            //
            //                     #endregion
            //                 }
            //                     break;
            //                 case "move":
            //                 {
            //                     #region player move message
            //                     try
            //                     {
            //                         // System.Console.WriteLine("recieve a move message");
            //                         string name = incmsg.ReadString();
            //                         int x = incmsg.ReadInt32();
            //                         int y = incmsg.ReadInt32();
            //                         int deltaX = incmsg.ReadInt32();
            //                         int deltaY = incmsg.ReadInt32();
            //                         bool fired = incmsg.ReadBoolean();
            //                         int health = incmsg.ReadInt32();
            //
            //                         if (LoginScene._playerName.Equals(name))
            //                         {
            //                             var platformerScene = Core.Scene as PlatformerScene;
            //                             platformerScene.UpdatePlayerHealth(health);
            //                         }
            //                         else
            //                         {
            //                             foreach (var player in OtherPlayer.players)
            //                             {
            //                                 //It is important that you only set the value of the player, if it is not yours, 
            //                                 //otherwise it would cause lagg (because you'll always be first with yours, and there is a slight delay from server-client).
            //                                 //Of course, sometimes have to force the server to the actual position of the player, otherwise could easily cheat.
            //                                 if (!player.name
            //                                         .Equals(
            //                                             name) || (player.name.Equals(LoginScene._playerName)))
            //                                     continue;
            //                                 System.Console.WriteLine(name);
            //                                 var platformerScene = Core.Scene as PlatformerScene;
            //                                 platformerScene.UpdateOtherPlayerMovement(name, new Vector2(x, y),
            //                                     new Vector2(deltaX, deltaY), fired, health);
            //                                 break;
            //                             }
            //                         }
            //                         
            //                     }
            //                     catch
            //                     {
            //                         continue;
            //                     }
            //                     #endregion
            //                 }
            //                     break;
            //                 case "mapSelect":
            //                 {
            //                     #region mapSelect
            //                     if (!MapSelectionScene.mapSelected)
            //                     {
            //                         string mapName = incmsg.ReadString();
            //                         MapSelectionScene.chosenMap = mapName;
            //                         MapSelectionScene.mapSelected = true;
            //                         TweenManager.StopAllTweens();
            //                         Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(PlatformerScene)) as Scene));
            //                     }
            //                     #endregion
            //                 }
            //                     break;
            //                 case "disconnect": //Clear enough :)
            //                 {
            //                     #region player disconnect message
            //                     string name = incmsg.ReadString();
            //
            //                     for (int i = 0; i < OtherPlayer.players.Count; i++)
            //                     {
            //                         if (OtherPlayer.players[i].name.Equals(name))
            //                         {
            //                             OtherPlayer.players.RemoveAt(i);
            //                             //TODO: REMOVE THE PLAYER FROM THE ENTITY
            //                             i--;
            //                             break;
            //                         }
            //                     }
            //                     #endregion
            //                 }
            //                     break;
            //                 case "deny": //If the name on the message is the same as ours
            //                 {
            //                     #region deny
            //                     // PlatformerScene.HeadText = "This name is already taken:";
            //                     // Weave.TextCanWrite = true;
            //                     OtherPlayer.players.Clear();
            //                     #endregion
            //                 }
            //                     break;
            //             }
            //         }
            //             break;
            //     }
            //
            //     Client.Recycle(incmsg);
            // }
        }
    }
}