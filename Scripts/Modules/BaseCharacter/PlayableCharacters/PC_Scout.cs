using BrannPack.AbilityHandling;
using BrannPack.Helpers.Attacks;
using BrannPack.Helpers.RecourcePool;
using BrannPack.InputHelpers;
using BrannPack.ModifiableStats;
using BrannPack.Projectile;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;

namespace BrannPack.Character.Playable
{
	

	public static class PC_Scout
	{
		//public static PackedScene Prefab { get; set; } = (PackedScene)ResourceLoader.Load("res://path_to_prefab.tscn");
	}

	public class DualPistols: Ability<DualPistols>
	{
		protected static RangeStat BulletRange = new RangeStat(400f, 600f, 250f);
		protected static DamageStat Damage = new DamageStat(3f, .5f);
		protected static FireRateStat FireRate = new FireRateStat(3.5f);

		public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>()
		{
			{ Stat.Range, BulletRange},
			{Stat.Damage, Damage },
			{Stat.FireRate, FireRate }
		},
			new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>())
		{ };

		public override string Name { get; protected set; } = "Dual Pistols";
		public override string CodeName { get; protected set; } = "Scout_Pistols";
		public override string Description { get; protected set; }
		public override string AdvancedDescription { get; protected set; }
		public override Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/Phase_Blast.png");

		//public AbilityUpgrade SSG_U1_Cooldown=
		public override BaseCharacterBody UpdateTarget()
		{
			return null;
		}

		public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo = null, EventChain eventChain = null)
		{
			if (master.Body.CooldownHandler.IsOnCooldown((1, this.Index, 0)))
				return;
			var bullet=PoolManager.PoolManagerNode.Spawn<BaseProjectile>("BasicEnemyBullet", PoolManager.ProjectilesNode);
			var damagestat = Damage.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(Stat.Damage));
			var rangestat = BulletRange.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(Stat.Range));
			var fireratestat = FireRate.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<FireRateStat>(Stat.FireRate));
			bullet.Initialize(new ProjectileInfo(master, null, (1, this.Index, 0), damagestat.CalculateTotal(), false, projectileName: "BasicEnemyBullet", direction: master.Body.AimDirection, position: master.Body.GlobalPosition, duration: float.MaxValue, range: rangestat.CalculateTotal(),speed:1500f));
			float cooldown=1f / (fireratestat.CalculateTotal());
			master.Body.CooldownHandler.AddCooldown((1, this.Index, 0), cooldown);
		}
	}

	public class ScoutShotGun : Ability<ScoutShotGun>
	{
		private float BlastWidth = 15;
		protected static RangeStat BlastRange = new RangeStat(100f, 500f, .5f);
		protected static DamageStat Damage = new DamageStat(60, .9f);
		protected static CooldownStat Cooldown = new CooldownStat(8f);
		protected static CooldownStat SpamCooldown = new CooldownStat(.1f);
		protected static ChargeStat Charges = new ChargeStat(2f);

		public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>()
			{
				{ Stat.Range, BlastRange },
				{ Stat.Damage, Damage },
				{ Stat.Cooldown, Cooldown },
				{ Stat.Charges, Charges },
				{Stat.SpamCooldown, SpamCooldown },
			},
		   new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>()) {
		};
		public override string Name { get; protected set; } = "Shotgun";
		public override string CodeName { get; protected set; } = "Scout_Shotgun";
		public override string Description { get; protected set; }
		public override string AdvancedDescription { get; protected set; }
		public override Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/Phase_Blast.png");

		//public AbilityUpgrade SSG_U1_Cooldown=
		public override BaseCharacterBody UpdateTarget()
		{
			return null;
		}

		public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo = null, EventChain eventChain=null)
		{
			GD.Print("SHOTGUN","Charge"+abilitySlot.CurrentCharges,"type"+abilityUseInfo.PressState);
			if (abilityUseInfo.PressState != InputPressState.JustPressed)
				return;
			var slot_range = abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(Stat.Range);
			float range=slot_range!=null? BlastRange.GetCombinedTotal(slot_range):BlastRange.CalculateTotal();
			float damage = Damage.GetCombinedTotal(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(Stat.Damage));
			GD.Print(range, damage," to"+master.Body.AimDirection);
			List<BaseCharacterBody> charactersInBlast=AttackHelper.GetCharactersInShotgunBlast(master.Body, master.Body.GetGlobalTransform(), master.Body.AimDirection.Angle(), BlastWidth, range, 8);
			foreach(BaseCharacterBody characterBody in charactersInBlast)
			{
				GD.Print(characterBody.CharacterName);
				if (true)//master.CanDamageTeams.Contains(characterBody.CharacterMaster.Team))
				{
					DamageInfo info = new DamageInfo(master,characterBody.CharacterMaster,(1,this.Index,0),damage,false);
					master.DealDamage(characterBody.CharacterMaster, info,null);
				}
			}

			abilitySlot.CCooldown.TryUseCharge();
			abilitySlot.CCooldown.Reset();
			GD.Print(abilitySlot.CurrentCharges, " " + abilitySlot.IsUsable, " " + abilitySlot.CCooldown.Duration, " " + abilitySlot.CCooldown.IsPaused);
		}

	}

	public class ScoutGrappleHook : Ability<ScoutGrappleHook>
	{
        protected static RangeStat Range = new RangeStat(1000f, 3000f, 500f);
        protected static DamageStat Damage = new DamageStat(0, 0f);
        protected static CooldownStat Cooldown = new CooldownStat(8f);
        protected static CooldownStat SpamCooldown = new CooldownStat(2f);
        protected static ChargeStat Charges = new ChargeStat(2f);

        public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>()
            {
                { Stat.Range, Range },
                { Stat.Damage, Damage },
                { Stat.Cooldown, Cooldown },
                { Stat.Charges, Charges },
                {Stat.SpamCooldown, SpamCooldown },
            },
           new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>())
        {
        };
        public override string Name { get; protected set; } = "Grapple Hook";
        public override string CodeName { get; protected set; } = "Scout_Grapple";
        public override string Description { get; protected set; }
        public override string AdvancedDescription { get; protected set; }
        public override Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/Phase_Blast.png");

        //public AbilityUpgrade SSG_U1_Cooldown=
        public override BaseCharacterBody UpdateTarget()
        {
            return null;
        }
    }

	public partial class GrappleProj : BaseProjectile
	{

	}

	public partial class ReelProj:BaseProjectile
	{

	}

}
