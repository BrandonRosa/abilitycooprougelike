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
             float damage, bool isCrit, Vector2 directionFrom = default, StatsHolder stats = null,
             string projectileName = "BaseProjectile", Vector2 direction = default, Vector2 position = default, float speed = 1000f, float duration = 5f, float range = 1000f, float collisionsLeft = 1f, ProjectileCollideBehavior bodyCollideBehavior=ProjectileCollideBehavior.Destroy, ProjectileCollideBehavior terrainCollideBehavior = ProjectileCollideBehavior.Destroy)
             : base(source, destination, key, damage, isCrit, directionFrom, stats) =>
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

        public Area2D Area { get; set; }

        public bool SetToDestroy { get; set; }
        public ProjectileInfo ProjectileInfo { get; set; }
        public void Move();
        public void Initialize(ProjectileInfo projectileInfo);
        public void Collide(Node node);
        public void Destroy();


        
    }
    public partial class BaseProjectile : Node2D, IProjectile
    {
        public static void SimpleEstimatedBounce(IProjectile proj,Node col)
        {
            var collisionShape = col.GetChild<CollisionShape2D>(0);
            if (collisionShape != null)
            {
                Vector2 collisionNormal = EstimateNormalFromShape(proj.ProjectileInfo.Position,collisionShape);
                proj.ProjectileInfo.Direction.Bounce(collisionNormal).Normalized();
            }
            else
            {
                proj.ProjectileInfo.Direction *= -1;
            }
            proj.ProjectileInfo.CollisionsLeft--;
            if (proj.ProjectileInfo.CollisionsLeft <= 0)
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

        public override void _Ready()
        {
            base._Ready();
            Area = GetNode<Area2D>("Area2D");
            Area.BodyEntered+=Collide;

        }

        public void Initialize(ProjectileInfo projectileInfo)
        {
            ProjectileInfo= projectileInfo;
            GlobalPosition = projectileInfo.Position;
            IProjectile.InvokeGlobalFired(this);
        }

        public void Collide(Node body)
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
                ProjectileInfo.Source.DealDamage(character.CharacterMaster, ProjectileInfo, null);
            }
            else
                OnOtherCollision?.Invoke(this, body);

            if (SetToDestroy == true)
            {
                Destroy();
            }
        }

        public virtual void Destroy()
        {
            IProjectile.InvokeGlobalDestroyed(this);
            PoolManager.PoolManagerNode.Return(ProjectileInfo.ProjectileName, this);
        }


        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            
            ProjectileInfo.Duration -= (float)delta;
            if (ProjectileInfo.Duration <= 0f)
            {
                Destroy();
            }
            Move(delta);
            
        }

        public void Move(double delta)
        {
            OnMove?.Invoke(this);
            if (ProjectileInfo.Direction != Vector2.Zero)
            {
                GlobalPosition += ProjectileInfo.Direction.Normalized() * ProjectileInfo.Speed * (float)delta;
            }

        }

        public void Move()
        {
            throw new NotImplementedException();
        }
    }
}
