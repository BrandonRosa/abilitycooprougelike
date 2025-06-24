using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Forces
{
	public abstract class Force
	{
		public Vector2 Velocity = Vector2.Zero;
		public abstract Vector2 GetdV(double delta);
		public abstract bool ShouldDelete();
	}

	public class LerpForce:Force
	{
		public float Acceleration = 100f;
		public Vector2 ToVelocity;

		public LerpForce(float acceleration, Vector2 toVelocity)
		{
			Acceleration = acceleration;
			ToVelocity = toVelocity;
		}

		public override Vector2 GetdV(double delta)
		{
			var oldVel = Velocity;
			float t = 1f - Mathf.Exp(-Acceleration * (float)delta); // exponential smoothing
			Velocity = Velocity.Lerp(ToVelocity, t);
			var dV = Velocity - oldVel;
			return dV;
		}

		public override bool ShouldDelete() { return false; }
	}

	public class DestinationPullForce
	{
		public float Acceleration= 200f;
		public float Deceleration=80f;
		public float EaseOutRange = 100f;
		public float StopRange = 20f;
		public float MaxSpeed = 100f;
		public float easeInSpeed = 10f;
		public DestinationPullForce()
		{
		}

		public  Vector2 GetdV(double delta, Vector2 currentPosition, Vector2 targetPosition)
		{
			return Vector2.Zero;
		}

		public static Vector2 CalculateGrappleVelocity(Vector2 currentPosition, Vector2 targetPosition,Vector2 currentVelocity, float maxSpeed, float easeInSpeed, float easeOutDistance)
		{
			Vector2 direction = currentPosition.DirectionTo(targetPosition);
			float distance = currentPosition.DistanceTo(targetPosition);
			float currSpeed = currentVelocity.Dot(direction);

			if (distance <= 0.01f)
				return Vector2.Zero;

			float t;

			if (distance > easeOutDistance)
			{
				// Ease-out zone
				float remaining = distance - easeOutDistance;
				float totalEaseOut = Mathf.Max(easeOutDistance, 0.001f); // Avoid divide by zero
				t = Mathf.SmoothStep(1f, 0f, remaining / totalEaseOut);
			}
			else if(currSpeed <maxSpeed)
			{
				// Ease-in zone
				t = Mathf.SmoothStep(0f, 1f, currSpeed / maxSpeed);
			}
			else
			{
				// Constant zone
				t = 1f;
			}

			float speed = maxSpeed * t;
			return direction * speed;
		}
	}
}
