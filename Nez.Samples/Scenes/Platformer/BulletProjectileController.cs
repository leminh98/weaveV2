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


        public BulletProjectileController(Vector2 velocity) => Velocity = velocity;

        public override void OnAddedToEntity() => _mover = Entity.GetComponent<TiledMapMover>();

        void IUpdatable.Update()
        {
            _mover.Move(Velocity * Time.DeltaTime, Entity.GetComponent<BoxCollider>(), _collisionState);
            if (_collisionState.HasCollision)
                Entity.Destroy();
            Velocity.Y += 400 * Time.DeltaTime;
        }
    }
}