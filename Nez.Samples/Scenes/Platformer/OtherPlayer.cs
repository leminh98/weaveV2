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
		public float JumpHeight = 32 * 3;
		public string name;
		public bool _fireInputIsPressed;
		public bool _fireBounceInputIsPressed;
		public string spriteType;
		public int playerIndex;
		public int projectileType = 1;
		public Vector2 _projDir;
		private ProjectileHandler projectiles;
		List<int> elemBuffer = new List<int>();
		public bool[] itemBuffer = new bool[4];

		SpriteAnimator _animator;
		TiledMapMover _mover;
		BoxCollider _boxCollider;
		TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
		public Vector2 _velocity;
		
		Vector2 _projectileVelocity = new Vector2(400);

		public static List<OtherPlayerListItem> players = new List<OtherPlayerListItem>();  //contain other players name

		public OtherPlayer(string name, int playerIndex, string spriteType)
		{
			this.name = name;
			this.spriteType = spriteType;
			this.playerIndex = playerIndex;
			this._projDir = new Vector2(0, 0);
		} 

		public override void OnAddedToEntity()
		{
			string textureToLoad = "Platformer/" + spriteType;

			var texture = Entity.Scene.Content.Load<Texture2D>(textureToLoad);
			var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);

			_boxCollider = Entity.GetComponent<BoxCollider>();
			_mover = Entity.GetComponent<TiledMapMover>();
			_animator = Entity.AddComponent(new SpriteAnimator(sprites[0]));
			
			projectiles = new ProjectileHandler(Entity.Scene.Content);
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

					// if (!_collisionState.Below && _velocity.Y > 0)
					// 	animation = "Falling";

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
						var platformerScene = Entity.Scene as PlatformerScene;
						if (projectileType == 1)
						{
							platformerScene.CreateShield(Entity, name);
						}
						else
						{
							// fire a projectile in the direction we are facing
                            var pos = Entity.Transform.Position;
                            if (_projDir.X <= 0)
                            	pos.X -= 30;
                            else
                            	pos.X += 20;
                            
                            platformerScene.CreateProjectiles(name, projectileType, pos, _projDir);
                            
						}
						_fireInputIsPressed = false;
					}
					//
					// if (_fireBounceInputIsPressed)
					// {
					// 	// fire a projectile in the direction we are facing
					// 	var dir = Vector2.Normalize(Entity.Scene.Camera.ScreenToWorldPoint(Input.MousePosition)
					// 	                            - Entity.Transform.Position);
					//
					// 	var pos = Entity.Transform.Position;
					// 	if (_projDir.X <= 0)
					// 		pos.X -= 30;
					// 	else
					// 		pos.X += 20;
					// 	
					// 	var platformerScene = Entity.Scene as PlatformerScene;
					// 	platformerScene.CreateBouncingProjectiles(pos, 1f, _projectileVelocity * _projDir);
					// }
					//
					// var healthComponent = Entity.GetComponent<BulletHitDetector>().currentHP;
					// string healthAnimation = healthComponent.ToString();
					//
					// Network.outmsg = Network.Client.CreateMessage();
					// Network.outmsg.Write("dealDamageToOther");
					// Network.outmsg.Write(LoginScene._playerName);
					// Network.outmsg.Write(name);
					// Network.outmsg.Write((int) healthComponent);
					// Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);
					// if (healthComponent == 0)
					// 	Entity.Destroy();
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