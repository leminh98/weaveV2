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

        public static List<OtherPlayerListItem> players = new List<OtherPlayerListItem>(); //contain other players name

        public OtherPlayer(string name, int playerIndex, string spriteType)
        {
            this.name = name;
            this.spriteType = spriteType;
            this.playerIndex = playerIndex;
            this._projDir = new Vector2(0, 0);
        }

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
        }

        public override void OnRemovedFromEntity()
        {
        }

        void IUpdatable.Update()
        {
            // handle movement and animations

            string animation = null;

            if (_velocity.Y < 0 || _velocity.Y > 0)
            {
                animation = "Jumping";
            }

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
                System.Console.WriteLine("SHOT");
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
            else
            {
                _fireInputIsPressed = false;
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