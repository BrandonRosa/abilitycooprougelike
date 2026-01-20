using BrannPack.AbilityHandling;
using BrannPack.Character;
using BrannPack.Helpers.RecourcePool;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;
using static Godot.Image;

namespace BrannPack.Projectile
{
    public enum ProjectileCollideBehavior { Destroy,Pierce,Bounce}
    public class ProjectileInfo:DamageInfo
    {
        public ProjectileInfo(CharacterMaster source, CharacterMaster destination, (int sourceType, int sourceIndex, int sourceEffect) key,
             float damage, bool isCrit, Vector2 directionFrom = default, StatsHolder stats = null, EffectSourceType actionSourceType = EffectSourceType.Other, bool isSourcePsudo = false,
             string projectileName = "BaseProjectile", Vector2 direction = default, Vector2 position = default, float speed = 1000f, float duration = 5f, float range = 1000f, float collisionsLeft = 1f, ProjectileCollideBehavior bodyCollideBehavior=ProjectileCollideBehavior.Destroy, ProjectileCollideBehavior terrainCollideBehavior = ProjectileCollideBehavior.Destroy)
             : base(source, destination, key, damage, isCrit, directionFrom, stats,actionSourceType,isSourcePsudo) =>
            (ProjectileName, Direction, Position, Speed, Duration, Range, CollisionsLeft,BodyCollideBehavior,TerrainCollideBehavior) = (
                projectileName, direction, position, speed, duration, range, collisionsLeft,bodyCollideBehavior,terrainCollideBehavior);

        public string ProjectileName { get; set; } = "BaseProjectile";
        public Vector2 Direction { get; set; } = Vector2.Zero;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Speed { get; set; } = 1000f;
        public float Duration { get; set; } = 5f;
        public float Range { get; set; } = 1000f;

        public float CollisionsLeft { get; set; } = 1f;

        public ProjectileCollideBehavior BodyCollideBehavior = ProjectileCollideBehavior.Destroy;
        public ProjectileCollideBehavior TerrainCollideBehavior = ProjectileCollideBehavior.Destroy;

    }
    public interface IProjectile
    {

        public static event Action<IProjectile> OnGlobalProjectileFired;
        public static event Action<IProjectile> OnGlobalProjectileDestroyed;

        public static void InvokeGlobalFired(IProjectile projectile) { OnGlobalProjectileFired?.Invoke(projectile); }
        public static void InvokeGlobalDestroyed(IProjectile projectile) { OnGlobalProjectileDestroyed?.Invoke(projectile); }

        public event Action<IProjectile, Node> OnCollision;
        public event Action<IProjectile, Node> OnTerrainCollision;
        public event Action<IProjectile, Node> OnBodyCollision;
        public event Action<IProjectile, Node> OnOtherCollision;
        public event Action<IProjectile> OnMove;
        public event Action<IProjectile> OnDestroy;

        public Area2D Area { get; set; }

