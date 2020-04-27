using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Nez.Samples.Scenes.CharacterSelection;
using Nez.Spatial;
using Nez.Tiled;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;

namespace Nez.Samples
{
	[SampleScene("Platformer", 120, "Work in progress...\nArrows, d-pad or left stick to move, z key or a button to jump")]
	public class PlatformerScene : SampleScene
	{
		private static List<TmxObject> SpawnObject = new List<TmxObject>();
		private static TmxMap Map;
		public static KillCountComponent playerKillComponent;
		public static ManaComponent playerMana;
		private ProjectileHandler projectiles;
		public List<SoundEffect> soundEffects = new List<SoundEffect>();
		public PlatformerScene() //: base(true, true)
		{}
		
		private int screen_width = 1200;
		private int screen_height = 650;

		public static int networkID = 0;
		private Song song;

		public override void Initialize()
		{
			// setup a pixel perfect screen that fits our map
			SetDesignResolution(screen_width, screen_height, SceneResolutionPolicy.ShowAllPixelPerfect);
			
			Screen.SetSize(screen_width, screen_height);

			// Create background - temporary until we have background graphics
			ClearColor = Color.LightSlateGray;

			// load up our TiledMap
			var map = Content.LoadTiledMap("Content/Platformer/" + MapSelectionScene.chosenMap +".tmx");
			Map = map;
			
			song = Content.Load<Song>("Platformer/battkeBG");
			MediaPlayer.Play(song);
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = LoginScene.MasterVolume ;

			//sound effects
			soundEffects.Add(Content.Load<SoundEffect>("Platformer/die"));
			soundEffects.Add(Content.Load<SoundEffect>("Platformer/oops"));
			soundEffects.Add(Content.Load<SoundEffect>("Platformer/shoot"));
			soundEffects.Add(Content.Load<SoundEffect>("Platformer/shield_form"));
			soundEffects.Add(Content.Load<SoundEffect>("Platformer/shield_pop"));
			soundEffects.Add(Content.Load<SoundEffect>("Platformer/wind sound"));

			//Clear existing spawn points
			SpawnObject.Clear();
			// var map = Content.LoadTiledMap("Content/Platformer/"+.tmx");
			foreach (var item in map.GetObjectGroup("objects").Objects)
			{
				if (item.Name.Equals("spawn"))
				{
					SpawnObject.Add(item);
				}
			}
			// SpawnObject = map.GetObjectGroup("objects").Objects["spawn"];
			var tiledEntity = CreateEntity("tiled-map-entity");
			tiledEntity.AddComponent(new TiledMapRenderer(map, "main")).RenderLayer = Int32.MaxValue - 32;
			
			projectiles = new ProjectileHandler(Content);

			// create our Player and add a TiledMapMover to handle collisions with the tilemap
			var playerInitialSpawnPos = map.GetObjectGroup("objects").Objects["spawnPlayer" + LoginScene.playerIndex];
			var playerEntity = CreateEntity("player", new Vector2(playerInitialSpawnPos.X, playerInitialSpawnPos.Y));
			var playerComponent = new Caveman(LoginScene._playerName);
			playerEntity.AddComponent(playerComponent);
			playerEntity.AddComponent(new BoxCollider(-12, -32, 16, 64));
			playerEntity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("main")));
			playerEntity.AddComponent(new BulletHitDetector());
			
			var playerNameTagEntity = CreateEntity( playerEntity.Name + "NameTag"); /* this is relatively to the parent */
			
			var tagName = playerNameTagEntity.AddComponent(new TextComponent());
			tagName.Text = playerEntity.GetComponent<Caveman>().name;
			playerNameTagEntity.SetScale(2);
			
			playerNameTagEntity.SetParent(playerEntity);
			playerNameTagEntity.SetLocalPosition(new Vector2(-16, -40));
			
			var playerKillCountEntity =
				CreateKillCountEntity(LoginScene._playerName, new Vector2(Screen.Width / 5, 30), CharacterSelectionScene.chosenSprite);
			
			var playerManaCountEntity =
				CreateManaCountEntity(LoginScene._playerName, new Vector2(Screen.Width / 5, 50));
			playerKillComponent = playerKillCountEntity.GetComponent<KillCountComponent>();
			playerMana = playerManaCountEntity.GetComponent<ManaComponent>();
			
			List<Entity> killCountEntityList = new List<Entity>();
			killCountEntityList.Add(playerKillCountEntity);
			
