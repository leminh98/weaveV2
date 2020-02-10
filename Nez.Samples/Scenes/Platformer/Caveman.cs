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
	public class Caveman : Component, ITriggerListener, IUpdatable
	{
		public float MoveSpeed = 150;
		public float Gravity = 1000;
		public float JumpHeight = 16 * 5;
		public string name;
		private bool _fireInputIsPressed;

		SpriteAnimator _animator;
		TiledMapMover _mover;
		BoxCollider _boxCollider;
		TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
		public Vector2 _velocity;
		
		Vector2 _projectileVelocity = new Vector2(400);

		VirtualButton _jumpInput;
		VirtualButton _fireInput;
		VirtualIntegerAxis _xAxisInput;

		public Caveman(string name) => this.name = name; 

		public override void OnAddedToEntity()
		{
			var texture = Entity.Scene.Content.Load<Texture2D>(Content.Platformer.Caveman);
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

			SetupInput();
		}

		public override void OnRemovedFromEntity()
		{
			// deregister virtual input
			_jumpInput.Deregister();
			_xAxisInput.Deregister();
			_fireInput.Deregister();
		}

		void SetupInput()
		{
			// setup input for shooting a fireball. we will allow z on the keyboard or a on the gamepad
			_fireInput = new VirtualButton();
			_fireInput.Nodes.Add(new VirtualButton.MouseLeftButton());
			_fireInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));
			_fireInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
			
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
						var center = new Vector2(320, 240);
						var dir = Vector2.Normalize(Input.MousePosition - center);
						
						var platformerScene = Entity.Scene as PlatformerScene;
						platformerScene.CreateProjectiles(Entity.Transform.Position, _projectileVelocity * dir);
						_fireInputIsPressed = true;
					} else { _fireInputIsPressed = false;}
					
					
					Network.outmsg = Network.Client.CreateMessage();
					Network.outmsg.Write("move");
					Network.outmsg.Write(LoginScene._playerName);
					Network.outmsg.Write((int) position.X);
					Network.outmsg.Write((int) position.Y);
					Network.outmsg.Write((int) _velocity.X);
					Network.outmsg.Write((int) _velocity.Y);
					Network.outmsg.Write((bool) _fireInputIsPressed);
					Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);
				
				// else
				// {
				// 	p._mover.Move();
				// 	var ninjaScene = Entity.Scene as PlatformerScene;
				// 	ninjaScene.CreateNewPlayer(name);
				// }

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