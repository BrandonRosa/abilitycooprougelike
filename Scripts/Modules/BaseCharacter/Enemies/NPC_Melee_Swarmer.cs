using BrannPack.AbilityHandling;
using BrannPack.CooldownHandling;
using BrannPack.Helpers.Attacks;
using BrannPack.InputHelpers;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;
using static System.Net.Mime.MediaTypeNames;


namespace BrannPack.Character.NonPlayable
{


	public static class PC_Scout
	{
		//public static PackedScene Prefab = (PackedScene)ResourceLoader.Load("res://path_to_prefab.tscn");
	}

	public class SwarmerSmash : Ability<SwarmerSmash>
	{
		public static DamageStat Damage = new DamageStat(15f, 1.2f);

		public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStats.ModifiableStat>
		{
			{Stat.Damage, Damage},
			{Stat.FireRate, new FireRateStat(.83f)},
			{Stat.Range, new RangeStat(1,3,.5f) }
		}, new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>())
		{ };
		public override string Name { get; protected set; } = "Swipe";
		public override string CodeName { get; protected set; } = "MELEE_SWARMER_SWIPE";
		public override string Description { get; protected set; } = "Swipe Nearby Enemies";
		public override string AdvancedDescription { get; protected set; }

		public override AIAbilityHint AbilityHint { get; set; } = new AIAbilityHint { RequiresLOS = true,RangeUseMultiplierBounds=(0,1f), ContinueWindupIfTargetLost=true };

		//private float BlastWidth = 15;
		//protected RangeStat BlastRange = new RangeStat(1.2f, 8f, .5f);
		//protected DamageStat Damage = new DamageStat(30, .9f);
		//protected CooldownStat Cooldown = new CooldownStat(14f);
		//protected CooldownStat SpamCooldown = new CooldownStat(.1f);
		//protected ChargeStat Charges = new ChargeStat(2f);

		//public AbilityUpgrade SSG_U1_Cooldown=
		public override BaseCharacterBody UpdateTarget()
		{
			return null;
		}

		public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo=null, EventChain eventChain=null)
		{
			var windup = abilitySlot.Windup;
			if (windup.IsPaused)
				windup.IsPaused = false;
			if (abilitySlot.WindupAttackDirection == null)
				if (abilityUseInfo.DirectionTo != null)
					abilitySlot.WindupAttackDirection = abilityUseInfo.DirectionTo;
				else
					abilitySlot.WindupAttackDirection = master.Body.AimDirection;

			if (((Windup)windup).IsWindupComplete)
			{
				float damage= Damage.GetCombinedTotal(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(Stat.Damage));
				DoAttack(master,damage);
				abilitySlot.WindupAttackDirection = abilityUseInfo.DirectionTo;
				windup.Reset();
			}

		}

		private void DoAttack(CharacterMaster master,float damage)
		{
			float size = 50f;
			var trans = master.Body.GetGlobalTransform();
			trans.Origin = trans.Origin + master.Body.AimDirection.Normalized() * 20f;
			var list = AttackHelper.GetCollisionsInBoxArea(trans, master.Body.AimDirection.Angle(), size, size, master.Body.GetWorld2D().DirectSpaceState, true);
			foreach (BaseCharacterBody characterBody in list)
			{
				if (master.CanDamageTeams.Contains(characterBody.CharacterMaster.Team))
				{
					DamageInfo info = new DamageInfo(master, characterBody.CharacterMaster, (1, this.Index, 0), damage, false);
					master.DealDamage(characterBody.CharacterMaster, info, null);
				}
			}
		}

		
	}

}