			// var itemTexture = Content.Load<Texture2D>("Platformer/crown");
			// var itemSpawn0 = map.GetObjectGroup("objects").Objects["spawnCrown0"];
			// ReleaseItem(0, new Vector2(itemSpawn0.X, itemSpawn0.Y), itemTexture, 1f, 0, 0);
			// var itemSpawn1 = map.GetObjectGroup("objects").Objects["spawnCrown1"];
			// ReleaseItem(1, new Vector2(itemSpawn1.X - 10, itemSpawn1.Y - 10), itemTexture, 1f, 0, 0);
			// var itemSpawn2 = map.GetObjectGroup("objects").Objects["spawnCrown2"];
			// ReleaseItem(2, new Vector2(itemSpawn2.X - 10, itemSpawn2.Y), itemTexture, 1f, 0, 0);
			// var itemSpawn3 = map.GetObjectGroup("objects").Objects["spawnCrown3"];
			// ReleaseItem(3, new Vector2(itemSpawn3.X, itemSpawn3.Y + 10), itemTexture, 1f, 0, 0);

			var i = 2;
			foreach (var player in OtherPlayer.players.Where(p => !p.name.Equals(LoginScene._playerName)))
			{
				CreateNewPlayer(player.name, player.playerIndex, player.playerSprite);
				var temp = CreateKillCountEntity(player.name, new Vector2(Screen.Width/5 * i , 30 ), player.playerSprite);
				
				// var temp2 = CreateManaCountEntity(player.name, new Vector2(Screen.Width / 5 * i, 50));
				killCountEntityList.Add(temp);
				i++;
			}
			
			// Only set up moving camera if level size requires it.
			if (map.Height > 21 || map.Width > 38)
			{
				var topLeft = new Vector2(0, 0);
				var bottomRight = new Vector2(map.TileWidth * (map.Width - 1),
					map.TileWidth * (map.Height - 1));
				tiledEntity.AddComponent(new WeaveCameraBounds(topLeft, bottomRight));
				Camera.Entity.AddComponent(new FollowCamera(playerEntity));
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

			List<Sprite> sprites = null;
			Vector2 velocity = new Vector2(400);
			if (type == 2)
			{
				soundEffects[2].CreateInstance().Play();
				entity.AddComponent(new BouncingBulletProjectileController(dir * velocity));
				entity.AddComponent(new BoxCollider(-8, -6, 16, 12));
				sprites = Sprite.SpritesFromAtlas(projectiles.Pebble, 32, 32);
				
				var friction = 0.3f;
				var elasticity = 0.4f;
				var rigidbody = new BouncingBullet(2)
					.SetName(name)
					.SetMass(1f)
					.SetFriction(friction)
					.SetElasticity(elasticity)
					.SetVelocity(dir * 600);
				
				entity.AddComponent(rigidbody);
			}
			else if (type == 3)
			{
				soundEffects[2].CreateInstance().Play();
				entity.AddComponent(new BouncingBulletProjectileController(dir * 500));
				entity.AddComponent(new BoxCollider(-16, -12, 30, 30));
				sprites = Sprite.SpritesFromAtlas(projectiles.Wind, 32, 32);
				
				var friction = 0.3f;
				var elasticity = 0.4f;
				var rigidbody = new BouncingBullet(3)
					.SetName(name)
					.SetMass(1f)
					.SetFriction(friction)
					.SetElasticity(elasticity)
					.SetVelocity(dir * 500);
				
				entity.AddComponent(rigidbody);
			}
			else if (type == 11)
			{
				soundEffects[2].CreateInstance().Play();
				entity.AddComponent(new BulletProjectileController(name, dir * 500, 11));
				entity.AddComponent(new BoxCollider(-12, -5, 30, 12));
				sprites = Sprite.SpritesFromAtlas(projectiles.Stream, 32, 32);
			}
			else if (type == 12)
			{
				soundEffects[2].CreateInstance().Play();
				entity.AddComponent(new BulletProjectileController(name, dir * velocity, 12));
				entity.AddComponent(new BoxCollider(-5, -5, 12, 10));
				sprites = Sprite.SpritesFromAtlas(projectiles.Seed, 32, 32);
			}
			else if (type == 22)
			{
				soundEffects[2].CreateInstance().Play();
				entity.AddComponent(new BouncingBulletProjectileController(dir * velocity));
				entity.AddComponent(new BoxCollider(-15, -18, 32, 32));
				sprites = Sprite.SpritesFromAtlas(projectiles.Boulder, 64, 64);
				
				var friction = 0.3f;
				var elasticity = 0.4f;
				var rigidbody = new BouncingBullet(22)
					.SetName(name)
					.SetMass(5f)
					.SetFriction(friction)
					.SetElasticity(elasticity)
					.SetVelocity(dir * 250);
				
				entity.AddComponent(rigidbody);
			}
			else if (type == 23)
			{
				soundEffects[2].CreateInstance().Play();
				for (int i = 0; i <= 5; i++)
				{
					CreateRandomPebble(name, position);
				}
			}
			else if (type == 33)
			{
				var windEffect = soundEffects[2].CreateInstance();
				windEffect.Volume = (float) 0.3;
				windEffect.Play();
				IEnumerable<Collider> colliders = Physics.GetAllColliders();
				foreach (var collide in colliders)
				{
					var player = collide.Entity.GetComponent<Caveman>();
					if (player != null)
					{
						if (player.name.Equals(name))
						{
						}
						else
						{
							player.push = 300;
							player.pushSoundPlayed = false;
						}
						
					}

				}
			}
			else
			{
				soundEffects[1].CreateInstance().Play();
				// entity.AddComponent(new BulletProjectileController(name, dir * velocity, 0));
				// entity.AddComponent(new BoxCollider(-2, -2, 5, 5));
				// sprites = Sprite.SpritesFromAtlas(Content.Load<Texture2D>(Nez.Content.NinjaAdventure.Plume), 64, 64);
			}
			
			// add the Sprite to the Entity and play the animation after creating it
			var animator = entity.AddComponent(new SpriteAnimator());
			
			// render after (under) our player who is on renderLayer 0, the default
			animator.RenderLayer = 1;

			if (sprites != null)
			{
				animator.AddAnimation("default", sprites.ToArray());
				animator.Play("default");
			}
			
			System.Console.WriteLine("name of projectile: " + entity.Name);

			return entity;
		}

