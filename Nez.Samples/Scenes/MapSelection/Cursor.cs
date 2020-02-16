using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez.Sprites;
using Nez.Textures;

namespace Nez.Samples
{
    public class Cursor: Component, IUpdatable
    {
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        private VirtualButton _selectInput;
        private Collider _collider;
        private bool hasSentMap = false;
        Mover _mover;
        float _moveSpeed = 500f;
        
        public override void OnAddedToEntity()
        {
            // load up our character texture atlas. we have different characters in 1 - 6.png for variety
            var texture = Entity.Scene.Content.Load<Texture2D>("MapSelection/MapCursor");
        
            _mover = Entity.AddComponent(new Mover());
            Entity.AddComponent(new SpriteRenderer(texture));
        
            _collider = Entity.GetComponent<Collider>();
            SetupInput();
        }
        void SetupInput()
        {
            _selectInput = new VirtualButton();
            _selectInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));
            
            
            _xAxisInput = new VirtualIntegerAxis();
            _xAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D));
            
            _yAxisInput = new VirtualIntegerAxis();
            _yAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.W, Keys.S));
        }
        public void Update()
        {
            var moveDir = new Vector2(_xAxisInput.Value, _yAxisInput.Value);
            
            if (moveDir != Vector2.Zero)
            {
                Entity.Position += moveDir * _moveSpeed * Time.DeltaTime;
            }


            if (!_selectInput.IsPressed || hasSentMap != false) return;
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
            System.Console.WriteLine("ha"); 
            foreach (var neighbor in neighbors)
            {
                System.Console.WriteLine("lol");
                // if the neighbor collider is of the same entity, ignore it
                if (neighbor.Entity == Entity)
                {
                    continue;
                }

                if (_collider.CollidesWith(neighbor, out var collisionResult))
                {
                    string selectedMap = neighbor.Entity.Name;
                    if (selectedMap.Contains("map"))
                    {
                        System.Console.WriteLine("Map chose: " + selectedMap);
                        Network.outmsg = Network.Client.CreateMessage();
                        Network.outmsg.Write("mapSelect");
                        Network.outmsg.Write(selectedMap); //sending the deltas
                        Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);
                        this.hasSentMap = true;
                        break;
                    }
                }
            }
        }
    }
}