using Microsoft.Xna.Framework;

namespace Nez.Samples
{
    public struct OtherPlayerStruct
    {
        public string name;
        public int playerIndex;
        public string playerSprite;

        public OtherPlayerStruct(string name, int playerIndex)
        {
            this.name = name;
            this.playerIndex = playerIndex;
            this.playerSprite = "";
        }
    }
}