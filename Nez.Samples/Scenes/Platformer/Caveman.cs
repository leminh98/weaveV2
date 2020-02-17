using System;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Microsoft.Xna.Framework.Input;
using Nez.Samples.Scenes.EndGame;
using Nez.Tiled;
using Nez.Tweens;


namespace Nez.Samples
{
	public class Caveman : Component, ITriggerListener, IUpdatable
	{
		public float MoveSpeed = 150;
		public float Gravity = 1000;
		public float JumpHeight = 16 * 5;
		public string name;
		private bool _fireInputIsPressed;
		private bool _fireBounceInputIsPressed;
		public bool _pickUpItem;
		private  string spriteType = LoginScene._characterSpriteType;
		public bool gotCrown = false; // show that the player has got the crown or not
		private bool startWinTransition = false;

		SpriteAnimator _animator;
		TiledMapMover _mover;
		BoxCollider _boxCollider;
		TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
		public Vector2 _velocity;
		
		Vector2 _projectileVelocity = new Vector2(400);

		VirtualButton _jumpInput;
		VirtualButton _fireInput;
		private VirtualButton _collectInput;
		private VirtualButton _fireBounceInput;
		VirtualIntegerAxis _xAxisInput;

		public Caveman(string name) => this.name = name; 

		public override void OnAddedToEntity()
		{
			string textureToLoad = "Platformer/player" + spriteType;

			var texture = Entity.Scene.Content.Load<Texture2D>(textureToLoad);
			var sprites = Sprite.SpritesFromAtlas(texture, 32, 32);

			_boxCollider = Entity.GetComponent<BoxCollider>();
			_mover = Entity.GetComponent<TiledMapMover>();
			_animator = Entity.AddComponent(new SpriteAnimator(sprites[0]));
			
			#region Movement Animation Setup
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
			
			SetupInput();
		}

		public override void OnRemovedFromEntity()
		{
			// deregister virtual input
			_jumpInput.Deregister();
			_xAxisInput.Deregister();
			_fireInput.Deregister();
			_fireBounceInput.Deregister();
			_collectInput.Deregister();
			
			//Send final move message with 0 health
			Network.outmsg = Network.Client.CreateMessage();
			Network.outmsg.Write("move");
			Network.outmsg.Write(LoginScene._playerName);
			Network.outmsg.Write((int) Entity.Position.X);
			Network.outmsg.Write((int) Entity.Position.Y);
			Network.outmsg.Write((int) _velocity.X);
			Network.outmsg.Write((int) _velocity.Y);
			Network.outmsg.Write((bool) _fireInputIsPressed);
			Network.outmsg.Write((int) 0);
			Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);
			
			//trigger lose scene
			if (gotCrown)
			{
				TweenManager.StopAllTweens();
				Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(WinScene)) as Scene));
			
			}
			else
			{
				TweenManager.StopAllTweens();
				Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(LoseScene)) as Scene));	
			}
			
		}

		void SetupInput()
		{
			// setup input for shooting a fireball
			_fireInput = new VirtualButton();
			_fireInput.Nodes.Add(new VirtualButton.MouseLeftButton());
			_fireInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));
			_fireInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
			
			_fireBounceInput = new VirtualButton();
			_fireBounceInput.Nodes.Add(new VirtualButton.MouseRightButton());
			
			_collectInput = new VirtualButton();
			_collectInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.S));
			
			// setup input for jumping. we will allow z on the keyboard or a on the gamepad
			_jumpInput = new VirtualButton();
			_jumpInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.W));
			_jumpInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));

			// horizontal input from dpad, left stick or keyboard left/right
			_xAxisInput = new VirtualIntegerAxis();
			_xAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
			_xAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
			_xAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D));
		}

		void IUpdatable.Update()
		{
			if (gotCrown)
			{
				Entity.RemoveComponent(this);
			}
			// handle movement and animations
				var moveDir = new Vector2(_xAxisInput.Value, 0);
				string animation = null;

				if (moveDir.X < 0)
				{
					if (_collisionState.Below)
						animation = "Run";
					_animator.FlipX = true;
					_velocity.X = -MoveSpeed;
				}
				else if (moveDir.X > 0)
				{
					if (_collisionState.Below)
						animation = "Run";
					_animator.FlipX = false;
					_velocity.X = MoveSpeed;
				}
				else
				{
					_velocity.X = 0;
					if (_collisionState.Below)
						animation = "Idle";
				}

				if (_collisionState.Below && _jumpInput.IsPressed)
				{
					animation = "Jumping";
					_velocity.Y = -Mathf.Sqrt(2f * JumpHeight * Gravity);
				}

				if (!_collisionState.Below && _velocity.Y > 0)
					animation = "Falling";

				// apply gravity
				_velocity.Y += Gravity * Time.DeltaTime;

				// move
				_mover.Move(_velocity * Time.DeltaTime, _boxCollider, _collisionState);

				var position = Entity.Transform.Position + _velocity* Time.DeltaTime;
				
				if (_collisionState.Below)
					_velocity.Y = 0;

				if (animation != null && !_animator.IsAnimationActive(animation))
					_animator.Play(animation);

				// handle firing a projectile
				if (_fireInput.IsPressed)
				{
					// fire a projectile in the direction we are facing
					var dir = Vector2.Normalize(Entity.Scene.Camera.ScreenToWorldPoint(Input.MousePosition) 
					                            - Entity.Transform.Position);
					var pos = Entity.Transform.Position;
					if (dir.X <= 0)
						pos.X -= 15;
					else
						pos.X += 10;

					pos.Y -= 15;
					
					var platformerScene = Entity.Scene as PlatformerScene;
					platformerScene.CreateProjectiles(pos, _projectileVelocity * dir);
					_fireInputIsPressed = true;
				} else { _fireInputIsPressed = false;}
				
				if (_fireBounceInput.IsPressed)
				{
					// fire a projectile in the direction we are facing
					var dir = Vector2.Normalize(Entity.Scene.Camera.ScreenToWorldPoint(Input.MousePosition) 
					                            - Entity.Transform.Position);
					var pos = Entity.Transform.Position;
					if (dir.X <= 0)
						pos.X -= 15;
					else
						pos.X += 10;

					pos.Y -= 15;
					
					var platformerScene = Entity.Scene as PlatformerScene;
					platformerScene.CreateBouncingProjectiles(pos, 1f, _projectileVelocity * dir);
					// _fireInputIsPressed = true;
				}/* else { _fireInputIsPressed = false;}*/

				
				_pickUpItem = _collectInput.IsPressed ? true : false;
				
				// health check
				var healthComponent = Entity.GetComponent<BulletHitDetector>().currentHP;
				// System.Console.WriteLine(healthComponent);

				Network.outmsg = Network.Client.CreateMessage();
				Network.outmsg.Write("move");
				Network.outmsg.Write(LoginScene._playerName);
				Network.outmsg.Write((int) position.X);
				Network.outmsg.Write((int) position.Y);
				Network.outmsg.Write((int) _velocity.X);
				Network.outmsg.Write((int) _velocity.Y);
				Network.outmsg.Write((bool) _fireInputIsPressed);
				Network.outmsg.Write((int) healthComponent);
				Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);
				
				// sending health of other player on your screen:
				if (healthComponent == 0)
					Entity.RemoveComponent(this);

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