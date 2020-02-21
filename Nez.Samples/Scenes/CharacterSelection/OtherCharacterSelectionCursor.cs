using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using TwoMGFX;

namespace Nez.Samples.Scenes.CharacterSelection
{
    public class OtherCharacterSelectionCursor: Component
    {
        // public static List<OtherCharacterSelectionCursor> otherCursorList = new List<OtherCharacterSelectionCursor>();
        public string name = "Updating..";

        public OtherCharacterSelectionCursor(string name)
        {
            this.name = name;
        }

        public override void OnAddedToEntity()
        {
            var texture = Entity.Scene.Content.Load<Texture2D>("CharacterSelection/CharCursor");
            var textBox = Entity.AddComponent(new SpriteRenderer(texture)).RenderLayer;
            
            var mouseCursorTextEntity = Core.Scene.CreateEntity("charCursorText_" + name);
            mouseCursorTextEntity.Parent = this.Entity.Transform;
            mouseCursorTextEntity.SetScale(2);
            var nameText = mouseCursorTextEntity.AddComponent(new TextComponent());
            nameText.Text = name;
            nameText.Color = Color.Green;
            nameText.SetVerticalAlign(VerticalAlign.Bottom);
            nameText.SetHorizontalAlign(HorizontalAlign.Center);
        }

        public void Update(Vector2 position)
        {
            this.Entity.Position = position;
        }

        public void DisableCharacterSelectionForSprite(string spriteName)
        {
            // Get the collider, and set its physic layer to 1 instead of 0
            // (the player cursor collides with 0)
            var charCollider = Core.Scene.FindEntity(spriteName).GetComponent<BoxCollider>();
            charCollider.Entity.GetComponent<SpriteRenderer>().Color = Color.Gray;
            Flags.SetFlagExclusive(ref charCollider.PhysicsLayer, 1);
            //Set the cursor to the top of the character sprite
            Entity.Position = charCollider.Entity.Position - new Vector2(100, 0); 
        }

    }
}