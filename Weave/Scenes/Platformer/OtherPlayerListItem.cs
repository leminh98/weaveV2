using Microsoft.Xna.Framework;

namespace Nez.Samples
{
    public class OtherPlayerListItem
    {
        public string name;
        public int playerIndex;
        public string playerSprite;

        public OtherPlayerListItem(string name, int playerIndex)
        {
            this.name = name;
            this.playerIndex = playerIndex;
            this.playerSprite = "";
        }
    }
}