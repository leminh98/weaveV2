using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;

namespace Nez.Samples
{
    public class Tracker : Component, IUpdatable
    {
        private string owner;
        
        public Tracker(string name)
        {
            owner = name;
        }

        public override void OnAddedToEntity()
        {
        }

        public override void OnRemovedFromEntity()
        {
        }

        public static Vector2 CalculateTrackerPosition(Vector2 mainPlayerPos, Vector2 mainPlayerLocalPos, Vector2 otherPlayerPos)
        {
            var localOffset = new Vector2(mainPlayerPos.X - mainPlayerLocalPos.X, mainPlayerPos.Y - mainPlayerLocalPos.Y);
            
            var angle = Math.Atan2(mainPlayerPos.Y - otherPlayerPos.Y, otherPlayerPos.X - mainPlayerPos.X);

            var topRightAngle = Math.Atan2(mainPlayerLocalPos.Y, 1200 - mainPlayerLocalPos.X);
            var topLeftAngle = Math.Atan2(mainPlayerLocalPos.Y, 0 - mainPlayerLocalPos.X);
            var bottomLeftAngle = Math.Atan2(mainPlayerLocalPos.Y - 650, 0 - mainPlayerLocalPos.X);
            var bottomRightAngle = Math.Atan2(mainPlayerLocalPos.Y - 650, 1200 - mainPlayerLocalPos.X);
            
            Vector2 edge = new Vector2(0,0);

            if (angle > bottomRightAngle && angle < topRightAngle)
            {
                edge = new Vector2(1200, mainPlayerLocalPos.Y - (float) (Math.Tan(angle) * (1200 - mainPlayerLocalPos.X)));
            } 
            else if (angle >= topRightAngle && angle < topLeftAngle)
            {
                edge = new Vector2(mainPlayerLocalPos.X + (float) ((mainPlayerLocalPos.Y) / Math.Tan(angle)),0);
            } 
            else if (angle >= topLeftAngle || angle < bottomLeftAngle)
            {
                edge = new Vector2(0,mainPlayerLocalPos.Y + (float) (Math.Tan(angle) * (mainPlayerLocalPos.X)));
            } 
            else if (angle >= bottomLeftAngle && angle < bottomRightAngle)
            {
                edge = new Vector2(mainPlayerLocalPos.X - (float) ((650 - mainPlayerLocalPos.Y) / Math.Tan(angle)),650);
            }
            else
            {
                System.Console.WriteLine("Error uncovered case");
            }
            
            edge = new Vector2(edge.X + localOffset.X, edge.Y + localOffset.Y);
            
            
            
            return edge;
        }

        void IUpdatable.Update()
        {
        }
    }
}