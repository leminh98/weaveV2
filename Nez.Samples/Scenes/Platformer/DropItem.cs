using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Samples
{
    public class DropItem : Component
    {
        private Item _item;
        
        public DropItem(Item item)
        {
            _item = item;
        }

        public void Release(Vector2 pos)
        {
            var platformerScene = Entity.Scene as PlatformerScene;
            platformerScene.ReleaseItem(pos, _item);
        }
    }
}