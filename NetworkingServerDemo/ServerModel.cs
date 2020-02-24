using Lidgren.Network;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace NetworkingDemo
{
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
            if (!Network.connectPhaseDone)
            {
                Network.connectionPhase();
                return;
            }

            if (!Network.playerSelectionPhaseDone)
            {
                Network.playerSelectionPhase();
                return;
            }

            if (!Network.mapSelectionPhaseDone)
            {
                Network.mapSelectionPhase();
                return;
            }

            if (!Network.singleGamePhaseDone)
            {
                Console.WriteLine("in single player game phase");
                Network.singleGamePhase();
                return;
            }

            if (!Network.postSingleGamePhaseDone)
            {
                return;
            }

            if (!Network.gameOver)
            {
                return;
            }
            // Network.Update();
            // Player.Update();
        }

        public static void Shutdown()
        {
            Network.Shutdown();
        }
    }

    
}