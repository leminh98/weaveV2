using System;
using System.Collections.Generic;
using Lidgren.Network;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Nez.Samples.Scenes.CharacterSelection;
using Nez.Tweens;
using Nez.UI;

namespace Nez.Samples.Scenes.Intro
{
    public class InstructionScene : Scene
    {
        public UICanvas Canvas;
        Table _table;
        public static TextButton continueButton;
        private Song song;

        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1200, 650);
            
            song = Content.Load<Song>("Platformer/music");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = LoginScene.MasterVolume;
            
            #region Continue button

            var continueButtonStyle = new TextButtonStyle(
                new PrimitiveDrawable(new Color(232, 106, 115,50)),
                new PrimitiveDrawable(new Color(244, 23, 135, 0)),
                new PrimitiveDrawable(new Color(232, 106, 115,70)))
            {
                FontColor = Color.White,
                DownFontColor = Color.Black
            };

            Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            Canvas.IsFullScreen = true;

            _table = Canvas.Stage.AddElement(new Table());
            _table.SetFillParent(true).Center();
            var instructionBG = Content.Load<Texture2D>("Intro/InstructionBG");
            _table.SetBackground(new SpriteDrawable(instructionBG));

            Label titleLabel = new Label("");
            Label introLabel = new Label("");
            Label elementListLabel = new Label("");
            var titleText = "Hello there! Ambitious Mage " + LoginScene._playerName;
            // var introText =
            //     "Welcome to the world of Weave!\n" +
            //     "The battle arena is starting soon,\n" +
            //     "so you should get ready.\n" +
            //     "Here's a quick recap of what is allowed \n" +
            //     "in the Weave Battle Arena:\n" +
            //     "\n"+
            //     "[A]/[D] -- Move Left / Right\n" +
            //     "[W] -- Jump\n" +
            //     "[Space] -- Climb\n" +
            //     "\n" +
            //     "I hope you are not too nervous to forget \n" +
            //     "how to cast spells!\n" +
            //     "\n" +
            //     "[Elemental Keys] -- Charge elements\n" +
            //     "[Left Click] -- Aim and cast spell after \n" +
            //     "elements are charged\n"+
            //     "\n";
            //
            // var elementList =
            //     "Only Water (bind to Key [1]) \n" +
            //     "and Earth (bind to Key [2]) elemental spells \n" +
            //     "are allowed for the safety of all battlers.\n" +
            //     "\n" +
            //     "Here's a quick list of all elemental combo:\n" +
            //     "\n" +
            //     "[Water] -- Shield\n" +
            //     "[Water] + [Water] -- Shoot Water Jet\n" +
            //     "[Earth] -- Throw Bouncing Pebble\n" +
            //     "[Earth] + [Earth] -- Throw Bouncing Boulders\n" +
            //     "[Water] + [Earth] -- Grow A Vine\n" +
            //     "\n" +
            //     "Of you are ready, click the button below";
            var introText = "";
            var elementList = "";
            Canvas.Stage.AddElement(titleLabel);
            Canvas.Stage.AddElement(introLabel);
            Canvas.Stage.AddElement(elementListLabel);
            
            
            titleLabel.SetText(titleText).SetFontScale(3).SetAlignment(Align.Center);
            introLabel.SetText(introText).SetFontScale(2).SetAlignment(Align.TopLeft);
            elementListLabel.SetText(elementList).SetFontScale(2).SetAlignment(Align.TopLeft);

            titleLabel.SetPosition(600, 35);
            introLabel.SetPosition(115, 90);
            elementListLabel.SetPosition(625, 90);
            
           
            
            continueButton = new TextButton("I'm Ready!", continueButtonStyle);
            continueButton.SetPosition(100, 500);
            continueButton.SetWidth(1000);
            continueButton.SetHeight(75);
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
                continueButton.SetText("Waiting for other mages to connect....");
            };

            #endregion

        }
    }
}