using System;
using System.Collections.Generic;
using Lidgren.Network;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Tweens;
using Nez.UI;
namespace Nez.Samples.Scenes.EndGame
{


    public class WinScene: Scene
    {
        public UICanvas Canvas;
        Table _table;
        public static TextButton restartGameButton;
        public static TextButton exitButton;
        
        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1200, 650);
            //
            // var moonTex = Content.Load<Texture2D>(Nez.Content.Shared.Moon);
            // var playerEntity = CreateEntity("player", new Vector2(Screen.Width / 5, Screen.Height / 3));
            // var moonComponent = playerEntity.AddComponent(new SpriteRenderer(moonTex));
            // moonComponent.RenderLayer = 75;
            // moonComponent.Color = Color.MediumVioletRed;
            // playerEntity.Transform.SetScale(new Vector2(2, 2));
            
            
            Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            Canvas.IsFullScreen = true;
            Canvas.RenderLayer = 100;
            _table = Canvas.Stage.AddElement(new Table());
            _table.SetFillParent(true).Center();
            var titleBg = Content.Load<Texture2D>("EndGame/winBG");
            _table.SetBackground(new SpriteDrawable(titleBg));
            
            Label nameLabel = new Label("CONGRATULATION!\n You win!");
            nameLabel.SetFontScale(3);
            _table.Add(nameLabel).Center().SetAlign(Align.Center).SetPrefWidth(350).SetMinHeight(50);
            
            
            var buttonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98), 10f),
                new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                DownFontColor = Color.Black
            };

            _table.Row();
            restartGameButton = _table.Add(new TextButton("New Game", buttonStyle)).SetFillX()
                .SetMinHeight(50).SetMinWidth(250).GetElement<TextButton>();
            restartGameButton.GetLabel().SetFontScale(2, 2);
            
            _table.Row();
            exitButton = _table.Add(new TextButton("Exit Game", buttonStyle)).SetFillX()
                .SetMinHeight(50).SetMinWidth(250).GetElement<TextButton>();
            exitButton.GetLabel().SetFontScale(2, 2);

            restartGameButton.OnClicked += butt =>
            {
                Network.outmsg = Network.Client.CreateMessage();
                Network.outmsg.Write("restart");
                Network.outmsg.Write(LoginScene._playerName);
                Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered); 
                restartGameButton.SetDisabled(true);
                restartGameButton.SetText("Waiting for other to restart....");
            };
            
            exitButton.OnClicked += butt =>
            {
                Network.outmsg = Network.Client.CreateMessage();
                Network.outmsg.Write("disconnect");
                Network.outmsg.Write(LoginScene._playerName);
                Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered); 
                exitButton.SetDisabled(true);
                exitButton.SetText("Disconnecting from server....");
            };

        }
        
        
    }

}