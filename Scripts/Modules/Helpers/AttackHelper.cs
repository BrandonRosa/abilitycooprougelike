using BrannPack.Character;
using BrannPack.Debugging;
using BrannPack.GameDirectrs;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;

namespace BrannPack.Helpers.Attacks
{
	public static class AttackHelper
	{
		public static List<BaseCharacterBody> GetCharactersInHitscan(BaseCharacterBody characterBody, Vector2 pointA, Vector2 pointB , int maxHit=int.MaxValue,bool respectTerrain = true)
		{
			List<BaseCharacterBody> hitCharacters = new List<BaseCharacterBody>();

			// Create the raycast
			var spaceState = characterBody.GetWorld2D().DirectSpaceState;
			var query = new PhysicsRayQueryParameters2D
			{
				// Set the ray start and end points
				From = pointA,
				To = pointB,
				Exclude = { characterBody.GetRid() },
				// Optionally, filter what should be checked (e.g., Ignore terrain layers if respectTerrain is true)
				CollisionMask = respectTerrain ? ~(1u << 0) : 0xFFFFFFFF// 0 assumes terrain is on layer 0 (you can modify based on your setup)
			};

			// Perform the raycast
			Dictionary result = spaceState.IntersectRay(query);

			// Check if the ray hits a valid object
			for (int i=0; result.Count>0 && i<maxHit;i++)
			{
				// Check if the hit object is a BaseCharacter
				if (result["collider"].Obj is BaseCharacterBody character)
				{
					hitCharacters.Add(character);
					query.Exclude.Add(character.GetRid());
				}

				// If respecting terrain, stop when it hits terrain
				if (result["collider"].Obj is StaticBody2D)
				{
					break; // Stop when hitting terrain
				}

				// Proceed to the next collision point
				result = spaceState.IntersectRay(query);
			}

			return hitCharacters;
		}

		public static List<BaseCharacterBody> GetCharactersInShotgunBlast(
			BaseCharacterBody characterBody, Transform2D origin, float facingAngle,
			float sweepRadius, float sweepDepth, int checks, bool respectTerrain = true, bool debug=true)
		{
			HashSet<BaseCharacterBody> hitCharacters = new HashSet<BaseCharacterBody>();
			var spaceState = characterBody.GetWorld2D().DirectSpaceState;

			float stepAngle = sweepRadius / (checks - 1); // Step size for each iteration

			// Create the raycast
			var query = new PhysicsRayQueryParameters2D
			{
				// Set the ray start and end points
				From = origin.Origin,
				To = Vector2.Zero,
				
			};

			for (int i = 0; i < checks; i++)
			{
				float currentAngle = facingAngle - (sweepRadius / 2) + stepAngle * i; // Calculate angle
				Vector2 direction = characterBody.AimDirection.Rotated(Mathf.DegToRad(currentAngle)); // Rotate the unit vector
				Vector2 pointB = query.From + direction * sweepDepth; // End position based on depth
				GD.Print(origin.Origin + " " + pointB);
				// Perform raycast

				query = new PhysicsRayQueryParameters2D
				{
					// Set the ray start and end points
					From = origin.Origin,
					To = pointB,

				};

				var result = spaceState.IntersectRay(query);

				if (debug)
				{
					// Draw the line for 0.5s using debug draw
					var DBL = new DebugLine();
					DBL.Initialize(origin.Origin, pointB, Colors.Red, 2, .5f);
					characterBody.GetTree().Root.AddChild(DBL); // Absolute root of the scene tree

				}

				// Process hits
				if (result.Count>0)
				{
					GD.Print("COUNT " + result.Count);
					if (result["collider"].Obj is BaseCharacterBody charBody) // Avoid self-hit
					{
						hitCharacters.Add(charBody);
						GD.Print("ADDED");
					}

					// Stop at terrain if enabled
					if (result["collider"].Obj is StaticBody2D)
					{
						GD.Print("STOPPED");
						continue; // Skip further processing if terrain blocks the ray
					}
				}
			}

			return hitCharacters.ToList();
		}

