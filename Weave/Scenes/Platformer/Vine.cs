using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;

namespace Nez.Samples
{
    public class Vine : Component, IUpdatable
    {
        Collider _collider;
        private bool destroy;

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            Debug.WarnIf(_collider == null, "BouncingBullet has no Collider. BouncingBullet requires a Collider!");
        }

        public override void OnRemovedFromEntity()
        {
        }

        void IUpdatable.Update()
        {
            CollisionResult collisionResult;

            // fetch anything that we might collide with at our new position
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
            foreach (var neighbor in neighbors)
            {
                // if the neighbor collider is of the same entity, ignore it
                if (neighbor.Entity == Entity)
                {
                    continue;
                }

                if (_collider.CollidesWith(neighbor, out collisionResult))
                {
                    var player = neighbor.Entity.GetComponent<Caveman>();
                    if (player != null)
                    {
                        player.climbable = true;
                    }
                }
            }

            Core.Schedule(10f, timer => destroy = true);
			
            if (destroy)
                Entity.Destroy();
        }
    }
}