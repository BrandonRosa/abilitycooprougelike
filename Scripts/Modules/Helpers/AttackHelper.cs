using BrannPack.Character;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    float sweepRadius, float sweepDepth, int checks, bool respectTerrain = true)
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
                Exclude = { characterBody.GetRid() },
                // Optionally, filter what should be checked (e.g., Ignore terrain layers if respectTerrain is true)
                CollisionMask = respectTerrain ? ~(1u << 0) : 0xFFFFFFFF// 0 assumes terrain is on layer 0 (you can modify based on your setup)
            };

            for (int i = 0; i < checks; i++)
            {
                float currentAngle = facingAngle - (sweepRadius / 2) + stepAngle * i; // Calculate angle
                Vector2 direction = new Vector2(1, 0).Rotated(Mathf.DegToRad(currentAngle)); // Rotate the unit vector
                Vector2 pointB = query.From + direction * sweepDepth; // End position based on depth

                // Perform raycast

                query.To = pointB;

                var result = spaceState.IntersectRay(query);

                // Process hits
                if (result.Count > 0)
                {
                    if (result["collider"].Obj is BaseCharacterBody charBody) // Avoid self-hit
                    {
                        hitCharacters.Add(charBody);
                    }

                    // Stop at terrain if enabled
                    if (result["collider"].Obj is StaticBody2D)
                    {
                        continue; // Skip further processing if terrain blocks the ray
                    }
                }
            }

            return hitCharacters.ToList();
        }
    }
}