		public Entity CreateRandomPebble(string name, Vector2 position)
		{
			var entity = CreateEntity("projectile");
			entity.Position = position;
			entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			
			Vector2 velocity = new Vector2(400);

			var dir = Vector2.Zero;
			dir.X = Random.Range(-1f, 1f);
			dir.Y = Random.Range(-1f, 1f);
			
			entity.AddComponent(new BouncingBulletProjectileController(dir * velocity));
			entity.AddComponent(new BoxCollider(-16, -16, 32, 32));
			List<Sprite> sprites = Sprite.SpritesFromAtlas(projectiles.WindPebble, 64, 64);

			var friction = 0.3f;
			var elasticity = 0.4f;
			var rigidbody = new BouncingBullet(23)
				.SetName(name)
				.SetMass(1f)
				.SetFriction(friction)
				.SetElasticity(elasticity)
				.SetVelocity(dir * 400);
				
			entity.AddComponent(rigidbody);
			
			var animator = entity.AddComponent(new SpriteAnimator());
			
			// render after (under) our player who is on renderLayer 0, the default
			animator.RenderLayer = 1;

			if (sprites != null)
			{
				animator.AddAnimation("default", sprites.ToArray());
				animator.Play("default");
			}

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
			
			soundEffects[3].CreateInstance().Play();

			return entity;
		}
		
		public Entity CreateVine(Vector2 pos)
		{
			// create an Entity to house the projectile and its logic
			var entity = CreateEntity("vine");
			entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			
			entity.AddComponent(new Vine());
			entity.SetPosition(new Vector2(pos.X, pos.Y - 45));
			entity.AddComponent(new BoxCollider(-16, -48, 32, 96));
			var sprites = Sprite.SpritesFromAtlas(projectiles.Vine, 32, 96);
			
			// add the Sprite to the Entity and play the animation after creating it
			var animator = entity.AddComponent(new SpriteAnimator());
			
			animator.RenderLayer = 2;

			animator.AddAnimation("default", sprites.ToArray());
			animator.Play("default");

			return entity;
		}
		
		public Entity CreateCyclone(Entity parent, string name)
		{
			// create an Entity to house the projectile and its logic
			var entity1 = CreateEntity("cyclone");
			entity1.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			
			entity1.AddComponent(new Cyclone(name));
			entity1.SetParent(parent);
			entity1.SetPosition(new Vector2(50, 0));
			entity1.AddComponent(new BoxCollider(-80, -32, 96, 64));
			var sprites1 = Sprite.SpritesFromAtlas(projectiles.Cyclone, 32, 64);
			
			var entity2 = CreateEntity("cyclone");
			entity2.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
				.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
			
			entity2.AddComponent(new Cyclone(name));
			entity2.SetParent(parent);
			entity2.SetPosition(new Vector2(-50, 0));
			entity2.AddComponent(new BoxCollider(-10, -32, 96, 64));
			var sprites2 = Sprite.SpritesFromAtlas(projectiles.Cyclone, 32, 64);
			
			// add the Sprite to the Entity and play the animation after creating it
			var animator1 = entity1.AddComponent(new SpriteAnimator());
			var animator2 = entity2.AddComponent(new SpriteAnimator());

			animator1.AddAnimation("default", sprites1.ToArray());
			animator2.AddAnimation("default", sprites2.ToArray());
			animator1.Play("default");
			animator2.Play("default");

			return entity1;
		}
		
