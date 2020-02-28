using Microsoft.Xna.Framework;
using Nez.Tiled;

namespace Nez.Samples
{
    public class BouncingBulletProjectileController : Component, IUpdatable
    {
        public Vector2 Velocity;

        TiledMapMover _mover;
        private bool destroy;

        public BouncingBulletProjectileController(Vector2 velocity) => Velocity = velocity;

        public override void OnAddedToEntity() => _mover = Entity.GetComponent<TiledMapMover>();

        void IUpdatable.Update()
        {
            Core.Schedule(2f, timer => destroy = true);

            if (destroy)
                Entity.Destroy();

            Velocity.Y += 400 * Time.DeltaTime;
        }
    }
}