using System;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez.Tiled;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;

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
			SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
			Screen.SetSize(1200, 650);

			// Create background - temporary until we have background graphics
			ClearColor = Color.Indigo;

			// load up our TiledMap
			// var map = Content.LoadTiledMap("Content/Platformer/small_lvl_3_forest.tmx");
			var map = Content.LoadTiledMap("Content/Platformer/prototype_forest_1.tmx");
			var spawnObject = map.GetObjectGroup("objects").Objects["spawn"];
			var tiledEntity = CreateEntity("tiled-map-entity");
			tiledEntity.AddComponent(new TiledMapRenderer(map, "main"));
			

			// create our Player and add a TiledMapMover to handle collisions with the tilemap
			var playerEntity = CreateEntity("player", new Vector2(spawnObject.X, spawnObject.Y));
			var playerComponent = new Caveman(LoginScene._playerName);
			playerEntity.AddComponent(playerComponent);
			playerEntity.AddComponent(new BoxCollider(-8, -16, 12, 32));
			playerEntity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("main")));
			playerEntity.AddComponent(new BulletHitDetector());
			AddHealthBarToEntity(playerEntity);
			
			// Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			// Flags.SetFlagExclusive(ref collider.PhysicsLayer, 1);
			
			// Only set up moving camera if level size requires it.
			if (map.Height > 21 || map.Width > 38)
			{
				// setup our camera bounds with a 1 tile border around the edges (for the outside collision tiles)
				var topLeft = new Vector2(map.TileWidth, map.TileWidth);
				var bottomRight = new Vector2(map.TileWidth * (map.Width - 1),
					map.TileWidth * (map.Height - 1));
				tiledEntity.AddComponent(new WeaveCameraBounds(topLeft, bottomRight));
				Camera.Entity.AddComponent(new FollowCamera(playerEntity));
			}

			var moonTexture = Content.Load<Texture2D>(Nez.Content.Shared.Moon);
			var moonSpawn = map.GetObjectGroup("objects").Objects["boss_spawn"];
			var moonEntity = CreateEntity("moon", new Vector2(moonSpawn.X, moonSpawn.Y));
			moonEntity.AddComponent(new Boss());
			var itemTexture = Content.Load<Texture2D>("Platformer/Temp_Arrow");
			moonEntity.AddComponent(new DropItem(itemTexture, 1f, 0, 0));
			moonEntity.AddComponent(new SpriteRenderer(moonTexture));
			moonEntity.AddComponent(new BulletHitDetector());
			var moonCollider = moonEntity.AddComponent(new CircleCollider(65));
			AddHealthBarToEntity(moonEntity);
			
			// Flags.SetFlagExclusive(ref moonCollider.CollidesWithLayers, 0);
			// Flags.SetFlagExclusive(ref moonCollider.PhysicsLayer, 1);

			OtherPlayer.players.Add(LoginScene._playerName);
			
			// Start the network
			var networkComponent = Core.GetGlobalManager<Network>();
			networkComponent.InitializeGameplay();
		}
		
		/// <summary>
		/// creates a projectile and sets it in motion
		/// </summary>
		public Entity CreateProjectiles(Vector2 position, Vector2 velocity)
		{
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("projectile");
			entity.Position = position;
			entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			entity.AddComponent(new BulletProjectileController(velocity));

			// add a collider so we can detect intersections
			var collider = entity.AddComponent(new BoxCollider(-2, -2, 5, 5));
			
			// Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			// Flags.SetFlagExclusive(ref collider.PhysicsLayer, 1);


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
		
		/// <summary>
		/// creates a projectile and sets it in motion
		/// </summary>
		public Entity CreateBouncingProjectiles(Vector2 position, float mass, Vector2 velocity)
		{
			var friction = 0.3f;
			var elasticity = 0.4f;
			
			var rigidbody = new BouncingBullet()
				.SetMass(mass)
				.SetFriction(friction)
				.SetElasticity(elasticity)
				.SetVelocity(velocity);
			
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("projectile");
			entity.Position = position;
			entity.AddComponent(rigidbody);
			entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			entity.AddComponent(new BouncingBulletProjectileController(velocity));

			// add a collider so we can detect intersections
			var collider = entity.AddComponent(new BoxCollider(-2, -2, 5, 5));
			
			
			
			// Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			// Flags.SetFlagExclusive(ref collider.PhysicsLayer, 1);

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
		
		public Entity ReleaseItem(Vector2 position, float mass, float friction, float elasticity, Texture2D texture)
		{
			var rigidbody = new Crown()
				.SetMass(mass)
				.SetFriction(friction)
				.SetElasticity(elasticity);
			
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("boss item");
			entity.Position = position;
			entity.AddComponent(rigidbody);
			// entity.AddComponent(new ProjectileMover());
			// entity.AddComponent(new BouncingBulletProjectileController(new Vector2(0, 200)));

			// add a collider so we can detect intersections
			var collider = entity.AddComponent<CircleCollider>();
			Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			// Flags.SetFlagExclusive(ref collider.PhysicsLayer, 1);

			// load up a Texture that contains a fireball animation and setup the animation frames
			var sprites = Sprite.SpritesFromAtlas(texture, 8, 8);

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

		public Entity CreateNewPlayer(string name, Vector2 position)
		{
			
			var playerEntity = CreateEntity("player_" + name, new Vector2(position.X, position.Y));
			playerEntity.AddComponent(new OtherPlayer(name));
			var collider = playerEntity.AddComponent(new BoxCollider(-8, -16, 16, 32));
			playerEntity.AddComponent(
				new TiledMapMover(Entities.FindEntity("tiled-map-entity")
					.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			playerEntity.AddComponent(new BulletHitDetector());
			AddHealthBarToEntity(playerEntity);
			// Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			// Flags.SetFlagExclusive(ref collider.PhysicsLayer, 0);
			
			return playerEntity;
		}

		/// <summary>
		/// Add a health bar above the current entity.
		/// The health bar entity name is parentEntity.Name + "HealthBar"
		/// </summary>
		/// <param name="parentEntity">the entity to add the health bar to</param>
		public void AddHealthBarToEntity(Entity parentEntity)
		{
			// Add health bar
			var playerHealthEntity = CreateEntity( parentEntity.Name + "HealthBar", new Vector2( 0, - 20)); /* this is relatively to the parent */
			playerHealthEntity.SetParent(parentEntity);
			var playerHealthComponent = new HealthBar();
			playerHealthEntity.AddComponent(playerHealthComponent);

		}
		
		/// <summary>
		/// Method for the network to call once it need to update the other players (not the current client)
		/// </summary>
		/// <param name="indexInList">the index of the other client in the players list</param>
		/// <param name="newVelocity">the new velocity the server dictates</param>
		public void UpdateOtherPlayerMovement(string name, Vector2 newPos, Vector2 newVelocity, bool fireInputPressed, int health)
		{
			var p = Entities.FindEntity("player_" + name);
			// if (p == null)
			// {
			// 	System.Console.WriteLine("p is null");
			// } else
			// {
			// 	System.Console.WriteLine("Updating other movement: " + p.GetComponent<OtherPlayer>().name);
			// }
			
			p.Transform.Position = newPos;
			p.GetComponent<OtherPlayer>()._velocity = newVelocity;
			p.GetComponent<OtherPlayer>()._fireInputIsPressed = fireInputPressed;
			// p.GetComponent<BulletHitDetector>().currentHP = health;
			p.Update();

		}
	}
}