using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Tiled;


namespace Nez.Samples
{
    /// <summary>
    /// moves a ProjectileMover and destroys the Entity if it hits anything
    /// </summary>
    public class BulletProjectileController : Component, IUpdatable
    {
        public Vector2 Velocity;
        private int Type;
        public string Name;

        TiledMapMover _mover;
        TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
        Collider _collider;


        public BulletProjectileController(string name, Vector2 velocity, int type)
        {
           Velocity = velocity;
           Type = type;
           Name = name;
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<TiledMapMover>();
            _collider = Entity.GetComponent<Collider>();
        }

        void IUpdatable.Update()
        {
            _mover.Move(Velocity * Time.DeltaTime, Entity.GetComponent<BoxCollider>(), _collisionState);
            if (_collisionState.HasCollision)
            {
                if (Type == 12)
                {
                    System.Console.WriteLine("vine");
                    var platformerScene = Entity.Scene as PlatformerScene;
                    platformerScene.CreateVine(Entity.Position);
                }
                Entity.Destroy();
            }
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
            foreach (var neighbor in neighbors)
            {
                var isPlayer = neighbor.Entity.GetComponent<BulletHitDetector>();
                if (isPlayer != null)
                {
                    Entity.Destroy();
                    isPlayer.currentHP--;
                    var notBoss = neighbor.Entity.GetComponent<Caveman>(); //TODO: change this to account for both caveman and OtherPlayer
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
                        bool[] buffer;
                        if (neighbor.Entity.Name.Equals("player"))
                        {
                            
                            buffer = neighbor.Entity.GetComponent<Caveman>().itemBuffer;
                        }
                        else
                        {
                            buffer = neighbor.Entity.GetComponent<OtherPlayer>().itemBuffer;
                        }
                        
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (buffer[i])
                            {
                                System.Console.WriteLine("Dropping at position: " + Entity.Transform.Position);
                                drop.Release(neighbor.Entity.Transform.Position);
                                neighbor.Entity.GetComponent<Caveman>().itemBuffer[drop.itemNum] = false;
                                neighbor.Entity.RemoveComponent(drop);
                                drop = neighbor.Entity.GetComponent<DropItem>();
                            }
                        }

                        var platformerScene = Entity.Scene as PlatformerScene;
                        platformerScene.Respawn(neighbor.Entity, Name);
                        // neighbor.Entity.Destroy();
                        Entity.Destroy();
                        return;
                    }
            
                    isPlayer._sprite.Color = Color.Red;
                    Core.Schedule(0.1f, timer => isPlayer._sprite.Color = Color.White);
                    
                }
            }
            
            if (Type == 1) { Velocity.Y += 50 * Time.DeltaTime; }
            else if (Type != 11) { Velocity.Y += 400 * Time.DeltaTime; }

            
        }
    }
}