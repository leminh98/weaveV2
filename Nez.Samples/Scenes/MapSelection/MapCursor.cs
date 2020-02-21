using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez.Sprites;
using Nez.Textures;

namespace Nez.Samples
{
    public class MapCursor: Component, IUpdatable
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
        }
        
        public void Update()
        {
            Entity.SetPosition(Input.ScaledMousePosition);
            
            if (!Input.LeftMouseButtonPressed|| hasSentMap != false) return;
            
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
            foreach (var neighbor in neighbors)
            {
                // if the neighbor collider is of the same entity, ignore it
                if (neighbor.Entity == Entity)
                {
                    continue;
                }

                if (_collider.CollidesWith(neighbor, out var collisionResult))
                {
                    string selectedChar = neighbor.Entity.Name;
                    
                    //Gray out the selection
                    neighbor.Entity.GetComponent<SpriteRenderer>().Color = Color.Gray;
                    if (selectedChar.Contains("map"))
                    {
                        SendMapSelection(selectedChar);
                        this.hasSentMap = true;
                        break;
                    }
                }
            }
        }
        
        private void SendMapSelection(string mapType)
        {
            Network.outmsg = Network.Client.CreateMessage();
            Network.outmsg.Write("mapSelect");
            Network.outmsg.Write(LoginScene._playerName);
            Network.outmsg.Write(mapType); 
            //This message does not necessary need to be received by the server
            Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.UnreliableSequenced); 
        }
    }
}