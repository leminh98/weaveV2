using System;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Microsoft.Xna.Framework.Input;
using Nez.Samples.Scenes.CharacterSelection;
using Nez.Samples.Scenes.EndGame;
using Nez.Tiled;
using Nez.Tweens;


namespace Nez.Samples
{
    public class Caveman : Component, ITriggerListener, IUpdatable
   { 
        public float MoveSpeed = 150;
        public float Gravity = 1000;
        public float JumpHeight = 32 * 2 + 16; // the height cap is at the center of the sprite, so I add 16 to account for it
        public string name;
        public bool climbable;
        List<int> elemBuffer = new List<int>();
        public bool[] itemBuffer = new bool[4];
        public int mana = 5;
        public int playerIndex = LoginScene.playerIndex; //Server should update this
        private bool _fireInputIsPressed;
        private bool _fireBounceInputIsPressed;
        public bool _pickUpItem;
        private string spriteType = CharacterSelectionScene.chosenSprite;
        public bool win = false; // true if number of kills >= 10
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
        VirtualIntegerAxis _yAxisInput;
        private VirtualButton _waterElemInput;
        private VirtualButton _earthElemInput;
        // private VirtualButton _climbInput;

        public Caveman(string name) => this.name = name;

        public override void OnAddedToEntity()
        {
            string textureToLoad = "Platformer/" + spriteType;

            var texture = Entity.Scene.Content.Load<Texture2D>(textureToLoad);
            var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);

            _boxCollider = Entity.GetComponent<BoxCollider>();
            _mover = Entity.GetComponent<TiledMapMover>();
            _animator = Entity.AddComponent(new SpriteAnimator(sprites[0]));
            _animator.RenderLayer = 1;
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
            
