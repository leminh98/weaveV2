using Microsoft.Xna.Framework;

namespace Nez.Samples
{
    public class BouncingBullet : Component, IUpdatable
    {
	    #region Fields
	    public float Mass
	    {
		    get => _mass;
		    set => SetMass(value);
	    }
	    
	    public float Elasticity
	    {
		    get => _elasticity;
		    set => SetElasticity(value);
	    }
	    
	    public float Friction
	    {
		    get => _friction;
		    set => SetFriction(value);
	    }

	    public float Glue
	    {
		    get => _glue;
		    set => SetGlue(value);
	    }

	    public bool ShouldUseGravity = true;
	    
	    public Vector2 Velocity;
	    
	    public bool IsImmovable => _mass < 0.0001f;
	    
	    #endregion

	    float _mass = 10f;
		float _elasticity = 0.5f;
		float _friction = 0.5f;
		float _glue = 0.01f;
		float _inverseMass;
		Collider _collider;
		
		public BouncingBullet()
		{
			_inverseMass = 1 / _mass;
		}

		#region Fluent setters
		
		public BouncingBullet SetMass(float mass)
		{
			_mass = Mathf.Clamp(mass, 0, float.MaxValue);

			if (_mass > 0.0001f)
				_inverseMass = 1 / _mass;
			else
				_inverseMass = 0f;
			return this;
		}

		public BouncingBullet SetElasticity(float value)
		{
			_elasticity = Mathf.Clamp01(value);
			return this;
		}

		public BouncingBullet SetFriction(float value)
		{
			_friction = Mathf.Clamp01(value);
			return this;
		}
		
		public BouncingBullet SetGlue(float value)
		{
			_glue = Mathf.Clamp(value, 0, 10);
			return this;
		}

		public BouncingBullet SetVelocity(Vector2 velocity)
		{
			Velocity = velocity;
			return this;
		}

		#endregion
		
		public void AddImpulse(Vector2 force)
		{
			if (!IsImmovable)
				Velocity += force * 100000 * (_inverseMass * Time.DeltaTime * Time.DeltaTime);
		}

		public override void OnAddedToEntity()
		{
			_collider = Entity.GetComponent<Collider>();
			Debug.WarnIf(_collider == null, "BouncingBullet has no Collider. BouncingBullet requires a Collider!");
		}

		void IUpdatable.Update()
		{
			if (IsImmovable || _collider == null)
			{
				Velocity = Vector2.Zero;
				return;
			}

			if (ShouldUseGravity)
				Velocity += Physics.Gravity * Time.DeltaTime;

			Entity.Transform.Position += Velocity * Time.DeltaTime;

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
					var isPlayer = neighbor.Entity.GetComponent<BulletHitDetector>();
					if (isPlayer != null)
					{
						isPlayer.currentHP--;
						Entity.Destroy();
						var notBoss = neighbor.Entity.GetComponent<Caveman>();
						if (notBoss != null)
						{
							var drop = neighbor.Entity.GetComponent<DropItem>();
							if (drop != null)
							{
								System.Console.WriteLine("Dropping at position: " + Entity.Transform.Position.ToString());
								drop.Release(neighbor.Entity.Transform.Position);
								neighbor.Entity.GetComponent<Caveman>().itemBuffer[drop.itemNum] = false;
								neighbor.Entity.RemoveComponent(drop);
							}
						}
						if (isPlayer.currentHP <=  0)
						{
							var drop = neighbor.Entity.GetComponent<DropItem>();
							while (drop != null)
							{
								System.Console.WriteLine(
									"Dropping at position: " + Entity.Transform.Position.ToString());
								drop.Release(neighbor.Entity.Transform.Position);
								neighbor.Entity.GetComponent<Caveman>().itemBuffer[drop.itemNum] = false;
								neighbor.Entity.RemoveComponent(drop);
								drop = neighbor.Entity.GetComponent<DropItem>();
							}
							
							var platformerScene = Entity.Scene as PlatformerScene;
							platformerScene.Respawn(neighbor.Entity, neighbor.Entity.GetComponent<Caveman>().name);
							// neighbor.Entity.Destroy();
							Entity.Destroy();
							return;
						}
            
						isPlayer._sprite.Color = Color.Red;
						Core.Schedule(0.1f, timer => isPlayer._sprite.Color = Color.White);
					}
					else
					{
						// neighbor has no ArcadeRigidbody so we assume its immovable and only move ourself
						Entity.Transform.Position -= collisionResult.MinimumTranslationVector;
						var relativeVelocity = Velocity;
						CalculateResponseVelocity(ref relativeVelocity, ref collisionResult.MinimumTranslationVector,
							out relativeVelocity);
						Velocity += relativeVelocity;
					}
				}
			}
		}
		
		void CalculateResponseVelocity(ref Vector2 relativeVelocity, ref Vector2 minimumTranslationVector,
		                               out Vector2 responseVelocity)
		{
			// first, we get the normalized MTV in the opposite direction: the surface normal
			var inverseMTV = minimumTranslationVector * -1f;
			Vector2 normal;
			Vector2.Normalize(ref inverseMTV, out normal);

			// the velocity is decomposed along the normal of the collision and the plane of collision.
			// The elasticity will affect the response along the normal (normalVelocityComponent) and the friction will affect
			// the tangential component of the velocity (tangentialVelocityComponent)
			float n;
			Vector2.Dot(ref relativeVelocity, ref normal, out n);

			var normalVelocityComponent = normal * n;
			var tangentialVelocityComponent = relativeVelocity - normalVelocityComponent;

			if (n > 0.0f)
				normalVelocityComponent = Vector2.Zero;

			// if the squared magnitude of the tangential component is less than glue then we bump up the friction to the max
			var coefficientOfFriction = _friction;
			if (tangentialVelocityComponent.LengthSquared() < _glue)
				coefficientOfFriction = 1.01f;

			// elasticity affects the normal component of the velocity and friction affects the tangential component
			responseVelocity = -(1.0f + _elasticity) * normalVelocityComponent -
			                   coefficientOfFriction * tangentialVelocityComponent;
		}
    }
}