        public bool SetToDestroy { get; set; }
        public ProjectileInfo ProjectileInfo { get; set; }
        public void Move(double delta);
        public void Initialize(ProjectileInfo projectileInfo);
        public void Collide(Node node);
        public void Destroy();


        
    }
    [GlobalClass]
    public partial class BaseProjectile : Node2D, IProjectile
    {
        public static CollisionShape2D GetFirstCollisionShape2D(Node node)
        {
            if (node == null)
                return null;

            foreach (Node child in node.GetChildren())
            {
                if (child is CollisionShape2D shape)
                    return shape;

                // Optional: search deeper if needed
                var nested = GetFirstCollisionShape2D(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }
        public static void SimpleEstimatedBounce(IProjectile proj,Node col)
        {
            var collisionShape = GetFirstCollisionShape2D(col);
            if (collisionShape != null)
            {

                Vector2 collisionNormal = EstimateNormalFromShape((proj as Node2D).GlobalPosition,collisionShape);
                proj.ProjectileInfo.Direction=proj.ProjectileInfo.Direction.Bounce(collisionNormal).Normalized();
            }
            else
            {
                proj.ProjectileInfo.Direction *= -1;
            }
            proj.ProjectileInfo.CollisionsLeft--;
            if (proj.ProjectileInfo.CollisionsLeft < 0)
                proj.SetToDestroy = true;
        }
        public static Vector2 EstimateNormalFromShape(Vector2 orig,CollisionShape2D shape)
        {
            Vector2 toProjectile = orig - shape.GlobalPosition;
            return toProjectile.Normalized();
        }
        public Area2D Area { get; set; } = new Area2D();
        public bool SetToDestroy { get; set; } = false;
        public ProjectileInfo ProjectileInfo { get; set; } = new ProjectileInfo(null, null, (0, 0, 0), 0f, false);

        public event Action<IProjectile, Node> OnCollision;
        public event Action<IProjectile, Node> OnTerrainCollision;
        public event Action<IProjectile, Node> OnBodyCollision;
        public event Action<IProjectile, Node> OnOtherCollision;
        public event Action<IProjectile> OnMove;
        public event Action<IProjectile> OnDestroy;

        public override void _Ready()
        {
            base._Ready();
            Area = GetNode<Area2D>("Area2D");
            Area.BodyEntered+=Collide;

        }

        public virtual void Initialize(ProjectileInfo projectileInfo)
        {
            ProjectileInfo= projectileInfo;
            GD.Print(projectileInfo.ProjectileName);
            GlobalPosition = projectileInfo.Position;
            SetToDestroy = false;
            IProjectile.InvokeGlobalFired(this);
        }

        public virtual void Collide(Node body)
        {
            OnCollision?.Invoke(this, body);
            if (body is StaticBody2D staticBody)
            {
                // Handle collision with static body
                GD.Print($"Collided with static body: {staticBody.Name}");
                switch(ProjectileInfo.TerrainCollideBehavior)
                {
                    case ProjectileCollideBehavior.Destroy: SetToDestroy = true; break;
                    case ProjectileCollideBehavior.Bounce: SimpleEstimatedBounce(this, body); break;//bounce code here;
                }
                OnTerrainCollision?.Invoke(this, body);

            }
            else if (body is BaseCharacterBody character && ProjectileInfo.Source.CanDamageTeams.Contains(character.CharacterMaster.Team))
            {
                OnBodyCollision?.Invoke(this, body);
                ProjectileInfo.Destination = character.CharacterMaster;
                ProjectileInfo.Source.DealDamage(character.CharacterMaster, ProjectileInfo, null);
                switch (ProjectileInfo.BodyCollideBehavior)
                {
                    case ProjectileCollideBehavior.Destroy: SetToDestroy = true; break;
                    case ProjectileCollideBehavior.Bounce: SimpleEstimatedBounce(this, body); GD.Print("BOUNCE"); break;//bounce code here;
                }
            }
            else
                OnOtherCollision?.Invoke(this, body);

            
        }

        public virtual void Destroy()
        {
            IProjectile.InvokeGlobalDestroyed(this);
            OnDestroy?.Invoke(this);
            OnDestroy= null;
            SetToDestroy = false;
            PoolManager.PoolManagerNode.Return(ProjectileInfo.ProjectileName, this);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (SetToDestroy == true)
            {
                Destroy();
            }
        }


        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            
            ProjectileInfo.Duration -= (float)delta;
            if (ProjectileInfo.Duration <= 0f)
            {
                SetToDestroy = true;
            }
            Move(delta);
            if (ProjectileInfo.Range <= 0)
                SetToDestroy = true;

        }

        public virtual void Move(double delta)
        {
            OnMove?.Invoke(this);
            if (ProjectileInfo.Direction != Vector2.Zero)
            {
                var movevect = ProjectileInfo.Direction.Normalized() * ProjectileInfo.Speed * (float)delta;
                GlobalPosition += movevect;
                ProjectileInfo.Range -= movevect.Length();
                
            }

        }

    }
}
