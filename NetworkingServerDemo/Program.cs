using System;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace NetworkingDemo
{
    class Program
    {
        public static int NumPlayer = 1;
        private static KeyboardState EscapeKeyState; //Current escape key is the Q button

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server");
            Console.WriteLine("Please enter the number of player for this session:");
            NumPlayer = int.Parse(Console.ReadLine());
            ServerModel.Start();
            while (true)
            {
                // this does not work yet
                if (EscapeKeyState.IsKeyDown(Keys.Q))
                {
                    ServerModel.Shutdown();
                    break;
                }
                ServerModel.Update();
            }
        }
    }
}