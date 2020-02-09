using Lidgren.Network;
using Microsoft.Xna.Framework;
using Nez.Tiled;

namespace Nez.Samples
{
    public class Network
    {
        public static NetClient Client;

        public static NetPeerConfiguration Config;

        /*public*/
        static NetIncomingMessage incmsg;
        public static NetOutgoingMessage outmsg;

        public static void Update()
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

                                //Another way to filter out the players with the same name, 
                                //where the first step in any case is added to the player, 
                                //and then check the second round with a double for loop that is in agreement with two players.

                                Caveman.players.Add(new Caveman(name));

                                for (int i1 = 0; i1 < Caveman.players.Count; i1++)
                                {
                                    for (int i2 = /*0*/i1 + 1; i2 < Caveman.players.Count; i2++)
                                    {
                                        if (i1 != i2 && Caveman.players[i1].name.Equals(Caveman.players[i2].name))
                                        {
                                            Caveman.players.RemoveAt(i1);
                                            i1--;
                                            break;
                                        }
                                    }
                                }
                            }
                                break;

                            case "move":
                            {
                                try
                                {
                                    string name = incmsg.ReadString();
                                    int x = incmsg.ReadInt32();
                                    int y = incmsg.ReadInt32();

                                    for (int i = 0; i < Caveman.players.Count; i++)
                                    {
                                        //It is important that you only set the value of the player, if it is not yours, 
                                        //otherwise it would cause lagg (because you'll always be first with yours, and there is a slight delay from server-client).
                                        //Of course, sometimes have to force the server to the actual position of the player, otherwise could easily cheat.
                                        if (Caveman.players[i].name.Equals(name) )//&&
                                            // Caveman.players[i].name != TextInput.text)
                                        {
                                            Caveman.players[i]._velocity= new Vector2(x, y);
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

                                for (int i = 0; i < Caveman.players.Count; i++)
                                {
                                    if (Caveman.players[i].name.Equals(name))
                                    {
                                        Caveman.players.RemoveAt(i);
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
                                Caveman.players.Clear();
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