using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez.Samples.Scenes.CharacterSelection;
using Nez.Tiled;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;

namespace Nez.Samples
{
	[SampleScene("Platformer", 120, "Work in progress...\nArrows, d-pad or left stick to move, z key or a button to jump")]
	public class PlatformerScene : SampleScene
	{
		private static TmxObject SpawnObject;
		private static TmxMap Map;
		private ProjectileHandler projectiles;
		public PlatformerScene() //: base(true, true)
		{}


		public override void Initialize()
		{
			// setup a pixel perfect screen that fits our map
			SetDesignResolution(1200, 650, SceneResolutionPolicy.ShowAllPixelPerfect);
			Screen.SetSize(1200, 650);

			// Create background - temporary until we have background graphics
			ClearColor = Color.LightSlateGray;

			// load up our TiledMap
			var map = Content.LoadTiledMap("Content/Platformer/" + MapSelectionScene.chosenMap +".tmx");
			Map = map;
			// var map = Content.LoadTiledMap("Content/Platformer/"+.tmx");
			SpawnObject = map.GetObjectGroup("objects").Objects["spawn"];
			var tiledEntity = CreateEntity("tiled-map-entity");
			tiledEntity.AddComponent(new TiledMapRenderer(map, "main"));
			
			projectiles = new ProjectileHandler(Content);

			// create our Player and add a TiledMapMover to handle collisions with the tilemap
			var playerInitialSpawnPos = map.GetObjectGroup("objects").Objects["spawnPlayer" + LoginScene.playerIndex];
			var playerEntity = CreateEntity("player", new Vector2(playerInitialSpawnPos.X, playerInitialSpawnPos.Y));
			var playerComponent = new Caveman(LoginScene._playerName);
			playerEntity.AddComponent(playerComponent);
			playerEntity.AddComponent(new BoxCollider(-12, -32, 16, 64));
			playerEntity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("main")));
			playerEntity.AddComponent(new BulletHitDetector());
			// AddHealthBarToEntity(playerEntity);
			
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
			var itemTexture = Content.Load<Texture2D>("Platformer/crown");
			var itemSpawn0 = map.GetObjectGroup("objects").Objects["spawnCrown0"];
			ReleaseItem(0, new Vector2(itemSpawn0.X, itemSpawn0.Y), itemTexture, 1f, 0, 0);
			var itemSpawn1 = map.GetObjectGroup("objects").Objects["spawnCrown1"];
			ReleaseItem(1, new Vector2(itemSpawn1.X - 10, itemSpawn1.Y - 10), itemTexture, 1f, 0, 0);
			var itemSpawn2 = map.GetObjectGroup("objects").Objects["spawnCrown2"];
			ReleaseItem(2, new Vector2(itemSpawn2.X - 10, itemSpawn2.Y), itemTexture, 1f, 0, 0);
			var itemSpawn3 = map.GetObjectGroup("objects").Objects["spawnCrown3"];
			ReleaseItem(3, new Vector2(itemSpawn3.X, itemSpawn3.Y + 10), itemTexture, 1f, 0, 0);

