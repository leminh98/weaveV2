using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;

namespace Nez.Samples
{
    public class ProjectileHandler
    {
        public Texture2D Bubble;
        public Texture2D Stream;
        public Texture2D Pebble;
        public Texture2D Boulder;
        public Texture2D Vine;
        public Texture2D Seed;

        public ProjectileHandler(NezContentManager content)
        {
            Bubble = content.Load<Texture2D>("Platformer/proj_bubble");
            Stream = content.Load<Texture2D>("Platformer/proj_water_jet"); 
            Pebble = content.Load<Texture2D>("Platformer/proj_pebble");
            Boulder = content.Load<Texture2D>("Platformer/proj_boulder");
            Vine = content.Load<Texture2D>("Platformer/proj_vine");
            Seed = content.Load<Texture2D>("Platformer/proj_seed");
        }

    }
}