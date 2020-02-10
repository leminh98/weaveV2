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

		public UICanvas Canvas;
		Table _table;
		List<Button> _sceneButtons = new List<Button>();
        
		public override void Initialize()
		{
			base.Initialize();

			// default to 1280x720 with no SceneResolutionPolicy
			SetDesignResolution(640, 480, SceneResolutionPolicy.ShowAllPixelPerfect);
			Screen.SetSize(640, 480);

			//Initialize the canvas            
			Canvas = CreateEntity("ui").AddComponent(new UICanvas());
			Canvas.IsFullScreen = true;
			Canvas.RenderLayer = 100;
			_table = Canvas.Stage.AddElement(new Table());
			_table.SetFillParent(true).Center();
            
			Label nameLabel = new Label("Name:");
			_table.Add(nameLabel);
			TextField nameText = new TextField("Minh", Skin.CreateDefaultSkin());
			_table.Add(nameText);
			_table.Row();
			
			Label ipLabel = new Label("Server IP address:");
			_table.Add(ipLabel);

			string localIp = "";
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIp =  ip.ToString();
				}
			}
			TextField ipText = new TextField(localIp, //get your current ip adress
				Skin.CreateDefaultSkin());
			_table.Add(ipText);odi
			_table.Row();
			
			var buttonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98), 10f),
				new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
			{
				DownFontColor = Color.Black
			};
            
			var button = _table.Add(new TextButton("Connect", buttonStyle)).SetFillX().SetColspan(2)
				.SetMinHeight(30).GetElement<TextButton>();
			
			_sceneButtons.Add(button);
			button.OnClicked += butt =>
			{
				// stop all tweens in case any demo scene started some up
				TweenManager.StopAllTweens();
				_playerName = nameText.GetText().Trim();
				_serverIp = ipText.GetText();
				Core.StartSceneTransition(new FadeTransition(() => Activator.CreateInstance(typeof(PlatformerScene)) as Scene));
			};
		}


	}
}