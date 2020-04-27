using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace NetworkingDemo
{
    class Program
    {
        public static int NumPlayer = 1;
        public static bool needNumberOfPlayer = true;
        private static KeyboardState EscapeKeyState; //Current escape key is the Q button
        public static StreamWriter log;
        public static StreamWriter log2;
        static void Main(string[] args)
        {
            // log = new StreamWriter("NetworkLog_Minh.txt", true);
            // log2 = new StreamWriter("NetworkLog_Minh2.txt", true);
            //
            // log.WriteLine(System.DateTime.Now.ToString());
            // log.WriteLine("Packages received: ");
            //
            // log2.WriteLine(System.DateTime.Now.ToString());
            // log2.WriteLine("Packages received: ");
            
            Console.WriteLine("Starting Server");
            Console.WriteLine("Please enter the number of player for this session:");
            NumPlayer = int.Parse(Console.ReadLine());
            needNumberOfPlayer = false;
            ServerModel.Start();
            while (true)
            {
                // this does not work yet
                if (EscapeKeyState.IsKeyDown(Keys.Q))
                {
                    log.Close();
                    ServerModel.Shutdown();
                    break;
                }
                ServerModel.Update();
            }
        }
    }
}