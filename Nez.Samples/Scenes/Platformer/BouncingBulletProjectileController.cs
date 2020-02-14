using Microsoft.Xna.Framework;

namespace Nez.Samples
{
    public class BouncingBulletProjectileController : Component, IUpdatable
    {
        public Vector2 Velocity;

        ProjectileMover _mover;
        private bool destroy = false;


        public BouncingBulletProjectileController(Vector2 velocity) => Velocity = velocity;

        public override void OnAddedToEntity() => _mover = Entity.GetComponent<ProjectileMover>();

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