		/// <summary>
		/// creates a projectile and sets it in motion
		/// </summary>
		// public Entity CreateBouncingProjectiles(Vector2 position, float mass, Vector2 velocity)
		// {
		// 	var friction = 0.3f;
		// 	var elasticity = 0.4f;
		// 	
		// 	var rigidbody = new BouncingBullet()
		// 		.SetMass(mass)
		// 		.SetFriction(friction)
		// 		.SetElasticity(elasticity)
		// 		.SetVelocity(velocity);
		// 	
		// 	// create an Entity to house the projectile and its logic
		// 	var entity = CreateEntity("projectile");
		// 	entity.Position = position;
		// 	entity.AddComponent(rigidbody);
		// 	entity.AddComponent(new TiledMapMover(Entities.FindEntity("tiled-map-entity")
		// 		.GetComponent<TiledMapRenderer>().TiledMap.GetLayer<TmxLayer>("main")));
		// 	entity.AddComponent(new BouncingBulletProjectileController(velocity));
		//
		// 	// add a collider so we can detect intersections
		// 	var collider = entity.AddComponent(new BoxCollider(-2, -2, 5, 5));
		//
		// 	// load up a Texture that contains a fireball animation and setup the animation frames
		// 	var texture = Content.Load<Texture2D>(Nez.Content.NinjaAdventure.Plume);
		// 	var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);
		//
		// 	// add the Sprite to the Entity and play the animation after creating it
		// 	var animator = entity.AddComponent(new SpriteAnimator());
		//
		// 	// render after (under) our player who is on renderLayer 0, the default
		// 	animator.RenderLayer = 1;
		//
		// 	animator.AddAnimation("default", sprites.ToArray());
		// 	animator.Play("default");
		//
		//
		// 	return entity;
		// }
		
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
			AddNameTagOnCharacter(playerEntity);
			// AddHealthBarToEntity(playerEntity);
			
			var mainPlayer = Entities.FindEntity("player");
			var mainPlayerPos = mainPlayer.Transform.Position;
			var mainPlayerLocalPos = mainPlayer.Transform.Position;
			
			
			var trackerTexture = Content.Load<Texture2D>("Platformer/Tracker/tracker" + spriteType.Last());
			var trackerEntity = CreateEntity("tracker" + name, Tracker.CalculateTrackerPosition(mainPlayerPos, mainPlayerLocalPos, new Vector2(position.X, position.Y)));
			trackerEntity.AddComponent(new SpriteRenderer(trackerTexture));
			trackerEntity.Transform.Rotation = Tracker.CalculateTrackerAngle(mainPlayerPos, new Vector2(position.X, position.Y));
			
