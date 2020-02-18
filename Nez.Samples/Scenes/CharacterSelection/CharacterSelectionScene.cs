using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Tweens;
using Nez.UI;

namespace Nez.Samples.Scenes.CharacterSelection
{
    public class CharacterSelectionScene : SampleScene
    {
        public static bool charSelected;
        public static string chosenMap;
        List<Button> _sceneButtons = new List<Button>();
        public UICanvas Canvas;
        Table _table;
        
        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1200, 650);
            
            var backgroundTexture = Content.Load<Texture2D>("CharacterSelection/characterSelectBackground");
            var background = CreateEntity("bg", new Vector2(Screen.Width/2, Screen.Height/2));
            background.AddComponent(new SpriteRenderer(backgroundTexture));
            
            for (int i = 0; i < 4; i++)
            {
                var charTexture2d = Content.Load<Texture2D>("CharacterSelection/charselect" + i.ToString());
                var charEntity = CreateEntity("character" + i.ToString(), 
                    new Vector2( (60 + 225/2f) * (i + 1) + 225/2f * i, 350));
                charEntity.AddComponent(new SpriteRenderer(charTexture2d));
                charEntity.AddComponent(new BoxCollider(-225/2f, -200, 225,400));
            }

            var mouseCursorEntity = CreateEntity("charCursor", new Vector2(Screen.Width/2, Screen.Height/2));
            var mouseCollider = mouseCursorEntity.AddComponent(new BoxCollider(-32, -16, 64, 25));
            Flags.SetFlagExclusive(ref mouseCollider.CollidesWithLayers, 0);
            mouseCursorEntity.AddComponent(new CharacterSelectionCursor());
            
            #region Continue button
            var continueButtonStyle = new TextButtonStyle(new PrimitiveDrawable(Color.Lavender, 0f, 10f),
                new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                FontColor = Color.Black,
                DownFontColor = Color.Black
            };
            
            Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            Canvas.IsFullScreen = true;
            // _table = Canvas.Stage.AddElement(new Table());
            // _table.SetFillParent(true).Bottom();
            //
            // var continueButton = _table.Add(new TextButton("Continue", continueButtonStyle)).SetPrefWidth(200)
            //     .SetMinHeight(50).GetElement<TextButton>();
            var continueButton = new TextButton("Continue", continueButtonStyle);
            continueButton.SetPosition(500,575);
            continueButton.SetWidth(200);
            continueButton.SetHeight(50);
            // continueButton.SetPosition(500, 625);
            Canvas.Stage.AddElement(continueButton);
            continueButton.GetLabel().SetFontScale(2);
            continueButton.OnClicked += butt =>
            {
                // stop all tweens in case any demo scene started some up
                TweenManager.StopAllTweens();
                Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(CharacterSelectionScene)) as Scene));
            };
            #endregion
        }
    }
}