            _animator.AddAnimation("Climb", new[]
            {
                sprites[32 + 0],
                sprites[32 + 1],
                sprites[32 + 2],
                sprites[32 + 3],
                sprites[32 + 4],
                sprites[32 + 5]
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
            // _climbInput.Deregister();
            _yAxisInput.Deregister();
            _xAxisInput.Deregister();
            _fireInput.Deregister();
            _fireBounceInput.Deregister();
            _collectInput.Deregister();
            _waterElemInput.Deregister();
            _earthElemInput.Deregister();

            // trigger lose scene
            if (win)
            {
                Network.outmsg = Network.Client.CreateMessage();
                Network.outmsg.Write("win");
                Network.outmsg.Write(LoginScene._playerName);
                Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
                // Network.singleGamePhaseDone = true;
                TweenManager.StopAllTweens();
                Core.StartSceneTransition(new FadeTransition(() =>
                    Activator.CreateInstance(typeof(WinScene)) as Scene));
                
            }
            else
            {
                // TweenManager.StopAllTweens();
                // Core.StartSceneTransition(
                //     new FadeTransition(() => Activator.CreateInstance(typeof(LoseScene)) as Scene));
            }
        }

        void SetupInput()
        {
            _waterElemInput = new VirtualButton();
            _waterElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.D1));
            _waterElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.NumPad1));

            _earthElemInput = new VirtualButton();
            _earthElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.D2));
            _earthElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.NumPad2));

            // setup input for shooting a fireball
            _fireInput = new VirtualButton();
            _fireInput.Nodes.Add(new VirtualButton.MouseLeftButton());
            _fireInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));

            _fireBounceInput = new VirtualButton();
            _fireBounceInput.Nodes.Add(new VirtualButton.MouseRightButton());

            _collectInput = new VirtualButton();
            _collectInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.S));

            // setup input for jumping. we will allow z on the keyboard or a on the gamepad
            _jumpInput = new VirtualButton();
            _jumpInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.W));
            _jumpInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
            
            // _climbInput = new VirtualButton();
            // _climbInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.W));

            // horizontal input from dpad, left stick or keyboard left/right
            _xAxisInput = new VirtualIntegerAxis();
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _xAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, 
                Keys.A, Keys.D));
            
            _yAxisInput = new VirtualIntegerAxis();
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _yAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, 
                Keys.Space, Keys.E));
        }

        void IUpdatable.Update()
        {
            if (_waterElemInput.IsPressed)
            {
                if (elemBuffer.Count <= 2 && mana > 0)
                {
                    elemBuffer.Add(1);
                    mana -= 1;
                }
            }

            if (_earthElemInput.IsPressed)
            {
                if (elemBuffer.Count <= 2 && mana > 0)
                {
                    elemBuffer.Add(2);
                    mana -= 1;
                }
            }

            if (win)
            {
                Entity.RemoveComponent(this);
            }

            // handle movement and animations

            #region movement

            var moveDir = new Vector2(_xAxisInput.Value, 0);
            string animation = null;

            if (climbable)
            {
                var moveY = new Vector2(0, _yAxisInput.Value);
                if (moveY.Y < 0)
                {
                    animation = "Climb";
                    _velocity.Y = -MoveSpeed;
                }
                else if (moveY.Y > 0)
                {
                    animation = "Climb";
                    _velocity.Y = MoveSpeed;
                }

                climbable = false;
            }

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

            var position = Entity.Transform.Position + _velocity * Time.DeltaTime;

            if (_collisionState.Below)
                _velocity.Y = 0;

            if (animation != null && !_animator.IsAnimationActive(animation))
                _animator.Play(animation);

            #endregion

            
            float projectileDirX = 0;
            float projectileDirY = 0;
            int fireType = 1;
            // handle firing a projectile
            var platformerScene = Entity.Scene as PlatformerScene;
            if (_fireInput.IsPressed)
            {
                if (elemBuffer.Count == 0)
                {
                    System.Console.WriteLine("Need to load element to shoot");
                }
                else
                {
                    // fire a projectile in the direction we are facing
                    var dir = Vector2.Normalize(Entity.Scene.Camera.ScreenToWorldPoint(Input.MousePosition)
                                                - Entity.Transform.Position);
                    var pos = Entity.Transform.Position;
                    if (dir.X <= 0)
                        pos.X -= 30;
                    else
                        pos.X += 20;

                    // pos.Y -= 30;
                    projectileDirX = dir.X;
                    projectileDirY = dir.Y;
                    int type = 0;
                    if (elemBuffer.Count == 1)
                    {
                        if (elemBuffer.Contains(1)) { type = 1; }
                        else if (elemBuffer.Contains(2)) { type = 2; }
                    }
                    else if (elemBuffer.Count == 2)
                    {
                        if (elemBuffer.Contains(1))
                        {
                            if (elemBuffer.Contains(2)) { type = 12; }
                            else { type = 11; }
                        }
                        else { type = 22; }
                    }

                    fireType = type;
                    if (type == 1) { platformerScene.CreateShield(Entity, name); } 
                    else { platformerScene.CreateProjectiles(name, type, pos, dir); }
                    
                    elemBuffer.Clear();
                    _fireInputIsPressed = true;
                }
            }
            else
            {
                _fireInputIsPressed = false;
            }

            // if (_fireBounceInput.IsPressed)
            // {
            //     // fire a projectile in the direction we are facing
            //     var dir = Vector2.Normalize(Entity.Scene.Camera.ScreenToWorldPoint(Input.MousePosition)
            //                                 - Entity.Transform.Position);
            //     var pos = Entity.Transform.Position;
            //     if (dir.X <= 0)
            //         pos.X -= 30;
            //     else
            //         pos.X += 20;
            //
            //     // pos.Y -= 50;
            //     projectileDir = dir;
            //     var platformerScene = Entity.Scene as PlatformerScene;
            //     platformerScene.CreateBouncingProjectiles(pos, 1f, _projectileVelocity * dir);
            //     // _fireInputIsPressed = true;
            // } /* else { _fireInputIsPressed = false;}*/

            _pickUpItem = _collectInput.IsPressed;

            if (PlatformerScene.playerKillComponent.kills >= 10) { win = true; }

            // health check
            var healthComponent = Entity.GetComponent<BulletHitDetector>().currentHP;
            // System.Console.WriteLine(healthComponent);

            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("move");
            Network.outmsg.Write(LoginScene._playerName);
            Network.outmsg.Write(position.X);
            Network.outmsg.Write(position.Y);
            Network.outmsg.Write( _velocity.X); //TODO: SHOULD THIS BE INT OR FLOAT
            Network.outmsg.Write( _velocity.Y);
            Network.outmsg.Write((bool) _fireInputIsPressed);
            Network.outmsg.Write((int) fireType);
            Network.outmsg.Write((float) projectileDirX);
            Network.outmsg.Write((float) projectileDirY);
            Network.outmsg.Write((int) PlatformerScene.playerKillComponent.kills);
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);

            // sending health of other player on your screen:
            // if (healthComponent == 0)
            // {
            //     var platformerScene = Entity.Scene as PlatformerScene;
            //     platformerScene.Respawn(Entity);
            //     // Entity.RemoveComponent(this);
            // }

            if (mana < 5)
            {
                mana += 1;
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