			return playerEntity;
		}

		public Entity Respawn(Entity player, string bulletOwner)
		{
			soundEffects[0].CreateInstance().Play();
			int spawn = Random.NextInt(SpawnObject.Count);
			System.Console.WriteLine(spawn);
			System.Console.WriteLine(SpawnObject[spawn].X);
			System.Console.WriteLine(SpawnObject[spawn].Y);
			player.Transform.Position = new Vector2(SpawnObject[spawn].X, SpawnObject[spawn].Y);
			player.GetComponent<BulletHitDetector>().currentHP = 1;
			if (player.Name.Equals("player") && (!bulletOwner.Equals(LoginScene._playerName))) //the other player needed to respawn
			{
				Network.outmsg = Network.Client.CreateMessage();
				Network.outmsg.Write("kill");
				Network.outmsg.Write(bulletOwner);
				Network.outmsg.Write(-1);
				Network.Client.SendMessage(Network.outmsg, NetDeliveryMethod.ReliableOrdered);
				// networkID++;

				// playerKillComponent.kills++;
				// playerKillComponent.Entity.GetComponent<TextComponent>().Text = playerKillComponent.playerName +"'s Kill: " + playerKillComponent.kills;
			}
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
		
		public void AddNameTagOnCharacter(Entity parentEntity)
		{
			// Add health bar
			var playerHealthEntity = CreateEntity( parentEntity.Name + "NameTag"); /* this is relatively to the parent */
	
			var tagName = playerHealthEntity.AddComponent(new TextComponent());
			tagName.Text = parentEntity.GetComponent<OtherPlayer>().name;
			playerHealthEntity.SetScale(2);
			
			playerHealthEntity.SetParent(parentEntity);
			playerHealthEntity.SetLocalPosition(new Vector2(-16, -40));
		}
		
		/// <summary>
		/// Method for the network to call once it need to update the other players (not the current client)
		/// </summary>
		/// <param name="indexInList">the index of the other client in the players list</param>
		/// <param name="newVelocity">the new velocity the server dictates</param>
		public void UpdateOtherPlayerMovement(string name, Vector2 newPos, Vector2 newVelocity, bool fireInputPressed, int projType, Vector2 projDir,int killCount, int mana)
		{
			var p = Entities.FindEntity("player_" + name);
			
			p.Transform.Position = newPos;
			var playerComponent = p.GetComponent<OtherPlayer>();
			playerComponent._velocity = newVelocity;
			playerComponent._fireInputIsPressed = fireInputPressed;
			playerComponent._projDir = projDir;
			playerComponent.projectileType = projType;
			// p.GetComponent<BulletHitDetector>().currentHP = health;
			var oplayerKillComponent = Entities.FindEntity("killCount_" + name).GetComponent<KillCountComponent>();
			oplayerKillComponent.kills = killCount;
			oplayerKillComponent.GetComponent<TextComponent>().Text = oplayerKillComponent.playerName +"'s Kill: " + oplayerKillComponent.kills;
			
			var playerManaComponent = Entities.FindEntity("manaCount_" + name).GetComponent<ManaComponent>();
			playerManaComponent.mana = mana;
			playerManaComponent.GetComponent<TextComponent>().Text = playerManaComponent.playerName +"'s Mana: " + playerManaComponent.mana;
			
			p.Update();
			
			
			
			var mainPlayer = Entities.FindEntity("player");
			var mainPlayerPos = mainPlayer.Transform.Position;
			var mainPlayerLocalPos = Camera.WorldToScreenPoint(mainPlayerPos);
			
			var tracker = Entities.FindEntity("tracker" + name);
			tracker.Transform.Position = Tracker.CalculateTrackerPosition(mainPlayerPos,  mainPlayerLocalPos, newPos);
			tracker.Transform.Rotation = Tracker.CalculateTrackerAngle(mainPlayerPos, newPos);

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

		public Entity CreateKillCountEntity(string playerName, Vector2 pos, string playerSpriteType)
		{
			var thisPlayerKillEntity = CreateEntity("killCount_" + playerName, pos);
			var thisPlayerKillComponent = new KillCountComponent(playerName);
			thisPlayerKillEntity.AddComponent(thisPlayerKillComponent);
			thisPlayerKillEntity.SetScale(2);
			
			var nameText = thisPlayerKillEntity.AddComponent(new TextComponent());
			nameText.Text = thisPlayerKillComponent.playerName +"'s Kill: " + thisPlayerKillComponent.kills;
			switch (playerSpriteType)
			{
				case "player0":
					nameText.Color = Color.Yellow;
					break;
				case "player1":
					nameText.Color = Color.Blue;
					break;
				case "player2":
					nameText.Color = Color.GreenYellow;
					break;
				case "player3":
					nameText.Color = Color.Pink;
					break;
			}
			nameText.SetVerticalAlign(VerticalAlign.Center);
			nameText.SetHorizontalAlign(HorizontalAlign.Center);
			nameText.RenderLayer = ScreenSpaceRenderLayer;
			return thisPlayerKillEntity;
		}
		
		public Entity CreateManaCountEntity(string name, Vector2 pos)
		{
			var thisPlayerManaEntity = CreateEntity("manaCount_" + name, pos);
			var thisPlayerManaComponent = new ManaComponent(name);
			thisPlayerManaEntity.AddComponent(thisPlayerManaComponent);
			thisPlayerManaEntity.SetScale(2);
			
			var nameText = thisPlayerManaEntity.AddComponent(new TextComponent());
			nameText.Text = thisPlayerManaComponent.playerName +"'s Mana: " + thisPlayerManaComponent.mana;
			nameText.Color = Color.White;
			nameText.SetVerticalAlign(VerticalAlign.Center);
			nameText.SetHorizontalAlign(HorizontalAlign.Center);
			nameText.RenderLayer = ScreenSpaceRenderLayer;
			return thisPlayerManaEntity;
		}
		
		/*
		 * Deprecated chunks of code go here for safe keeping
		 *
		 * 
		 */
	}
}