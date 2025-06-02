using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;

namespace BrannPack.Projectile
{
    public class ProjectileInfo:DamageInfo
    {
        public ProjectileInfo(CharacterMaster source, CharacterMaster destination, (int sourceType, int sourceIndex, int sourceEffect) key,
             float damage, bool isCrit, Vector2 directionFrom = default, StatsHolder stats = null,
             string projectileName = "BaseProjectile", Vector2 direction = default, Vector2 position = default, float speed = 1000f, float duration = 5f, float range = 1000f, float collisionsLeft = 1f)
             : base(source, destination, key, damage, isCrit, directionFrom, stats) =>
            (ProjectileName, Direction, Position, Speed, Duration, Range, CollisionsLeft) = (
                projectileName, direction, position, speed, duration, range, collisionsLeft);

        public string ProjectileName { get; set; } = "BaseProjectile";
        public Vector2 Direction { get; set; } = Vector2.Zero;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Speed { get; set; } = 1000f;
        public float Duration { get; set; } = 5f;
        public float Range { get; set; } = 1000f;

        public float CollisionsLeft { get; set; } = 1f;

    }
    public interface IProjectile
    {

        public static event Action<IProjectile> OnGlobalProjectileFired;
        public static event Action<IProjectile> OnGlobalProjectileDestroyed;

        public static void InvokeGlobalFired(IProjectile projectile) { OnGlobalProjectileFired?.Invoke(projectile); }
        public static void InvokeGlobalDestroyed(IProjectile projectile) { OnGlobalProjectileDestroyed?.Invoke(projectile); }

        public List<Node> CollisionsLastFrame { get; set; }
        public void Move();
        public void Initialize(ProjectileInfo projectileInfo);
        public void Collide(Node node);
        public void Destroy();

        public event Action<IProjectile, Node> OnCollision;
        public event Action<IProjectile> OnMove;

        //public ProjectileInfo InitialProjectileInfo { get; set; }
        public ProjectileInfo ProjectileInfo { get; set; }
    }
    public partial class BaseProjectile : Node2D, IProjectile
    {
        public Area2D Area2D { get; set; } = new Area2D();
        public List<Node> CollisionsLastFrame { get; set; } = new List<Node>();
        public ProjectileInfo ProjectileInfo { get; set; } = new ProjectileInfo(null, null, (0, 0, 0), 0f, false);

        public event Action<IProjectile, Node> OnCollision;
        public event Action<IProjectile> OnMove;

        public override void _Ready()
        {
            base._Ready();
            Area2D = GetNode<Area2D>("Area2D");
            Area2D.BodyEntered+=Collide;
        }

        public void Initialize(ProjectileInfo projectileInfo)
        {
            ProjectileInfo= projectileInfo;
            GlobalPosition = projectileInfo.Position;
            IProjectile.InvokeGlobalFired(this);
        }

        public void Collide(Node body)
        {
            if(body is StaticBody2D staticBody)
            {
                // Handle collision with static body
                GD.Print($"Collided with static body: {staticBody.Name}");

                Destroy();
            }
            OnCollision?.Invoke(this, body);
            if (body is BaseCharacterBody character && ProjectileInfo.Source.CanDamageTeams.Contains(character.CharacterMaster.Team))
            {
                ProjectileInfo.Source.DealDamage(character.CharacterMaster, ProjectileInfo,null);
            }
        }

        public void Destroy()
        {
            throw new NotImplementedException();
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
        
    }
}
