using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;

namespace Nez.Samples
{
    public class Shield : Component, IUpdatable
    {
        Collider _collider;
        private string owner;
        private int destroy = 180;
        
        public Shield(string name)
        {
	        owner = name;
        }

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
					if (neighbor.Entity.Name.Equals("projectile"))
					{
						string name = "owner";
						if (neighbor.Entity.GetComponent<BulletProjectileController>() != null)
						{
							name = neighbor.Entity.GetComponent<BulletProjectileController>().Name;
						}
						else if (neighbor.Entity.GetComponent<BouncingBullet>() != null)
						{
							name = neighbor.Entity.GetComponent<BouncingBullet>().Name;
						}

						if (!owner.Equals(name))
						{
							Entity.Destroy();
							neighbor.Entity.Destroy();
						}
					}
				}
			}

			// Core.Schedule(3f, timer => destroy = true);
			destroy -= 1;
			if (destroy <= 0)
			{
				Entity.Destroy();
			}
        }
    }
}