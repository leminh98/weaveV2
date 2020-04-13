using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Nez.Sprites;
using Nez.Tweens;
using Nez.UI;

namespace Nez.Samples.Scenes.CharacterSelection
{
    public class CharacterSelectionScene : SampleScene
    {
        public static string chosenSprite;
        List<Button> _sceneButtons = new List<Button>();
        public UICanvas Canvas;
        Table _table;
        private Song song;
        public TextButton continueButton;
        
        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1200, 650);

            var backgroundTexture = Content.Load<Texture2D>("CharacterSelection/characterSelectBackground");
            var background = CreateEntity("bg", new Vector2(Screen.Width/2, Screen.Height/2));
            background.AddComponent(new SpriteRenderer(backgroundTexture));
            
            song = Content.Load<Song>("Platformer/music");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            
            for (int i = 0; i < 4; i++)
            {
                var charTexture2d = Content.Load<Texture2D>("CharacterSelection/charselect" + i.ToString());
                var charEntity = CreateEntity("player" + i.ToString(), 
                    new Vector2( (60 + 225/2f) * (i + 1) + 225/2f * i, 350));
                charEntity.AddComponent(new SpriteRenderer(charTexture2d));
                charEntity.AddComponent(new BoxCollider(-225/2f, -200, 225,400));
            }

            var mouseCursorEntity = CreateEntity("charCursor", new Vector2(Screen.Width/2, Screen.Height/2));
            var mouseCollider = mouseCursorEntity.AddComponent(new BoxCollider(-64, -32, 128, 50));
            Flags.SetFlagExclusive(ref mouseCollider.CollidesWithLayers, 0);
            var mouseComponent = mouseCursorEntity.AddComponent(new CharacterSelectionCursor());

            var mouseCursorTextEntity = CreateEntity("charCursorText");
            mouseCursorTextEntity.Parent = mouseCursorEntity.Transform;
            mouseCursorTextEntity.SetScale(2);
            var nameText = mouseCursorTextEntity.AddComponent(new TextComponent());
            nameText.Text = mouseComponent.name;
            nameText.Color = Color.Black;
            nameText.SetVerticalAlign(VerticalAlign.Bottom);
            nameText.SetHorizontalAlign(HorizontalAlign.Center);
            
            #region Other player's cursor

            foreach (var player in OtherPlayer.players.Where(p => !p.name.Equals(LoginScene._playerName)))
            {
                System.Console.WriteLine(player.name);
                var otherPlayerCursor = new OtherCharacterSelectionCursor(player.name);
                var otherPlayerMouseCursorEntity = CreateEntity("charCursor_" + player.name, new Vector2(Screen.Width/2, Screen.Height/2));
                otherPlayerMouseCursorEntity.AddComponent(otherPlayerCursor);
            }
            #endregion
            
            // #region Continue button
            // var continueButtonStyle = new TextButtonStyle(
            //     new PrimitiveDrawable(new Color(38,41,6)),
            //     new PrimitiveDrawable(new Color(244, 23, 135)), 
            //     new PrimitiveDrawable(new Color(168, 207, 115)))
            // {
            //     FontColor = Color.White,
            //     DownFontColor = Color.Black
            // };
            //
            // Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            // Canvas.IsFullScreen = true;
            //
            // continueButton = new TextButton("Continue", continueButtonStyle);
            // continueButton.SetPosition(500,575);
            // continueButton.SetWidth(200);
            // continueButton.SetHeight(50);
            // // continueButton.SetPosition(500, 625);
            // Canvas.Stage.AddElement(continueButton);
            // continueButton.GetLabel().SetFontScale(2);
            // continueButton.SetDisabled(true);
            //
            // continueButton.OnClicked += butt =>
            // {
            //     // stop all tweens in case any demo scene started some up
            //     TweenManager.StopAllTweens();
            //     Network.playerSelectionPhaseDone = true;
            //     Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(MapSelectionScene)) as Scene));
            // };
            // #endregion
        }
    }
}