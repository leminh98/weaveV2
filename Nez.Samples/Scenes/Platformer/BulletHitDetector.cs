using Nez.Sprites;
using Microsoft.Xna.Framework;

namespace Nez.Samples
{
    /// <summary>
    /// simple component that detects if it has been hit by a projectile. When hit, it flashes red and destroys itself after being hit
    /// a certain number of times.
    /// </summary>
    public class BulletHitDetector : Component, ITriggerListener
    {
        public int maxHP = 5;
        public int currentHP;
        public SpriteRenderer _sprite;

        public override void OnAddedToEntity()
        {
            _sprite = Entity.GetComponent<SpriteRenderer>();
            currentHP = maxHP;
        }


        void ITriggerListener.OnTriggerEnter(Collider other, Collider self)
        {
            currentHP--;
            if (currentHP <=  0)
            {
                // var drop = Entity.GetComponent<DropItem>();
                // if (drop != null)
                // {
                //     System.Console.WriteLine("Dropping at position: " + Entity.Transform.Position.ToString());
                //     drop.Release(Entity.Transform.Position);
                // }

                if (Entity.Name.Equals("player"))
                {
                    //If it's the player, then remove the player component to trigger the losing message.
                    Entity.RemoveComponent<Caveman>();
                }
                Entity.Destroy();
                return;
            }
            
            _sprite.Color = Color.Red;
            Core.Schedule(0.1f, timer => _sprite.Color = Color.White);
        }


        void ITriggerListener.OnTriggerExit(Collider other, Collider self)
        {
        }
    }
}