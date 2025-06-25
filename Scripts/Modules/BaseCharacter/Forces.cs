using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Forces
{
	public interface IForcable
	{
		public HashSet<Force> ExternalVelocityInput { get; set; }
		public Vector2 FPosition { get; }

		public Vector2 FVelocity { get; }
	}
	public abstract class Force
	{
		public Vector2 Velocity = Vector2.Zero;
		public Vector2 dV = Vector2.Zero;
		public bool SetToDelete = false;
		public abstract Vector2 SetdV(double delta);
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

		public override Vector2 SetdV(double delta)
		{
			var oldVel = Velocity;
			float t = 1f - Mathf.Exp(-Acceleration * (float)delta); // exponential smoothing
			Velocity = Velocity.Lerp(ToVelocity, t);
			dV = Velocity - oldVel;
			return dV;
		}
	}

	public class DestinationPullForce:Force
	{
		public float Acceleration= 200f;
		public float Deceleration=80f;
		public float EaseOutRange = 100f;
		public float StopRange = 20f;
		public float MaxSpeed = 30f;
		public float EaseInSpeed = 10f;
		public IForcable PullVictim;
		public Node2D Puller;
		public DestinationPullForce(IForcable pullVictim, Node2D puller, float maxSpeed = 30f, float easeInSpeed = 10f, float easeOutRange = 100f, float stopRange = 20f, float acceleration = 200f, float deceleration = 80f)
        {
            PullVictim = pullVictim;
            Puller = puller;
            MaxSpeed = maxSpeed;
            EaseInSpeed = easeInSpeed;
            EaseOutRange = easeOutRange;
            StopRange = stopRange;
            Acceleration = acceleration;
            Deceleration = deceleration;
        }

		public override Vector2 SetdV(double delta)
		{
			var oldVel = Velocity;
			var targetVelocity= CalculateGrappleVelocity(PullVictim.FPosition,Puller.GlobalPosition,PullVictim.FVelocity,MaxSpeed,EaseInSpeed,EaseOutRange,StopRange);
            float t = 1f - Mathf.Exp(-Acceleration * (float)delta); // exponential smoothing
            Velocity = Velocity.Lerp(targetVelocity, t);
			dV = Velocity - oldVel;
			return dV;
        }

		public static Vector2 CalculateGrappleVelocity(Vector2 currentPosition, Vector2 targetPosition,Vector2 currentVelocity, float maxSpeed, float easeInSpeed, float easeOutDistance, float stopDistance)
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
