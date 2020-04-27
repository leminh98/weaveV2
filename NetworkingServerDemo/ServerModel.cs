using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
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
            Console.WriteLine("Your IP is...");
            
            var localIp = "";

            try
            {
                StringBuilder sb = new StringBuilder(); 

                // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces(); 

                foreach (NetworkInterface network in networkInterfaces) 
                { 
                    // Read the IP configuration for each network 
                    IPInterfaceProperties properties = network.GetIPProperties(); 

                    // Each network interface may have multiple IP addresses 
                    foreach (IPAddressInformation address in properties.UnicastAddresses) 
                    { 
                        // We're only interested in IPv4 addresses for now 
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork) 
                            continue; 

                        // Ignore loopback addresses (e.g., 127.0.0.1) 
                        if (IPAddress.IsLoopback(address.Address)) 
                            continue;

                        if (address.Address.ToString().StartsWith("10"))
                        {
                            sb.AppendLine(address.Address.ToString());
                            break;
                        }
                        
                    }

                    if (sb.Length != 0)
                        break;
                } 

                Console.WriteLine(sb.ToString());
            }
            catch 
            {
                
            }
           
            // try
            // {
            //     var host = Dns.GetHostAddresses("machine-mbp.dyndns.rice.edu");
            //     foreach (var ip in host)
            //     {
            //         Console.WriteLine("haha " + ip + " " + ip.AddressFamily);
            //         if (ip.AddressFamily == AddressFamily.InterNetwork)
            //         {
            //             localIp =  ip.ToString();
            //         }
            //     }
            //      
            // } catch 
            // {}
            //Console.WriteLine(localIp);
            Console.WriteLine("Waiting for connections...");
        }

        public static void Update() //updating the Network and the Player Method with timer1 (Tick interval 16 ≈ 60FPS)
        {
            System.Threading.Thread.Sleep(10);
            if (Program.needNumberOfPlayer)
            {
                Console.WriteLine("Enter number of players:");
                Program.NumPlayer =  int.Parse(Console.ReadLine());
                Program.needNumberOfPlayer = false;
                Console.WriteLine("Num player = " + Program.NumPlayer);
                return;
            }
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
                Console.WriteLine("in map selection phase");
                Network.mapSelectionPhase();
                return;
            }

            if (!Network.singleGamePhaseDone)
            {
                
                Network.singleGamePhase();
                return;
            }

            if (!Network.postSingleGamePhaseDone)
            {
                Console.WriteLine("in post single player game phase");
                Network.postSingleGamePhase();
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