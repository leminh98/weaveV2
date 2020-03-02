using Nez.Samples.Scenes.CharacterSelection;
using Nez.Samples.Scenes.EndGame;
using Nez.Samples.Scenes.Intro;

namespace Nez.Samples
{
	public class Game1 : Core
	{
		protected override void Initialize()
		{
			base.Initialize();

			Window.AllowUserResizing = true;
			Nez.Core.PauseOnFocusLost = false;
			Scene = new TitleScene();
		}
	}
}