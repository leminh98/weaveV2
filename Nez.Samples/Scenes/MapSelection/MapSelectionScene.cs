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
                
                var titleBg = Content.Load<Texture2D>("MapSelection/Map" + i);
                var titleBGEntity = CreateEntity("Map" + i.ToString(), 
                    new Vector2(Screen.Width/4 + ((i % 2) * Screen.Width) /2, Screen.Height/4 + ((i > 1 ? 1 : 0) * Screen.Height) /2));
                titleBGEntity.AddComponent(new SpriteRenderer(titleBg));
            }
            
            var buttonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98), 10f),
                new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                DownFontColor = Color.Black
            };
            
            #region Continue button
            var continueButtonStyle = new TextButtonStyle(new PrimitiveDrawable(Color.Lavender, 0f, 10f),
                new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                FontColor = Color.Black,
                DownFontColor = Color.Black
            };
            
            var continueButton = _table.Add(new TextButton("Connect", continueButtonStyle)).SetFillX().SetColspan(12)
                .SetMinHeight(50).GetElement<TextButton>();
            continueButton.GetLabel().SetFontScale(2);
			
            continueButton.OnClicked += butt =>
            {
                // stop all tweens in case any demo scene started some up
                TweenManager.StopAllTweens();
                Core.StartSceneTransition(new FadeTransition(() =>
                {
                    var scene =  new PlatformerScene();
                    // scene.AddSceneComponent(networkComponent);
                    return scene as Scene;

                }));
            };
            #endregion
        }
        
        
    }
}