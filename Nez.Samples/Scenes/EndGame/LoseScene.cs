using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.UI;

namespace Nez.Samples.Scenes.EndGame
{
    public class LoseScene: Scene
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
            
            
            var moonTex = Content.Load<Texture2D>(Nez.Content.Shared.Moon);
            var playerEntity = CreateEntity("player", new Vector2(Screen.Width / 5, Screen.Height / 3));
            var moonComponent = playerEntity.AddComponent(new SpriteRenderer(moonTex));
            moonComponent.RenderLayer = 75;
            moonComponent.Color = Color.MediumVioletRed;
            playerEntity.Transform.SetScale(new Vector2(2, 2));
            
            // var titleArt = Texture2D.FromStream(Nez.Core.GraphicsDevice, TitleContainer.OpenStream("Content/Intro/Title.png")); 
            var titleArt = Content.Load<Texture2D>("Intro/Title");
            var titleEntity = CreateEntity("title", new Vector2(Screen.Width/2, Screen.Height / 3));
            var playerComponent = titleEntity.AddComponent(new SpriteRenderer(titleArt));
            playerComponent.RenderLayer = 50;
            titleEntity.Transform.SetScale(new Vector2(2, 2));
            
            Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            Canvas.IsFullScreen = true;
            Canvas.RenderLayer = 100;
            _table = Canvas.Stage.AddElement(new Table());
            _table.SetFillParent(true).Center();
            var titleBg = Content.Load<Texture2D>("Intro/TitleBG");
            _table.SetBackground(new SpriteDrawable(titleBg));
            
            Label nameLabel = new Label("Oh my! It seems like you have lost!\n Where did the crown go?");
            nameLabel.SetFontScale(2);
            _table.Add(nameLabel).Center().SetPrefWidth(250).SetMinHeight(50);

        }
        
        
    }
}