        public static List<BaseCharacterBody> GetCollisionsInBoxArea(
        Transform2D originTransform,float rotation,
        float width,
        float height,
		PhysicsDirectSpaceState2D spaceState,
        bool isAnchoredOnEdge = false,
        uint collisionMask = uint.MaxValue,
		bool debug=true
    )
        {
            

            // Build the box shape
            RectangleShape2D box = new RectangleShape2D
            {
                Size = new Vector2(width, height)
            };

            // Handle rotation

            // Calculate box transform
            Vector2 position = originTransform.Origin;

            if (isAnchoredOnEdge)
            {
                // Push the box forward by half height in its local up direction
                Vector2 forward = new Vector2(0, 1).Rotated(rotation-(float)Math.PI/2f);
                position += forward * (height / 2f);
            }

            Transform2D boxTransform = new Transform2D(rotation, position);

            // Build the query
            PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D
            {
                Shape = box,
                Transform = boxTransform,
                CollisionMask = collisionMask,
                CollideWithAreas = false,
                CollideWithBodies = true
            };

            // Get results
            var results = spaceState.IntersectShape(query);

			if(debug)
			{
                // Draw the line for 0.5s using debug draw
                var DBR = new DebugRect();
                DBR.Initialize(new Transform2D(rotation,position), box.Size, Colors.Red, 2);
                GameDirector.instance.GetTree().Root.AddChild(DBR); // Absolute root of the scene tree
            }

            // Filter results and cast to PhysicsBody2D
            List<BaseCharacterBody> hitBodies = new();

            foreach (var result in results)
            {
                if (result["collider"].Obj is BaseCharacterBody charBody)
                {
                    hitBodies.Add(charBody);
                }
            }

            return hitBodies;
        }

        public static List<BaseCharacterBody> GetCollisionsInCircleArea(
        Transform2D originTransform, float rotation,
        float radius,
        PhysicsDirectSpaceState2D spaceState,
        bool isAnchoredOnEdge = false,
        uint collisionMask = uint.MaxValue,
        bool debug = true
    )
        {


            // Build the box shape
            CircleShape2D circle = new CircleShape2D
            {
                Radius = radius
            };

            // Handle rotation

            // Calculate box transform
            Vector2 position = originTransform.Origin;

            if (isAnchoredOnEdge)
            {
                // Push the box forward by half height in its local up direction
                Vector2 forward = new Vector2(0, 1).Rotated(rotation - (float)Math.PI / 2f);
                position += forward * (radius);
            }

            Transform2D boxTransform = new Transform2D(rotation, position);

            // Build the query
            PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D
            {
                Shape = circle,
                Transform = boxTransform,
                CollisionMask = collisionMask,
                CollideWithAreas = false,
                CollideWithBodies = true
            };

            // Get results
            var results = spaceState.IntersectShape(query);

            if (debug)
            {
                // Draw the line for 0.5s using debug draw
                var DBC = new DebugCircle();
                DBC.Initialize(new Transform2D(rotation, position), circle.Radius, Colors.Red, 2);
                GameDirector.instance.GetTree().Root.AddChild(DBC); // Absolute root of the scene tree
            }

            // Filter results and cast to PhysicsBody2D
            List<BaseCharacterBody> hitBodies = new();

            foreach (var result in results)
            {
                if (result["collider"].Obj is BaseCharacterBody charBody)
                {
                    hitBodies.Add(charBody);
                }
            }

            return hitBodies;
        }

        public static (bool IsSuccess, int SuccessfulRolls, int LuckUsed, int BadLuckUsed) RollWithProcAndLucks(float chance,float procChance, float luck, float badLuck)
		{
			int rerolls = (int)Mathf.Floor(luck) +(Roll(luck% 1f)?1:0);
			int badReroll = (int)Mathf.Floor(badLuck) + (Roll(badLuck % 1f) ? 1 : 0);
			int successRolls = (int)Mathf.Floor(chance* procChance);
			float percent = (chance* procChance)%1f;

			bool success = false;
			var rglbl = RollGoodLuckBadLuck(percent,rerolls,badReroll);
			successRolls += (rglbl.Success ? 1 : 0);
			if (successRolls> 0)
				success = true;
			return (success, successRolls,rerolls-rglbl.GoodLuckLeft,badReroll-rglbl.BadLuckLeft);
		}
		private static (bool Success,int GoodLuckLeft, int BadLuckLeft) RollGoodLuckBadLuck(float chance, int goodLuckLeft, int badLuckLeft)
		{
			if (Roll(chance))
			{
				if (0 < badLuckLeft)
					return RollGoodLuckBadLuck(chance, goodLuckLeft, badLuckLeft - 1);
				else
					return (true, goodLuckLeft, badLuckLeft);
			}
			else
			{
				if (0 < goodLuckLeft)
					return RollGoodLuckBadLuck(chance, goodLuckLeft - 1, badLuckLeft);
				else
					return (false, goodLuckLeft, badLuckLeft);
			}
		}
		private static bool Roll(float chance)
		{
			return GD.Randf() < chance; // GD.Randf() generates a float between 0 and 1
		}
	}
}
