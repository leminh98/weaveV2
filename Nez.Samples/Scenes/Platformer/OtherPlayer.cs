using System;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Microsoft.Xna.Framework.Input;
using Nez.Tiled;


namespace Nez.Samples
{
    public class OtherPlayer : Component, ITriggerListener, IUpdatable
	{
		public float MoveSpeed = 150;
		public float Gravity = 1000;
		public float JumpHeight = 16 * 5;
		public string name;
		public bool _fireInputIsPressed;
		public bool _fireBounceInputIsPressed;
		private string spriteType;

		SpriteAnimator _animator;
		TiledMapMover _mover;
		BoxCollider _boxCollider;
		TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
		public Vector2 _velocity;
		
		Vector2 _projectileVelocity = new Vector2(400);

		VirtualButton _jumpInput;
		private VirtualButton _fireInput;
		private VirtualButton _fireBounceInput;
		VirtualIntegerAxis _xAxisInput;

		public static List<Tuple<string, string>> players = new List<Tuple<string, string>>();  //contain other players name

		public OtherPlayer(string name, string spriteType)
		{
			this.name = name;
			this.spriteType = spriteType;
		} 

		public override void OnAddedToEntity()
		{
			string textureToLoad = "Platformer/player" + spriteType;

			var texture = Entity.Scene.Content.Load<Texture2D>(textureToLoad);
			var sprites = Sprite.SpritesFromAtlas(texture, 32, 32);

			_boxCollider = Entity.GetComponent<BoxCollider>();
			_mover = Entity.GetComponent<TiledMapMover>();
			_animator = Entity.AddComponent(new SpriteAnimator(sprites[0]));

			#region Animation Setup
			// extract the animations from the atlas. they are setup in rows with 8 columns
			_animator.AddAnimation("Walk", new[]
			{
				sprites[0],
				sprites[1],
				sprites[2],
				sprites[3],
				sprites[4],
				sprites[5]
			});

			_animator.AddAnimation("Run", new[]
			{
				sprites[8 + 0],
				sprites[8 + 1],
				sprites[8 + 2],
				sprites[8 + 3],
				sprites[8 + 4],
				sprites[8 + 5],
				sprites[8 + 6]
			});

			_animator.AddAnimation("Idle", new[]
			{
				sprites[16]
			});

			_animator.AddAnimation("Attack", new[]
			{
				sprites[24 + 0],
				sprites[24 + 1],
				sprites[24 + 2],
				sprites[24 + 3]
			});

			_animator.AddAnimation("Death", new[]
			{
				sprites[40 + 0],
				sprites[40 + 1],
				sprites[40 + 2],
				sprites[40 + 3]
			});

			_animator.AddAnimation("Falling", new[]
			{
				sprites[48]
			});

			_animator.AddAnimation("Hurt", new[]
			{
				sprites[64],
				sprites[64 + 1]
			});

			_animator.AddAnimation("Jumping", new[]
			{
				sprites[72 + 0],
				sprites[72 + 1],
				sprites[72 + 2],
				sprites[72 + 3]
			});
			#endregion

		}

		public override void OnRemovedFromEntity()
		{
		}

		void IUpdatable.Update()
		{
			// handle movement and animations
			
					string animation = null;

					if (_velocity.X < 0)
					{
						if (_collisionState.Below)
							animation = "Run";
						_animator.FlipX = true;
					}
					else if (_velocity.X > 0)
					{
						if (_collisionState.Below)
							animation = "Run";
						_animator.FlipX = false;
					}
					else
					{
						_velocity.X = 0;
						if (_collisionState.Below)
							animation = "Idle";
					}

					if (!_collisionState.Below && _velocity.Y > 0)
						animation = "Falling";

					// apply gravity
					_velocity.Y += Gravity * Time.DeltaTime;

					// move
					_mover.Move(_velocity * Time.DeltaTime, _boxCollider, _collisionState);
					
					if (_collisionState.Below)
						_velocity.Y = 0;

					if (animation != null && !_animator.IsAnimationActive(animation))
						_animator.Play(animation);

					// handle firing a projectile
					if (_fireInputIsPressed)
					{
						// fire a projectile in the direction we are facing
						var dir = Vector2.Normalize(Input.MousePosition - Entity.Transform.Position);
						
						var platformerScene = Entity.Scene as PlatformerScene;
						platformerScene.CreateProjectiles(Entity.Transform.Position, _projectileVelocity * dir);
					}
					
					if (_fireBounceInputIsPressed)
					{
						// fire a projectile in the direction we are facing
						var dir = Vector2.Normalize(Input.MousePosition - Entity.Transform.Position);
						
						var platformerScene = Entity.Scene as PlatformerScene;
						platformerScene.CreateBouncingProjectiles(Entity.Transform.Position, 1f, _projectileVelocity * dir);
					}
					
					var healthComponent = Entity.GetComponent<BulletHitDetector>().currentHP;
					string healthAnimation = healthComponent.ToString();

					Network.outmsg = Network.Client.CreateMessage();
					Network.outmsg.Write("dealDamageToOther");
					Network.outmsg.Write(LoginScene._playerName);
					Network.outmsg.Write(name);
					Network.outmsg.Write((int) healthComponent);
					Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);
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