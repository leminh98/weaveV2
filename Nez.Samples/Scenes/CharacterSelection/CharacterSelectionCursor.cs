using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.UI;

namespace Nez.Samples.Scenes.CharacterSelection
{
    public class CharacterSelectionCursor: Component, IUpdatable
    {
        private Collider _collider;
        private bool hasChosenCharacter = false;
        public string name = "Updating..";
        float _moveSpeed = 500f;
        public override void Initialize()
        {
            // load up our character texture atlas. we have different characters in 1 - 6.png for variety
            var texture = Entity.Scene.Content.Load<Texture2D>("CharacterSelection/CharCursor");

            name = LoginScene._playerName;
            var textBox = Entity.AddComponent(new SpriteRenderer(texture)).RenderLayer;
            _collider = Entity.GetComponent<Collider>();
        }

        public void Update()
        {
            Entity.SetPosition(Input.ScaledMousePosition);

            if (!Input.LeftMouseButtonPressed|| hasChosenCharacter != false) return;
            
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
                    neighbor.Entity.GetComponent<SpriteRenderer>().Color = Color.Gray;
                    if (selectedChar.Contains("character"))
                    {
                        // System.Console.WriteLine("Character chose: " + selectedMap);
                        // Network.outmsg = Network.Client.CreateMessage();
                        // Network.outmsg.Write("characterSelect");
                        // Network.outmsg.Write(selectedMap); //sending the deltas
                        // Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.Unreliable);
                        this.hasChosenCharacter = true;
                        break;
                    }
                }
            }
        }
    }
}