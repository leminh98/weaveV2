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
        static void Main(string[] args)
        {
            log = new StreamWriter("NetworkLog.txt", true);

            log.WriteLine(System.DateTime.Now.ToString());
            log.WriteLine("Packages received: ");
            
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