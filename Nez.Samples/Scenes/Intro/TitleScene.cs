using System;
using System.Collections.Generic;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.ImGuiTools;
using Nez.Tweens;
using Nez.UI;

namespace Nez.Samples.Scenes.Intro
{
    public class TitleScene: Scene
    {
        public UICanvas Canvas;
        Table _table;
        List<Button> _sceneButtons = new List<Button>();
        
        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(640, 480, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(640, 480);

            var moonTex = Content.Load<Texture2D>(Nez.Content.Shared.Moon);
            var playerEntity = CreateEntity("player", new Vector2(Screen.Width / 5, Screen.Height / 3));
            playerEntity.AddComponent(new SpriteRenderer(moonTex));
            
            var titleArt = Texture2D.FromStream(Nez.Core.GraphicsDevice, TitleContainer.OpenStream("Content/Intro/Title.png")); 
            var titleEntity = CreateEntity("title", new Vector2(Screen.Width  * 3 / 5, Screen.Height / 3));
            titleEntity.AddComponent(new SpriteRenderer(titleArt));
            
            Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            Canvas.IsFullScreen = true;
            Canvas.RenderLayer = 100;
            _table = Canvas.Stage.AddElement(new Table());
            _table.SetFillParent(true).Center();
            _table.SetBackground(new PrimitiveDrawable(new Color(23, 40, 97, 255)));
            
            var buttonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98), 10f),
                new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                DownFontColor = Color.Black
            };
            
            var button = _table.Add(new TextButton("Start", buttonStyle)).SetFillX()
                .SetMinHeight(30).GetElement<TextButton>();
            _sceneButtons.Add(button);
            button.OnClicked += butt =>
            {
                // stop all tweens in case any demo scene started some up
                TweenManager.StopAllTweens();
                Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(LoginScene)) as Scene));
            };
        }
        
        
    }
}