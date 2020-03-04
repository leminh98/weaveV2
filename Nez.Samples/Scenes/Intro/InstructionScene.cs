using System;
using System.Collections.Generic;
using Lidgren.Network;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Samples.Scenes.CharacterSelection;
using Nez.Tweens;
using Nez.UI;

namespace Nez.Samples.Scenes.Intro
{
    public class InstructionScene: Scene
    {
        public UICanvas Canvas;
        Table _table;
        public static TextButton continueButton;
        
        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1200, 650);
            
            #region Continue button
            var continueButtonStyle = new TextButtonStyle(
                new PrimitiveDrawable(new Color(38,41,6)),
                new PrimitiveDrawable(new Color(244, 23, 135)), 
                new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                FontColor = Color.White,
                DownFontColor = Color.Black
            };
            
            Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            Canvas.IsFullScreen = true;
            
            continueButton = new TextButton("Ready", continueButtonStyle);
            continueButton.SetPosition(500,575);
            continueButton.SetWidth(200);
            continueButton.SetHeight(50);
            // continueButton.SetPosition(500, 625);
            Canvas.Stage.AddElement(continueButton);
            continueButton.GetLabel().SetFontScale(2);
            
            continueButton.OnClicked += butt =>
            {
                // stop all tweens in case any demo scene started some up
                Network.outmsg = Network.Client.CreateMessage();
                Network.outmsg.Write("ready");
                Network.outmsg.Write(LoginScene._playerName);
                Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered); 
                continueButton.SetDisabled(true);
                continueButton.SetText("Waiting for other to connect....");
            };
            #endregion
            
            // Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            // Canvas.IsFullScreen = true;
            // Canvas.RenderLayer = 100;
            // _table = Canvas.Stage.AddElement(new Table());
            // _table.SetFillParent(true).Center();
            // var titleBg = Content.Load<Texture2D>("Intro/TitleBG");
            // _table.SetBackground(new SpriteDrawable(titleBg));
            //
            // var buttonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98), 10f),
            //     new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            // {
            //     DownFontColor = Color.Black
            // };
            //
            //
            // button = _table.Add(new TextButton("Proceed...", buttonStyle)).SetFillX()
            //     .SetMinHeight(50).SetMinWidth(200).GetElement<TextButton>();
            // button.GetLabel().SetFontScale(2, 2);
            // button.SetDisabled(true);
            //
            // button.OnClicked += butt =>
            // {
            //     // stop all tweens in case any demo scene started some up
            //     TweenManager.StopAllTweens();
            //     Network.connectPhaseDone = true;
            //     Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(CharacterSelectionScene)) as Scene));
            // };
        }
    }
}