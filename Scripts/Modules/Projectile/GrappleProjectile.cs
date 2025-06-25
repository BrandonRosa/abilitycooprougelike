using BrannPack.Forces;
using BrannPack.Projectile;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Projectile
{
    [GlobalClass]
    public partial class GrappleProjectile : BaseProjectile
    {
        public static List<GrappleProjectile> AllGrapples = new();
        public bool IsReeling = false;
        public Force Force;

        public override void Initialize(ProjectileInfo projectileInfo)
        {
            base.Initialize(projectileInfo);
            Reset();
            AllGrapples.Add(this);
        }
        
        public void Reset()
        {
            IsReeling = false;
            Force = null;
        }

        public override void _PhysicsProcess(double delta)
        {
            //(this as Node2D)._PhysicsProcess(delta);
            if (IsReeling)
            {
                ProjectileInfo.Duration -= (float)delta;
                if (ProjectileInfo.Duration <= 0f)
                {
                    SetToDestroy = true;
                    if (Force != null)
                    {
                        Force.SetToDelete = true;
                        GD.Print("DELLLIII");
                    }

                }
                else if (Force != null && Force.SetToDelete)
                    SetToDestroy = true;
            }
            else
            {
                Move(delta);

                if (ProjectileInfo.Range <= 0)
                {
                    IsReeling = true;
                    Force = new DestinationPullForce(ProjectileInfo.Destination.Body, this, deleteDistance: 10f, stopRange: 10f, maxSpeed: 400f);

                    ProjectileInfo.Destination.Body.ExternalVelocityInput.Add(Force);
                }
            }
        }

    }
}
