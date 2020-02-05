using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tiled;
using Nez.Sprites;
using Nez.Textures;

namespace Nez.Samples
{
	[SampleScene("Platformer", 120, "Work in progress...\nArrows, d-pad or left stick to move, z key or a button to jump")]
	public class PlatformerScene : SampleScene
	{
		public PlatformerScene() : base(true, true)
		{}


		public override void Initialize()
		{
			// setup a pixel perfect screen that fits our map
			SetDesignResolution(640, 480, SceneResolutionPolicy.ShowAllPixelPerfect);
			Screen.SetSize(640 * 2, 480 * 2);

			// load up our TiledMap
			var map = Content.LoadTiledMap("Content/Platformer/tiledMap.tmx");
			var spawnObject = map.GetObjectGroup("objects").Objects["spawn"];

			var tiledEntity = CreateEntity("tiled-map-entity");
			tiledEntity.AddComponent(new TiledMapRenderer(map, "main"));


			// create our Player and add a TiledMapMover to handle collisions with the tilemap
			var playerEntity = CreateEntity("player", new Vector2(spawnObject.X, spawnObject.Y));
			playerEntity.AddComponent(new Caveman());
			var collider = playerEntity.AddComponent(new BoxCollider(-8, -16, 16, 32));
			playerEntity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("main")));
			
			Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			Flags.SetFlagExclusive(ref collider.PhysicsLayer, 1);
			
			var moonTexture = Content.Load<Texture2D>(Nez.Content.Shared.Moon);
			var moonEntity = CreateEntity("moon", new Vector2(412, 460));
			moonEntity.AddComponent(new SpriteRenderer(moonTexture));
			moonEntity.AddComponent(new ProjectileHitDetector());
			moonEntity.AddComponent<CircleCollider>();

			AddPostProcessor(new VignettePostProcessor(1));
		}
		
		/// <summary>
		/// creates a projectile and sets it in motion
		/// </summary>
		public Entity CreateProjectiles(Vector2 position, Vector2 velocity)
		{
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("projectile");
			entity.Position = position;
			entity.AddComponent(new ProjectileMover());
			entity.AddComponent(new BulletProjectileController(velocity));

			// add a collider so we can detect intersections
			var collider = entity.AddComponent<CircleCollider>();
			Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			Flags.SetFlagExclusive(ref collider.PhysicsLayer, 1);


			// load up a Texture that contains a fireball animation and setup the animation frames
			var texture = Content.Load<Texture2D>(Nez.Content.NinjaAdventure.Plume);
			var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);

			// add the Sprite to the Entity and play the animation after creating it
			var animator = entity.AddComponent(new SpriteAnimator());

			// render after (under) our player who is on renderLayer 0, the default
			animator.RenderLayer = 1;

			animator.AddAnimation("default", sprites.ToArray());
			animator.Play("default");

			//
			// // clone the projectile and fire it off in the opposite direction
			// var newEntity = entity.Clone(entity.Position);
			// newEntity.GetComponent<FireballProjectileController>().Velocity *= -1;
			// AddEntity(newEntity);

			return entity;
		}
	}
}