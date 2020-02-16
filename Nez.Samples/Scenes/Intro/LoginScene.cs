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
using Nez.Samples.Scenes.Intro;

namespace Nez.Samples
{
	/// <summary>
	/// this entire class is one big sweet hack job to make adding samples easier. An exceptional hack is made so that we can render small
	/// pixel art scenes pixel perfect and still display our UI at a reasonable size.
	/// </summary>
	public class LoginScene : Scene
	{
		public static string _playerName;
		public static string _serverIp;
		public static string _characterSpriteType = "0";

		public UICanvas Canvas;
		Table _table;
		List<Button> _sceneButtons = new List<Button>();
        
		public override void Initialize()
		{
			base.Initialize();

			// default to 1280x720 with no SceneResolutionPolicy
			SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
			Screen.SetSize(1200, 650);
			
			// Adding title
			var titleArt = Content.Load<Texture2D>("Intro/Title");
			var titleEntity = CreateEntity("title", new Vector2(Screen.Width/2, Screen.Height / 5));
			var playerComponent = titleEntity.AddComponent(new SpriteRenderer(titleArt));
			playerComponent.RenderLayer = 50;
			titleEntity.Transform.SetScale(new Vector2(2, 2));
			
			//Initialize the canvas            
			Canvas = CreateEntity("ui").AddComponent(new UICanvas());
			Canvas.IsFullScreen = true;
			Canvas.RenderLayer = 100;
			_table = Canvas.Stage.AddElement(new Table());
			_table.SetFillParent(true).Center();
            
			#region Name field
			Label nameLabel = new Label("Name:");
			nameLabel.SetFontScale(2);
			_table.Add(nameLabel).Center().Left().SetPrefWidth(250).SetMinHeight(50).SetColspan(5);
			
			TextField nameText = new TextField("Minh", Skin.CreateDefaultSkin());
			nameText.SetScale(2);
			_table.Add(nameText).SetPrefWidth(350).Fill().SetColspan(7);
			_table.Row();
			#endregion
			
			#region Ip field
			Label ipLabel = new Label("Server IP address:");
			ipLabel.SetFontScale(2);
			_table.Add(ipLabel).Center().Left().SetMinHeight(50).SetColspan(5);

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
				Skin.CreateDefaultSkin());

			_table.Add(ipText).Fill().SetColspan(7);
			_table.Row();
			#endregion
			
			#region Character selection buttons
			Label characterSelectionLabel = new Label("Choose your character:");
			characterSelectionLabel.SetFontScale(2);
			_table.Add(characterSelectionLabel).Center().Left().SetPrefWidth(250).SetMinHeight(50).SetColspan(12);
			_table.Row();
			
			var characterButtonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98)),
				new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
			{
				DownFontColor = Color.DarkGray
			};
			var characterSelectedStyle =  new TextButtonStyle(new PrimitiveDrawable(new Color(244, 23, 135)),
				new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
			{
				DownFontColor = Color.DarkGray
			};

			for (int i = 0; i < 4; i++)
			{
				var button = _table.Add(new TextButton(i.ToString(), characterButtonStyle))
					.SetFillX().SetUniformX()
					.SetColspan(3).Center()
					.SetMinHeight(50).GetElement<TextButton>();
				button.GetLabel().SetFontScale(2);
				
				_sceneButtons.Add(button);
			}
			
			var buttonGroup = new ButtonGroup(_sceneButtons.ToArray());
			buttonGroup.SetMaxCheckCount(1);
			buttonGroup.SetMinCheckCount(0);
			buttonGroup.SetUncheckLast(true);

			
			foreach (var button in buttonGroup.GetButtons())
			{
				button.OnClicked += butt =>
				{
					if (button.IsChecked)
					{
						// butt.SetDisabled(true);
						// (new PrimitiveDrawable(new Color(244, 23, 135)));
						
						_characterSpriteType = ((TextButton) butt).GetText();
						System.Console.WriteLine(_characterSpriteType);
						foreach (var otherButton in buttonGroup.GetButtons())
							otherButton.SetStyle(characterButtonStyle);
						butt.SetStyle(characterSelectedStyle);
					}
					else
					{
						// butt.SetStyle(characterButtonStyle);
						System.Console.WriteLine("Rah");
					}
					
				};
			}
			
			_table.Row();
			#endregion
			
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
				_playerName = nameText.GetText().Trim();
				_serverIp = ipText.GetText();
				
				// var networkComponent = GetOrCreateSceneComponent<Network>();
				// NetworkComponent.SetEnabled(true);
				var networkService = new Network();
				Core.RegisterGlobalManager(networkService);
				Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(MapSelectionScene)) as Scene));
			};
			#endregion
		}


	}
}