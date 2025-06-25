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
    public partial class GrappleProj : BaseProjectile
    {
        public static List<GrappleProj> AllGrapples = new();
        public bool IsReeling = false;
        public Force Force;

        public override void Initialize(ProjectileInfo projectileInfo)
        {
            base.Initialize(projectileInfo);
            AllGrapples.Add(this);
        }

        public override void _PhysicsProcess(double delta)
        {
            (this as Node2D)._PhysicsProcess(delta);
            if (IsReeling)
            {
                ProjectileInfo.Duration -= (float)delta;
                if (ProjectileInfo.Duration <= 0f)
                {
                    SetToDestroy = true;
                    if (Force != null)
                        Force.SetToDelete = true;
                }
            }
            else
                Move(delta);

            if (ProjectileInfo.Range <= 0)
            {
                IsReeling = true;
                Force = new DestinationPullForce(ProjectileInfo.Destination.Body, this);

                ProjectileInfo.Destination.Body.ExternalVelocityInput.Add(Force);
            }
        }

    }
}
