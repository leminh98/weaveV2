using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Samples
{
	public class MovingPlatform : Component, IUpdatable
	{
		float _minX;
		float _maxX;
		float _minY;
		float _maxY;
		float _speedFactor;

		private float _deltaX;
		private float _deltaY;


		public MovingPlatform(float deltaX, float deltaY, float speedFactor = 2f)
		{
			_deltaX = deltaX;
			_deltaY = deltaY;
			_speedFactor = speedFactor;
		}


		public override void OnAddedToEntity()
		{
			_minX = Entity.Position.X;
			_minY = Entity.Position.Y;

			_maxX = _minX + _deltaX;
			_maxY = _minY + _deltaY;
		}


		void IUpdatable.Update()
		{
			var x = Mathf.PingPong(Time.TotalTime, 1f);
			var xToTheSpeedFactor = Mathf.Pow(x, _speedFactor);
			var alpha = 1f - xToTheSpeedFactor / xToTheSpeedFactor + Mathf.Pow(1 - x, _speedFactor);

			var deltaY = Tweens.Lerps.Lerp(_minY, _maxY, alpha) - Entity.Position.Y;
			var deltaX = Tweens.Lerps.Lerp(_minX, _maxX, alpha) - Entity.Position.X;

			// TODO: probably query Physics to fetch the actors that we will intersect instead of blindly grabbing them all
			var ridingActors = GetAllRidingActors();

			MoveSolid(new Vector2(deltaX, deltaY), ridingActors);
		}


		void MoveSolid(Vector2 motion, List<Entity> ridingActors)
		{
			if (motion.X == 0 && motion.Y == 0)
				return;

			MoveSolidX(motion.X, ridingActors);
			MoveSolidY(motion.Y, ridingActors);
		}


		void MoveSolidX(float amount, List<Entity> ridingActors)
		{
			var moved = false;
			Entity.Position += new Vector2(amount, 0);

			var platformCollider = Entity.GetComponent<Collider>();
			var colliders = new HashSet<Collider>(Physics.BoxcastBroadphaseExcludingSelf(platformCollider));
			foreach (var collider in colliders)
			{

				var _caveman = collider.Entity.GetComponent<Caveman>();
				if (_caveman == null)
					continue;

				float pushAmountX = 0;
				float pushAmountY = 0;
				
				// if (amount > 0)
				// 	pushAmountX = platformCollider.Bounds.Right - collider.Bounds.Left;
				// else
				// 	pushAmountX = platformCollider.Bounds.Left - collider.Bounds.Right;
				
				float right_dist = platformCollider.Bounds.Right - (collider.Bounds.Left - _caveman._velocity.X * Time.DeltaTime);
				float left_dist = platformCollider.Bounds.Left - (collider.Bounds.Right - _caveman._velocity.X * Time.DeltaTime);
				float bottom_dist = platformCollider.Bounds.Bottom - (collider.Bounds.Top - _caveman._velocity.Y * Time.DeltaTime);
				float top_dist = platformCollider.Bounds.Top - (collider.Bounds.Bottom - _caveman._velocity.Y * Time.DeltaTime);
				
				System.Console.WriteLine(right_dist.ToString() + " " + left_dist.ToString() + " " + bottom_dist.ToString() + " " + top_dist.ToString());
				
				if (-left_dist < right_dist &&
				    -left_dist < -top_dist * 4 &&
				    -left_dist < bottom_dist)
				{
					pushAmountX = platformCollider.Bounds.Left - collider.Bounds.Right;
				}
				else if (right_dist < -top_dist * 4 &&
				         right_dist < bottom_dist)
				{
					pushAmountX = platformCollider.Bounds.Right - collider.Bounds.Left;
				}
				else if (-top_dist < bottom_dist)
				{
					pushAmountY = platformCollider.Bounds.Top - collider.Bounds.Bottom;
					_caveman._velocity.Y = 0;
					_caveman._collisionState.Below = true;
				}
				else
				{
					pushAmountY = platformCollider.Bounds.Bottom - collider.Bounds.Top;
				}
				
				collider.Entity.Position += new Vector2(pushAmountX, pushAmountY);
				
				// var mover = collider.Entity.GetComponent<Mover>();
				// if (mover != null)
				// {
				// 	moved = true;
				// 	CollisionResult collisionResult;
				// 	if (mover.Move(new Vector2(pushAmountX, 0), out collisionResult))
				// 	{
				// 		collider.Entity.Destroy();
				// 		return;
				// 	}
				// }
				// else
				// {
				// 	collider.Entity.Position += new Vector2(pushAmountX, pushAmountY);
				// }
			}


			foreach (var ent in ridingActors)
			{
				if (!moved)
					ent.Position += new Vector2(amount, 0);
			}
		}


		void MoveSolidY(float amount, List<Entity> ridingActors)
		{
			var moved = false;
			Entity.Position += new Vector2(0, amount);

			var platformCollider = Entity.GetComponent<Collider>();
			var colliders = new HashSet<Collider>(Physics.BoxcastBroadphaseExcludingSelf(platformCollider));
			foreach (var collider in colliders)
			{
				float pushAmount;
				if (amount > 0)
					pushAmount = platformCollider.Bounds.Bottom - collider.Bounds.Top;
				else
					pushAmount = platformCollider.Bounds.Top - collider.Bounds.Bottom;

				var mover = collider.Entity.GetComponent<Mover>();
				if (mover != null)
				{
					moved = true;
					CollisionResult collisionResult;
					if (mover.Move(new Vector2(0, pushAmount), out collisionResult))
					{
						collider.Entity.Destroy();
						return;
					}
				}
				else
				{
					collider.Entity.Position += new Vector2(0, pushAmount);
				}
			}


			foreach (var ent in ridingActors)
			{
				if (!moved)
					ent.Position += new Vector2(0, amount);
			}
		}


		/// <summary>
		/// brute force search for Entities on top of this Collider. Not a great approach.
		/// </summary>
		/// <returns>The all riding actors.</returns>
		List<Entity> GetAllRidingActors()
		{
			var list = new List<Entity>();
			var platformCollider = Entity.GetComponent<Collider>();

			var entities = Entity.Scene.FindEntitiesWithTag(0);
			for (var i = 0; i < entities.Count; i++)
			{
				var collider = entities[i].GetComponent<Collider>();
				if (collider == platformCollider || collider == null)
					continue;

                if (collider.CollidesWith(platformCollider, new Vector2(0f, 1f), out CollisionResult collisionResult))
                    list.Add(entities[i]);
            }

			return list;
		}
	}
}