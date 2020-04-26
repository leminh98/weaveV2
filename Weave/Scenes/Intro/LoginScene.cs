using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.UI;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;
using System.Linq;
using System.Reflection;
using Nez.ImGuiTools;
using Nez.Console;
using Nez.Sprites;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Content.Pipeline.BitmapFonts;
using Nez.Samples.Scenes.Intro;

namespace Nez.Samples
{
	/// <summary>
	/// this entire class is one big sweet hack job to make adding samples easier. An exceptional hack is made so that we can render small
	/// pixel art scenes pixel perfect and still display our UI at a reasonable size.
	/// </summary>
	public class LoginScene : Scene
	{
		public static string _playerName = "Minh";
		public static string _serverIp;
		public static string _characterSpriteType = "0";
		public static int playerIndex;
		public static int numPlayer = 1;
		private Song song;

		public UICanvas Canvas;
		Table _table;
		List<Button> _sceneButtons = new List<Button>();
        
		public override void Initialize()
		{
			base.Initialize();

			// default to 1280x720 with no SceneResolutionPolicy
			SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
			Screen.SetSize(1200, 650);
			
			song = Content.Load<Song>("Platformer/music");
			MediaPlayer.Play(song);
			MediaPlayer.IsRepeating = true;
			
			// // Adding title
			// var titleArt = Content.Load<Texture2D>("Intro/Title");
			// var titleEntity = CreateEntity("title", new Vector2(Screen.Width/2, Screen.Height / 5));
			// var playerComponent = titleEntity.AddComponent(new SpriteRenderer(titleArt));
			// playerComponent.RenderLayer = 50;
			// titleEntity.Transform.SetScale(new Vector2(2, 2));
			
			//Initialize the canvas            
			Canvas = CreateEntity("ui").AddComponent(new UICanvas());
			Canvas.IsFullScreen = true;
			Canvas.RenderLayer = 100;
			_table = Canvas.Stage.AddElement(new Table());
			_table.SetFillParent(true).Center();
			
			var titleBG = Content.Load<Texture2D>("Intro/TitleBG");
			_table.SetBackground(new SpriteDrawable(titleBG));
            
			var skin = Skin.CreateDefaultSkin();
			var font = Graphics.Instance.BitmapFont;
			var textFieldStyle = // new TextFieldStyle(null, Color.White);
				new TextFieldStyle(font, Color.White, skin.GetDrawable("cursor"), skin.GetDrawable("selection"),
					new PrimitiveDrawable(new Color(125,125,5,0))); 

			#region Name field
			Label nameLabel = new Label("Name:");
			nameLabel.SetFontScale(4);
			
			nameLabel.SetAlignment(Align.TopLeft);
			nameLabel.SetPosition(120, 325);
			Canvas.Stage.AddElement(nameLabel);
			
			TextField nameText = new TextField("Minh", textFieldStyle);
			nameText.SetSize(580, 100);
			nameText.SetAlignment(Align.TopLeft);
			nameText.SetPosition(120, 340);
			Canvas.Stage.AddElement(nameText);
			
			nameText.SetFontScale(3);
			#endregion
			
			#region Ip field
			Label ipLabel = new Label("Server IP address:");
			ipLabel.SetFontScale(4);
			
			ipLabel.SetAlignment(Align.TopLeft);
			ipLabel.SetPosition(120, 500);
			Canvas.Stage.AddElement(ipLabel);

	
			var localIp = "";
			try
			{
				
				var host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (var ip in host.AddressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						localIp =  ip.ToString();
					}
				}
			} catch 
			{}
			
			
			
			TextField ipText = new TextField(localIp, //get your current ip adress
				textFieldStyle);
			ipText.SetSize(580, 100);
			ipText.SetAlignment(Align.TopLeft);
			ipText.SetPosition(120, 515);
			ipText.SetFontScale(3);
			Canvas.Stage.AddElement(ipText);
			#endregion
			
			if (_playerName != null)
			{
				nameText.SetText(_playerName);
			}

			if (_serverIp != null)
			{
				ipText.SetText(_serverIp);
			}
			
			#region Continue button
			var continueButtonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color (232, 106, 115,50)),
				new PrimitiveDrawable(new Color(232, 106, 115,50)), new PrimitiveDrawable(new Color(232, 106, 115,70)))
			{
				FontColor = Color.White,
				DownFontColor = Color.White
			};

			
			var continueButton = new TextButton("Connect", continueButtonStyle);
			continueButton.SetWidth(375);
			continueButton.SetHeight(325);
			continueButton.SetPosition(725, 300);
			continueButton.GetLabel().SetFontScale(4);
			Canvas.Stage.AddElement(continueButton);
			
			continueButton.OnClicked += butt =>
			{
				// stop all tweens in case any demo scene started some up
				TweenManager.StopAllTweens();
				_playerName = nameText.GetText().Trim();
				_serverIp = ipText.GetText();
				
				// var networkComponent = GetOrCreateSceneComponent<Network>();
				// NetworkComponent.SetEnabled(true);
				var networkService = new Network();
				Core.RegisterGlobalManager(networkService);
				networkService.Start();
				Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(InstructionScene)) as Scene));
			};
			#endregion
		}


	}
}

