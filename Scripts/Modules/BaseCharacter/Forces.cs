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
		public virtual Vector2 Velocity { get; set; } = Vector2.Zero;
		public virtual Vector2 dV { get; set; } = Vector2.Zero;
		public virtual bool SetToDelete { get; set; } = false;
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
		public float EaseOutRange = 150f;
		public float StopRange =  5f;
		public float MaxSpeed = 20f;
		public float EaseInSpeed = 10f;
		public float DeleteDistance = -1f;
		public IForcable PullVictim;
		public Node2D Puller;
		public DestinationPullForce(IForcable pullVictim, Node2D puller, float maxSpeed = 20f, float easeInSpeed = 10f, float easeOutRange = 150f, float stopRange = 5f, float acceleration = 200f, float deceleration = 80f, float deleteDistance=-1)
		{
			PullVictim = pullVictim;
			Puller = puller;
			MaxSpeed = maxSpeed;
			EaseInSpeed = easeInSpeed;
			EaseOutRange = easeOutRange;
			StopRange = stopRange;
			Acceleration = acceleration;
			Deceleration = deceleration;
			DeleteDistance = deleteDistance;
		}

		public override Vector2 SetdV(double delta)
		{
			var oldVel = Velocity;
			GD.Print("oldVel:" + oldVel);
			Vector2 direction = Puller.GlobalPosition - PullVictim.FPosition;
			GD.Print("Dist:" + direction.Length());
			var targetVelocity= CalculateGrappleVelocity(direction, PullVictim.FVelocity, MaxSpeed,EaseInSpeed,EaseOutRange,StopRange);
			float t = 1f - Mathf.Exp(-Acceleration * (float)delta); // exponential smoothing
			
			GD.Print("Calc:" + Velocity + "-" + PullVictim.FVelocity);
			dV = (targetVelocity-PullVictim.FVelocity.Dot(targetVelocity.Normalized()))(0,MaxSpeed);

			if (direction.Length() < DeleteDistance)
			{
				SetToDelete = true;
			}
			return dV;
		}

		public static Vector2 CalculateGrappleVelocity(Vector2 direction,Vector2 currentVelocity, float maxSpeed, float easeInSpeed, float easeOutDistance, float stopDistance)
		{
			float distance = direction.Length();

			return direction.Normalized() * maxSpeed;
		}
	}

	public class FrictionalForce:Force
	{
		public IForcable ForceObject;
		public float FrictionConstant = .5f;
		public bool UseFriction=false;
		public Vector2 IgnoredVelocity = Vector2.Zero;
		
		public FrictionalForce(IForcable forceObject,float frictionConstant=5f)
		{
			ForceObject = forceObject;
			FrictionConstant = frictionConstant;
		}

		public override Vector2 SetdV(double delta)
		{
			if(!UseFriction)
			{
				dV = Vector2.Zero;
				Velocity = Vector2.Zero;
				return dV;
			}
			var mov = ForceObject.FVelocity - IgnoredVelocity;

			Velocity = -(mov) * FrictionConstant * (float)delta;
			dV = Velocity;
			return dV;
		}
	}
}
