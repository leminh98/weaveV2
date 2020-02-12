using System.Threading;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using Nez.Tiled;

namespace Nez.Samples
{
    public class Boss : Component, ITriggerListener, IUpdatable
    {
        SpriteAnimator _animator;
        TiledMapMover _mover;
        CircleCollider _collider;
        TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
        public Vector2 _velocity;
        Vector2 _projectileVelocity = new Vector2(300);
        private bool shot = false;
        
        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<CircleCollider>();
            _mover = Entity.GetComponent<TiledMapMover>();
        }
        
        public void Update()
        {
            if (!shot)
            {
                var dir = Vector2.Zero;
                var pos = Entity.Transform.Position;
                dir.X = Random.Range(-1f, 1f);
                dir.Y = Random.Range(-1f, 1f);

                if (dir.X < 0)
                    pos.X -= 70;
                else
                    pos.X += 70;
                
                if (dir.Y < 0)
                    pos.Y -= 70;
                else
                    pos.Y += 70;
                
                var bossScene = Entity.Scene as PlatformerScene;
                bossScene.CreateProjectiles(pos, _projectileVelocity * dir);
                shot = true;
                Core.Schedule(0.5f, timer => shot = false);
            }
        }
        
        #region ITriggerListener implementation

        void ITriggerListener.OnTriggerEnter(Collider other, Collider self)
        {
            Debug.Log("triggerEnter: {0}", other.Entity.Name);
        }

        void ITriggerListener.OnTriggerExit(Collider other, Collider self)
        {
            Debug.Log("triggerExit: {0}", other.Entity.Name);
        }

        #endregion
    }
}