using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;

namespace BrannPack.Projectile
{
    public class ProjectileInfo:EventInfo
    {
        public ProjectileInfo(CharacterMaster source, CharacterMaster destination, (int sourceType, int sourceIndex, int sourceEffect) key,
             float damage, bool isCrit, Vector2 directionFrom = default, StatsHolder stats = null)
             : base(source, destination, key) =>
             (Damage, IsCrit, DirectionFrom, Stats) = (damage, isCrit, directionFrom, stats);

        public Vector2 Direction { get; set; }
        public Vector2 Position { get; set; }
        public float Speed { get; set; } = 1000f;
        public float LifeTime { get; set; } = 5f;
        public float Range { get; set; } = 1000f;

        public float Damage 

        public int MaxCollisions = 1;

        public StatsHolder
    }
    public interface IProjectile
    {

        public void Initialize(ProjectileInfo projectileInfo);
        public void Fire(Vector2 direction, Vector2 position);
        public void Update(float delta);
        public void Destroy();

        public List<Action<IProjectile>> OnCollision { get; set; }

        public ProjectileInfo InitialProjectileInfo { get; set; }
        public ProjectileInfo ProjectileInfo { get; set; }
    }
    public partial class BaseProjectile: Node2D, IProjectile
    {
    }
}
