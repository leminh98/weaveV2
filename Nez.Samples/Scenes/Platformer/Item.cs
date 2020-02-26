using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Tiled;

namespace Nez.Samples
{
    public class Item : Component, IUpdatable
    {
	    public Texture2D Texture
	    {
		    get => _texture;
		    set => SetTexture(value);
	    }
	    
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

		public bool ShouldUseGravity = true;

		public Vector2 Velocity;

		Texture2D _texture;
		float _mass = 10f;
		float _elasticity = 0.5f;
		float _friction = 0.5f;
		float _glue = 0.01f;
		float _inverseMass;
		Collider _collider;
		TiledMapMover _mover;
		TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
		private int _num;


		public Item(int num)
		{
			_num = num;
			_inverseMass = 1 / _mass;
		}


		#region Fluent setters

		public Item SetTexture(Texture2D texture)
		{
			_texture = texture;
			return this;
		}
		
		public Item SetMass(float mass)
		{
			_mass = Mathf.Clamp(mass, 0, float.MaxValue);

			if (_mass > 0.0001f)
				_inverseMass = 1 / _mass;
			else
				_inverseMass = 0f;
			return this;
		}

		public Item SetElasticity(float value)
		{
			_elasticity = Mathf.Clamp01(value);
			return this;
		}
		
		public Item SetFriction(float value)
		{
			_friction = Mathf.Clamp01(value);
			return this;
		}
		
		
		public Item SetVelocity(Vector2 velocity)
		{
			Velocity = velocity;
			return this;
		}

		#endregion


		public void AddImpulse(Vector2 force)
		{
			Velocity += force * 100000 * (_inverseMass * Time.DeltaTime * Time.DeltaTime);
		}

		public override void OnAddedToEntity()
		{
			_mover = Entity.GetComponent<TiledMapMover>();
			_collider = Entity.GetComponent<Collider>();
			Debug.WarnIf(_collider == null, "Item has no Collider. Item requires a Collider!");
		}

		void IUpdatable.Update()
		{
			_mover.Move(Velocity * Time.DeltaTime, Entity.GetComponent<BoxCollider>(), _collisionState);
			// if (_collisionState.HasCollision)
			// 	Entity.Destroy();
			if (_collider == null)
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
					// if the neighbor has an ArcadeRigidbody we handle full collision response. If not, we calculate things based on the
					// neighbor being immovable.
					var isPlayer = neighbor.Entity.GetComponent<Caveman>();
					if (isPlayer != null)
					{
						if (isPlayer._pickUpItem)
						{
							Entity.Destroy();
							isPlayer.itemBuffer[_num] = true;
							neighbor.Entity.AddComponent(new DropItem(_num, Texture, Mass, Friction, Elasticity));
							return;
						}
					}
					else if (!neighbor.Entity.Name.Contains("player_"))
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
		
		/// <summary>
		/// given the relative velocity between the two objects and the MTV this method modifies the relativeVelocity to make it a collision
		/// response.
		/// </summary>
		/// <param name="relativeVelocity">Relative velocity.</param>
		/// <param name="minimumTranslationVector">Minimum translation vector.</param>
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