			foreach (var player in OtherPlayer.players.Where(p => !p.name.Equals(LoginScene._playerName)))
			{
				CreateNewPlayer(player.name, player.playerIndex, player.playerSprite);
			}
		}
		
		/// <summary>
		/// creates a projectile and sets it in motion
		/// </summary>
		public Entity CreateProjectiles(string name, int type, Vector2 position, Vector2 dir)
		{
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("projectile");
			entity.Position = position;
			entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));

			List<Sprite> sprites;
			Vector2 velocity = new Vector2(400);
			if (type == 2)
			{
				entity.AddComponent(new BouncingBulletProjectileController(dir * velocity));
				entity.AddComponent(new BoxCollider(-8, -6, 16, 12));
				sprites = Sprite.SpritesFromAtlas(projectiles.Pebble, 32, 32);
				
				var friction = 0.3f;
				var elasticity = 0.4f;
				var rigidbody = new BouncingBullet()
					.SetName(name)
					.SetMass(1f)
					.SetFriction(friction)
					.SetElasticity(elasticity)
					.SetVelocity(dir * velocity);
				
				entity.AddComponent(rigidbody);
			}
			else if (type == 11)
			{
				entity.AddComponent(new BulletProjectileController(name, dir * 500, 11));
				entity.AddComponent(new BoxCollider(-12, -5, 30, 12));
				sprites = Sprite.SpritesFromAtlas(projectiles.Stream, 32, 32);
			}
			else if (type == 12)
			{
				entity.AddComponent(new BulletProjectileController(name, dir * velocity, 12));
				entity.AddComponent(new BoxCollider(-5, -5, 12, 10));
				sprites = Sprite.SpritesFromAtlas(projectiles.Seed, 32, 32);
			}
			else if (type == 22)
			{
				entity.AddComponent(new BouncingBulletProjectileController(dir * velocity));
				entity.AddComponent(new BoxCollider(-15, -18, 32, 32));
				sprites = Sprite.SpritesFromAtlas(projectiles.Boulder, 64, 64);
				
				var friction = 0.3f;
				var elasticity = 0.4f;
				var rigidbody = new BouncingBullet()
					.SetName(name)
					.SetMass(5f)
					.SetFriction(friction)
					.SetElasticity(elasticity)
					.SetVelocity(dir * velocity);
				
				entity.AddComponent(rigidbody);
			}
			else
			{
				entity.AddComponent(new BulletProjectileController(name, dir * velocity, 0));
				entity.AddComponent(new BoxCollider(-2, -2, 5, 5));
				sprites = Sprite.SpritesFromAtlas(Content.Load<Texture2D>(Nez.Content.NinjaAdventure.Plume), 64, 64);
			}
			
			// add the Sprite to the Entity and play the animation after creating it
			var animator = entity.AddComponent(new SpriteAnimator());
			
			// render after (under) our player who is on renderLayer 0, the default
			animator.RenderLayer = 1;

			animator.AddAnimation("default", sprites.ToArray());
			animator.Play("default");

			return entity;
		}
		
		public Entity CreateShield(Entity parent, string name)
		{
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("shield");
			entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			
			entity.AddComponent(new Shield(name));
			entity.AddComponent(new BoxCollider(-32, -32, 64, 64));
			entity.SetParent(parent);
			var sprites = Sprite.SpritesFromAtlas(projectiles.Shield, 64, 64);
			
			// add the Sprite to the Entity and play the animation after creating it
			var animator = entity.AddComponent(new SpriteAnimator());

			animator.AddAnimation("default", sprites.ToArray());
			animator.Play("default");

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

			// load up a Texture that contains a fireball animation and setup the animation frames
			var texture = Content.Load<Texture2D>(Nez.Content.NinjaAdventure.Plume);
			var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);

			// add the Sprite to the Entity and play the animation after creating it
			var animator = entity.AddComponent(new SpriteAnimator());

			// render after (under) our player who is on renderLayer 0, the default
			animator.RenderLayer = 1;

			animator.AddAnimation("default", sprites.ToArray());
			animator.Play("default");


			return entity;
		}
		
		public Entity ReleaseItem(int num, Vector2 position, Texture2D texture, float mass, float friction, float elasticity)
		{
			var item = new Item(num)
				.SetMass(mass)
				.SetFriction(friction)
				.SetElasticity(elasticity)
				.SetTexture(texture);
			
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("item");
			entity.Position = position;
			entity.AddComponent(item);
			entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			// entity.AddComponent(new ProjectileMover());
			// entity.AddComponent(new BouncingBulletProjectileController(new Vector2(0, 200)));

			// add a collider so we can detect intersections
			var collider = entity.AddComponent(new BoxCollider(-8, -8, 16, 16));
			// var collider = entity.AddComponent(new CircleCollider(8));
			Flags.SetFlagExclusive(ref collider.CollidesWithLayers, 0);
			// Flags.SetFlagExclusive(ref collider.PhysicsLayer, 1);

			// load up a Texture that contains a fireball animation and setup the animation frames
			var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);

			// add the Sprite to the Entity and play the animation after creating it
			var animator = entity.AddComponent(new SpriteAnimator());
			
			// render after (under) our player who is on renderLayer 0, the default
			animator.RenderLayer = 1;
			
			animator.AddAnimation("default", sprites.ToArray());
			animator.Play("default");


			return entity;
		}

		public Entity CreateNewPlayer(string name, int playerIndex, string spriteType)
		{
			var position = Map.GetObjectGroup("objects").Objects["spawnPlayer" + playerIndex];
			
			var playerEntity = CreateEntity("player_" + name, new Vector2(position.X, position.Y));
			playerEntity.AddComponent(new OtherPlayer(name, playerIndex, spriteType));
			
			playerEntity.AddComponent(new BoxCollider(-12, -32, 16, 64));
			playerEntity.AddComponent(new TiledMapMover(Map.GetLayer<TmxLayer>("main")));
			
			playerEntity.AddComponent(new BulletHitDetector());
			// AddHealthBarToEntity(playerEntity);
			
			return playerEntity;
		}

		public Entity Respawn(Entity player, string bulletOwner)
		{
			// player.GetComponent<BulletHitDetector>().currentHP = player.GetComponent<BulletHitDetector>().maxHP;
			// Component playerComponent = null;
			// if (player.GetComponent<Caveman>() != null)
			// {
			// 	playerComponent = new Caveman(player.GetComponent<Caveman>().name);
			// 	player.RemoveComponent<Caveman>();
			// }
			// else if (player.GetComponent<OtherPlayer>() != null)
			// {
			// 	playerComponent = new Caveman(player.GetComponent<OtherPlayer>().name);
			// 	player.RemoveComponent<OtherPlayer>();
			// }
			// var playerEntity = player.WeaveClone(new Vector2(SpawnObject.X, SpawnObject.Y));
			player.Destroy();
			// if (playerComponent != null)
			// {
			// 	playerEntity.AddComponent(playerComponent);
			// }
			// AddEntity(playerEntity);
			// Entity playerEntity = null;
			// Component playerComponent = null;
			// if (player.GetComponent<Caveman>() != null)
			// {
			// 	playerEntity = CreateEntity("player", new Vector2(SpawnObject.X, SpawnObject.Y));
			// 	playerComponent = new Caveman(player.GetComponent<Caveman>().name);
			// 	playerEntity.AddComponent(playerComponent);
			// 	playerEntity.AddComponent(new BoxCollider(-12, -32, 16, 64));
			// 	playerEntity.AddComponent(new TiledMapMover(Map.GetLayer<TmxLayer>("main")));
			// 	playerEntity.AddComponent(new BulletHitDetector());
			// 		//TODO: SHOULDN"T WE UPDATE THEIR ITEM BUFFER TO BE THE OLD ITEM BUFFER TOO?
			// }
			// else if (player.GetComponent<OtherPlayer>() != null)
			// {
			// 	var old = player.GetComponent<OtherPlayer>();
			// 	playerEntity = CreateEntity("player_" + old.name, new Vector2(SpawnObject.X, SpawnObject.Y));
   //              playerComponent = new OtherPlayer(old.name, old.playerIndex, old.spriteType);
   //              playerEntity.AddComponent(playerComponent);
   //              playerEntity.AddComponent(new BoxCollider(-12, -32, 16, 64));
   //              playerEntity.AddComponent(new TiledMapMover(Map.GetLayer<TmxLayer>("main")));
   //              playerEntity.AddComponent(new BulletHitDetector());
			// }
			//
			// player.Destroy();
			
			// Entity playerEntity = null;
			// Component playerComponent = null;
			player.Transform.Position = new Vector2(SpawnObject.X, SpawnObject.Y);
			player.GetComponent<BulletHitDetector>().currentHP = 1;
			// if (player.GetComponent<Caveman>() != null)
			// {
			// 	player.Transform.Position = new Vector2(SpawnObject.X, SpawnObject.Y);
			// 	player.GetComponent<BulletHitDetector>().currentHP = 1;
			// 	//TODO: SHOULDN"T WE UPDATE THEIR ITEM BUFFER TO BE THE OLD ITEM BUFFER TOO?
			// }
			// else if (player.GetComponent<OtherPlayer>() != null)
			// {
			// 	var old = player.GetComponent<OtherPlayer>();
			// 	playerEntity = CreateEntity("player_" + old.name, new Vector2(SpawnObject.X, SpawnObject.Y));
			// 	playerComponent = new OtherPlayer(old.name, old.playerIndex, old.spriteType);
			// 	playerEntity.AddComponent(playerComponent);
			// 	playerEntity.AddComponent(new BoxCollider(-12, -32, 16, 64));
			// 	playerEntity.AddComponent(new TiledMapMover(Map.GetLayer<TmxLayer>("main")));
			// 	playerEntity.AddComponent(new BulletHitDetector());
			// }
			//
			// player.Destroy();
			
			// AddHealthBarToEntity(playerEntity);

			// return playerEntity;
			return player;
		}

		/// <summary>
		/// Add a health bar above the current entity.
		/// The health bar entity name is parentEntity.Name + "HealthBar"
		/// </summary>
		/// <param name="parentEntity">the entity to add the health bar to</param>
		public void AddHealthBarToEntity(Entity parentEntity)
		{
			// Add health bar
			var playerHealthEntity = CreateEntity( parentEntity.Name + "HealthBar", new Vector2( 1000, 200)); /* this is relatively to the parent */
			// playerHealthEntity.SetParent(parentEntity);
			// playerHealthEntity.SetPosition(new Vector2(1000, 200));
			var playerHealthComponent = new HealthBar();
			playerHealthEntity.AddComponent(playerHealthComponent);
			parentEntity.AddComponent(playerHealthComponent);

		}
		
		/// <summary>
		/// Method for the network to call once it need to update the other players (not the current client)
		/// </summary>
		/// <param name="indexInList">the index of the other client in the players list</param>
		/// <param name="newVelocity">the new velocity the server dictates</param>
		public void UpdateOtherPlayerMovement(string name, Vector2 newPos, Vector2 newVelocity, bool fireInputPressed, int projType, Vector2 projDir,int health)
		{
			var p = Entities.FindEntity("player_" + name);
			
			p.Transform.Position = newPos;
			var playerComponent = p.GetComponent<OtherPlayer>();
			playerComponent._velocity = newVelocity;
			playerComponent._fireInputIsPressed = fireInputPressed;
			playerComponent._projDir = projDir;
			playerComponent.projectileType = projType;
			// p.GetComponent<BulletHitDetector>().currentHP = health;
			p.Update();

		}
		/// <summary>
		/// Method for the network to call once it need to update the current client health
		/// </summary>
		/// <param name="health"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void UpdatePlayerHealth(int health)
		{
			var p = Entities.FindEntity("player");

			if (p.GetComponent<BulletHitDetector>().currentHP > health)
				p.GetComponent<BulletHitDetector>().currentHP = health;
			// if (p.GetComponent<BulletHitDetector>().currentHP <= 0)
			// 	p.Destroy();
		}
		
		/*
		 * Deprecated chunks of code go here for safe keeping
		 *
		 * 
		 */
	}
}