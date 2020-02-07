using System;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace NetworkingDemo
{
    class Program
    {
        private static KeyboardState EscapeKeyState; //Current escape key is the Q button

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server");

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