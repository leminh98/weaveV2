using System.Collections.Generic;
using Lidgren.Network;
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
        public static TextButton restartGameButton;
        public static TextButton exitButton;
        
        public override void Initialize()
        {
            base.Initialize();

            // default to 1280x720 with no SceneResolutionPolicy
            SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1200, 650);
            
            
            Canvas = CreateEntity("ui").AddComponent(new UICanvas());
            Canvas.IsFullScreen = true;
            Canvas.RenderLayer = 100;
            _table = Canvas.Stage.AddElement(new Table());
            _table.SetFillParent(true).Center();
            var titleBg = Content.Load<Texture2D>("EndGame/loseBG");
            _table.SetBackground(new SpriteDrawable(titleBg));
            
            Label nameLabel = new Label("Ah Geez!\n You lost. Better luck next time!");
            nameLabel.SetFontScale(3);
            _table.Add(nameLabel).Center().SetPrefWidth(350).SetMinHeight(50);
            
            
            var restartGameButtonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98), 10f),
                new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                DownFontColor = Color.Black
            };

            _table.Row();
            restartGameButton = _table.Add(new TextButton("New Game", restartGameButtonStyle)).SetFillX()
                .SetMinHeight(50).SetMinWidth(250).GetElement<TextButton>();
            restartGameButton.GetLabel().SetFontScale(2, 2);
            
            restartGameButton.OnClicked += butt =>
            {
                Network.outmsg = Network.Client.CreateMessage();
                Network.outmsg.Write("restart");
                Network.outmsg.Write(LoginScene._playerName);
                Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered); 
                restartGameButton.SetDisabled(true);
                restartGameButton.SetText("Waiting for other to restart....");
            };

            _table.Row();
            exitButton = _table.Add(new TextButton("Exit Game", restartGameButtonStyle)).SetFillX()
                .SetMinHeight(50).SetMinWidth(250).GetElement<TextButton>();
            exitButton.GetLabel().SetFontScale(2, 2);
            
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