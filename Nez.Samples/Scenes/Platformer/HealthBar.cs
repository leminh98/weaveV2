using Lidgren.Network;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;

namespace Nez.Samples
{
    public class HealthBar : Component, ITriggerListener, IUpdatable
	{

		private SpriteAnimator _healthBarAnimator;
		public int maxHP = 5;
		public int currentHP;
		public SpriteRenderer _sprite;

		public override void OnAddedToEntity()
		{
			var healthTexture = Entity.Scene.Content.Load<Texture2D>("Platformer/healthbar");
			var healthSprites = Sprite.SpritesFromAtlas(healthTexture, 64, 6);
			_healthBarAnimator = Entity.AddComponent(new SpriteAnimator(healthSprites[5]));
			_sprite = Entity.GetComponent<SpriteRenderer>();
			currentHP = maxHP;
			
			#region Health Animation Setup
			_healthBarAnimator.AddAnimation("5", new[]
			{
				healthSprites[0]
			});
			_healthBarAnimator.AddAnimation("4", new[]
			{
				healthSprites[1]
			});
			_healthBarAnimator.AddAnimation("3", new[]
			{
				healthSprites[2]
			});
			_healthBarAnimator.AddAnimation("2", new[]
			{
				healthSprites[3]
			});
			_healthBarAnimator.AddAnimation("1", new[]
			{
				healthSprites[4]
			});
			_healthBarAnimator.AddAnimation("0", new[]
			{
				healthSprites[5]
			});
			#endregion
		}

		public override void OnRemovedFromEntity()
		{
		}

		void IUpdatable.Update()
		{
			var healthComponent = currentHP;
			if (healthComponent < 0)
				healthComponent = 0;
			_healthBarAnimator.Play(healthComponent.ToString());
		}
		

		
		#region ITriggerListener implementation

		void ITriggerListener.OnTriggerEnter(Collider other, Collider self)
		{
			Debug.Log("triggerEnter: {0}", other.Entity.Name);
		}

		void ITriggerListener.OnTriggerExit(Collider other, Collider self)
		{
			Debug.Log("triggerExit: {0}", other.Entity.Name);
		}

		#endregion
	}
}