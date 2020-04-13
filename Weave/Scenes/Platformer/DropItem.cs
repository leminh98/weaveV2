using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Samples
{
    public class DropItem : Component
    {
        public int itemNum;
        private Texture2D itemTexture;
        private float itemMass;
        private float itemFriction;
        private float itemElasticity;
        private Vector2 position;
        
        public DropItem(int num, Texture2D texture, float mass, float friction, float elasticity)
        {
            itemNum = num;
            itemTexture = texture;
            itemMass = mass;
            itemFriction = friction;
            itemElasticity = elasticity;
        }

        public void Release(Vector2 pos)
        {
            var platformerScene = Entity.Scene as PlatformerScene;
            platformerScene.ReleaseItem(itemNum, pos, itemTexture, itemMass, itemFriction, itemElasticity);
        }
    }
}