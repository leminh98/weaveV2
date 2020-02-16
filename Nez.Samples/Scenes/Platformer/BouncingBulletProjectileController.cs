using Microsoft.Xna.Framework;
using Nez.Tiled;

namespace Nez.Samples
{
    public class BouncingBulletProjectileController : Component, IUpdatable
    {
        public Vector2 Velocity;

        TiledMapMover _mover;
        TiledMapMover.CollisionState _collisionState= new TiledMapMover.CollisionState();
        private bool destroy = false;


        public BouncingBulletProjectileController(Vector2 velocity) => Velocity = velocity;

        public override void OnAddedToEntity() => _mover = Entity.GetComponent<TiledMapMover>();

        void IUpdatable.Update()
        {
            // if (_mover.Move(Velocity * Time.DeltaTime))
            //     Entity.Destroy();
            
            Core.Schedule(3f, timer => destroy = true);

            if (destroy)
                Entity.Destroy();

            Velocity.Y += 400 * Time.DeltaTime;
        }
    }
}