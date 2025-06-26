using BrannPack.Character;
using BrannPack.Debugging;
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
		public float EaseOutRange = 150f;
		public float StopRange =  5f;
		public float MaxAcc = 20f;
		public float MaxSpeed = 20f;
		public float DeleteDistance = -1f;
		public float deltaTime = 0f;
		public IForcable PullVictim;
		public Node2D Puller;
		public DestinationPullForce(IForcable pullVictim, Node2D puller, float maxSpeed = 550f, float easeOutRange = 150f, float stopRange = 10f, float maxAcc = 300f, float deleteDistance=10f)
		{
			PullVictim = pullVictim;
			Puller = puller;
			MaxAcc = maxAcc;
			MaxSpeed = maxSpeed;
			EaseOutRange = easeOutRange;
			StopRange = stopRange;
			DeleteDistance = deleteDistance;
		}
		/// <summary>
		/// NOT A FUCTION OF VELOCITY OUTSIDE OF OUTRANGE
		/// dV IS THE NEW VELOCITY MINUS THE OLD ONE
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		public override Vector2 SetdV(double delta)
		{
            Vector2 direction = Puller.GlobalPosition - PullVictim.FPosition;
            var oldVel = Velocity;
			GD.Print("oldVel:" + oldVel);
            GD.Print("Dist:" + direction.Length());
            
			
			var targetVelocity= CalculateGrappleVelocity(direction, PullVictim.FVelocity, MaxAcc*deltaTime,EaseOutRange,StopRange).LimitLength(MaxSpeed);
            //targetVelocity = (targetVelocity - PullVictim.FVelocity).LimitLength(Acceleration * (float)delta);
            dV = (targetVelocity - oldVel);
            Velocity = targetVelocity;
            GD.Print("Calc:" + Velocity + "-" + oldVel+"="+dV);
			
			deltaTime += (float)delta;

			if (direction.Length() < DeleteDistance)
			{
				SetToDelete = true;

			}
			if(true)
			{
                var DBL = new DebugLine();
                DBL.Initialize(PullVictim.FPosition, (dV*(float)100f+PullVictim.FPosition), Colors.Red, 2, .2f);
                var DBL2 = new DebugLine();
                DBL2.Initialize(PullVictim.FPosition, (Velocity* (float)100f + PullVictim.FPosition), Colors.Green, 2, .2f);
                ((Node2D)PullVictim).GetTree().Root.AddChild(DBL); // Absolute root of the scene tree
                ((Node2D)PullVictim).GetTree().Root.AddChild(DBL2);
            }

			return dV;
		}

		public static Vector2 CalculateGrappleVelocity(Vector2 direction,Vector2 currentVelocity, float maxSpeed, float easeOutDistance, float stopDistance)
		{
			float distance = direction.Length();
			float speed= maxSpeed;
			if(distance < stopDistance)
            {
				var dot = currentVelocity.Dot(direction.Normalized());
				if(dot>0)
					direction =  dot* direction.Normalized();
            }
			else if (distance < easeOutDistance)
			{
				// Ease out the speed as we approach the target
                speed = Mathf.Lerp(maxSpeed*2, 1, (easeOutDistance - distance) / easeOutDistance);
			}
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
