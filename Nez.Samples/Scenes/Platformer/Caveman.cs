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
        public int reload = 240;
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
        VirtualButton _dropInput;
        VirtualButton _fireInput;
        private VirtualButton _collectInput;
        private VirtualButton _fireBounceInput;
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        private VirtualButton _waterElemInput;
        private VirtualButton _earthElemInput;
        private VirtualButton _windElemInput;
        // private VirtualButton _climbInput;

        public Caveman(string name) => this.name = name;

        public override void OnAddedToEntity()
        {
            string castingLoad = "Platformer/" + spriteType + "/casting_" + spriteType;
            string fallingLoad = "Platformer/" + spriteType + "/falling_" + spriteType;
            string idleLoad = "Platformer/" + spriteType + "/idle_" + spriteType;
            string jumpLoad = "Platformer/" + spriteType + "/jump_" + spriteType;
            string landingLoad = "Platformer/" + spriteType + "/landing_" + spriteType;
            string runningLoad = "Platformer/" + spriteType + "/running_" + spriteType;

            var castingTexture = Entity.Scene.Content.Load<Texture2D>(castingLoad);
            var casting = Sprite.SpritesFromAtlas(castingTexture, 78, 78);
            var fallingTexture = Entity.Scene.Content.Load<Texture2D>(fallingLoad);
            var falling = Sprite.SpritesFromAtlas(fallingTexture, 67, 69);
            var idleTexture = Entity.Scene.Content.Load<Texture2D>(idleLoad);
            var idle = Sprite.SpritesFromAtlas(idleTexture, 53, 64);
            var jumpTexture = Entity.Scene.Content.Load<Texture2D>(jumpLoad);
            var jump = Sprite.SpritesFromAtlas(jumpTexture, 55, 71);
            var landingTexture = Entity.Scene.Content.Load<Texture2D>(landingLoad);
            var landing = Sprite.SpritesFromAtlas(landingTexture, 68, 75);
            var runningTexture = Entity.Scene.Content.Load<Texture2D>(runningLoad);
            var running = Sprite.SpritesFromAtlas(runningTexture, 67, 74);

            _boxCollider = Entity.GetComponent<BoxCollider>();
            _mover = Entity.GetComponent<TiledMapMover>();
            _animator = Entity.AddComponent(new SpriteAnimator(idle[0]));
            _animator.RenderLayer = 1;
            #region Movement Animation Setup

            // extract the animations from the atlas. they are setup in rows with 8 columns
            // _animator.AddAnimation("Walk", new[]
            // {
            //     sprites[0],
            //     sprites[1],
            //     sprites[2],
            //     sprites[3],
            //     sprites[4],
            //     sprites[5]
            // });

            _animator.AddAnimation("Run", new[]
            {
                running[0],
                running[1],
                running[2],
                running[3],
                running[4],
                running[5],
                running[6],
                running[7],
                running[8],
                running[9],
                running[10],
                running[11]
            });

            _animator.AddAnimation("Idle", new[]
            {
                idle[0],
                idle[1],
                idle[2],
                idle[3]
            });

            _animator.AddAnimation("Casting", new[]
            {
                casting[0],
                casting[1],
                casting[2],
                casting[3],
                casting[4],
                casting[5]
            });
            
            _animator.AddAnimation("Climb", new[]
            {
                idle[0],
                idle[1],
                idle[2],
                idle[3]
            });

            // _animator.AddAnimation("Death", new[]
            // {
            //     sprites[40 + 0],
            //     sprites[40 + 1],
            //     sprites[40 + 2],
            //     sprites[40 + 3]
            // });

            // _animator.AddAnimation("Falling", new[]
            // {
            //     falling[0],
            //     falling[1],
            //     falling[2],
            //     falling[3],
            //     falling[4],
            //     falling[5]
            // });
            
            _animator.AddAnimation("Landing", new[]
            {
                landing[0],
                landing[1],
                landing[2],
                landing[3],
                landing[4],
                landing[5],
                landing[6]
            });

            // _animator.AddAnimation("Hurt", new[]
            // {
            //     sprites[64],
            //     sprites[64 + 1]
            // });

            _animator.AddAnimation("Jumping", new[]
            {
                jump[0],
                jump[1],
                jump[2],
                jump[3]
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
            _windElemInput.Deregister();

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
        }

        void SetupInput()
        {
            _waterElemInput = new VirtualButton();
            _waterElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.D1));
            _waterElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.NumPad1));

            _earthElemInput = new VirtualButton();
            _earthElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.D2));
            _earthElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.NumPad2));
            
            _windElemInput = new VirtualButton();
            _windElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.D3));
            _windElemInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.NumPad3));

            // setup input for shooting a fireball
            _fireInput = new VirtualButton();
            _fireInput.Nodes.Add(new VirtualButton.MouseLeftButton());
            _fireInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));

            _fireBounceInput = new VirtualButton();
            _fireBounceInput.Nodes.Add(new VirtualButton.MouseRightButton());

            _collectInput = new VirtualButton();
            _collectInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.S));

            // setup input for jumping. we will allow w on the keyboard or a on the gamepad
            _jumpInput = new VirtualButton();
            _jumpInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.W));
            _jumpInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
            
            
            // setup input for dropping through platforms. we will allow s on the keyboard
            _dropInput = new VirtualButton();
            _dropInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.S));
            
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
            var platformerScene = Entity.Scene as PlatformerScene;
            if (_waterElemInput.IsPressed)
            {
                if (elemBuffer.Count < 2 && PlatformerScene.playerMana.mana > 0)
                {
                    elemBuffer.Add(1);
                    PlatformerScene.playerMana.mana -= 1;
                    PlatformerScene.playerMana.Entity.GetComponent<TextComponent>().Text = 
                        PlatformerScene.playerMana.playerName +"'s Mana: " + PlatformerScene.playerMana.mana;
                }
            }

            if (_earthElemInput.IsPressed)
            {
                if (elemBuffer.Count < 2 && PlatformerScene.playerMana.mana > 0)
                {
                    elemBuffer.Add(2);
                    PlatformerScene.playerMana.mana -= 1;
                    PlatformerScene.playerMana.Entity.GetComponent<TextComponent>().Text = 
                        PlatformerScene.playerMana.playerName +"'s Mana: " + PlatformerScene.playerMana.mana;
                }
            }
            
            if (_windElemInput.IsPressed)
            {
                if (elemBuffer.Count < 2 && PlatformerScene.playerMana.mana > 0)
                {
                    elemBuffer.Add(3);
                    PlatformerScene.playerMana.mana -= 1;
                    PlatformerScene.playerMana.Entity.GetComponent<TextComponent>().Text = 
                        PlatformerScene.playerMana.playerName +"'s Mana: " + PlatformerScene.playerMana.mana;
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
            
            if (_collisionState.Below && _dropInput.IsPressed)
            {
                var playerPos = this.Entity.Transform.Position;
                playerPos.Y += 2;
                this.Entity.Transform.SetPosition(playerPos);
            }

            if (!_collisionState.Below && _velocity.Y > 0)
                // animation = "Falling";
                animation = "Landing";
            
            

            // apply gravity
            _velocity.Y += Gravity * Time.DeltaTime;

            // move
            _mover.Move(_velocity * Time.DeltaTime, _boxCollider, _collisionState);

            var position = Entity.Transform.Position + _velocity * Time.DeltaTime;

            if (_collisionState.Below)
                _velocity.Y = 0;

            // if (animation != null && !_animator.IsAnimationActive(animation))
            //     _animator.Play(animation);

            #endregion

            
            float projectileDirX = 0;
            float projectileDirY = 0;
            int fireType = 1;
            // handle firing a projectile
            
            if (_fireInput.IsPressed)
            {
                if (elemBuffer.Count == 0)
                {
                    System.Console.WriteLine("Need to load element to shoot");
                }
                else
                {
                    animation = "Casting";
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
                        else if (elemBuffer.Contains(3)) { type = 3; }
                    }
                    else if (elemBuffer.Count == 2)
                    {
                        if (elemBuffer.Contains(1))
                        {
                            if (elemBuffer.Contains(2)) { type = 12; }
                            else if (elemBuffer.Contains(3)) { type = 13; }
                            else { type = 11; }
                        }
                        else if (elemBuffer.Contains(2))
                        {
                            if (elemBuffer.Contains(3)) { type = 23; }
                            else { type = 22; }
                        }
                        else { type = 33; }
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
            
            if (animation != null && !_animator.IsAnimationActive(animation))
                _animator.Play(animation);

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
            Network.outmsg.Write( _velocity.X); 
            Network.outmsg.Write( _velocity.Y);
            Network.outmsg.Write((bool) _fireInputIsPressed);
            Network.outmsg.Write((int) fireType);
            Network.outmsg.Write((float) projectileDirX);
            Network.outmsg.Write((float) projectileDirY);
            Network.outmsg.Write((int) PlatformerScene.playerKillComponent.kills);
            Network.outmsg.Write((int) PlatformerScene.playerMana.mana);
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);

            reload -= 1;

            if (reload <= 0 && PlatformerScene.playerMana.mana < 5)
            {
                PlatformerScene.playerMana.mana += 1;
                PlatformerScene.playerMana.Entity.GetComponent<TextComponent>().Text =
                    PlatformerScene.playerMana.playerName + "'s Mana: " + PlatformerScene.playerMana.mana;
                reload = 120;
            } 
            // else if (!reload)
            // {
            //     Core.Schedule(5f, timer => reload = true);
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