using System;
using System.Collections.Generic;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Tweens;
using Nez.UI;

namespace Nez.Samples
{
    public class MapSelectionScene : Scene
    {
        public static bool mapSelected;
        public static string chosenMap;
        public UICanvas Canvas;
        Table _table;
        List<Button> _sceneButtons = new List<Button>();
        
        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1200, 650);
            
            for (int i = 0; i < 4; i++)
            {
                var mapTexture2D = Content.Load<Texture2D>("MapSelection/Map" + i.ToString());
                var mapEntity = CreateEntity("map" + i.ToString(), 
                    new Vector2(Screen.Width/4 + ((i % 2) * Screen.Width) /2, Screen.Height/4 + ((i > 1 ? 1 : 0) * Screen.Height) /2));
                mapEntity.AddComponent(new SpriteRenderer(mapTexture2D));
                mapEntity.AddComponent(new BoxCollider(-Screen.Width / 4, -Screen.Height / 4, Screen.Width / 2,
                    Screen.Height / 2));
            }

            var mouseCursorEntity = CreateEntity("mouseCursor", new Vector2(Screen.Width/2, Screen.Height/2));
            var mouseCollider = mouseCursorEntity.AddComponent(new BoxCollider(-1, -16, 1, 1));
            Flags.SetFlagExclusive(ref mouseCollider.CollidesWithLayers, 0);
            mouseCursorEntity.AddComponent(new MapCursor());
        }

    }
}