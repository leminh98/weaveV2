using Microsoft.Xna.Framework;
using Nez.Tiled;


namespace Nez.Samples
{
    /// <summary>
    /// moves a ProjectileMover and destroys the Entity if it hits anything
    /// </summary>
    public class BulletProjectileController : Component, IUpdatable
    {
        public Vector2 Velocity;

        TiledMapMover _mover;
        TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
        Collider _collider;


        public BulletProjectileController(Vector2 velocity) => Velocity = velocity;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<TiledMapMover>();
            _collider = Entity.GetComponent<Collider>();
        }

        void IUpdatable.Update()
        {
            _mover.Move(Velocity * Time.DeltaTime, Entity.GetComponent<BoxCollider>(), _collisionState);
            if (_collisionState.HasCollision)
                Entity.Destroy();
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
            foreach (var neighbor in neighbors)
            {
                var isPlayer = neighbor.Entity.GetComponent<BulletHitDetector>();
                if (isPlayer != null)
                {
                    Entity.Destroy();
                    isPlayer.currentHP--;
                    if (isPlayer.currentHP <=  0)
                    {
                        neighbor.Entity.Destroy();
                        Entity.Destroy();
                        return;
                    }
            
                    isPlayer._sprite.Color = Color.Red;
                    Core.Schedule(0.1f, timer => isPlayer._sprite.Color = Color.White);
                }
            }

            Velocity.Y += 400 * Time.DeltaTime;
        }
    }
}