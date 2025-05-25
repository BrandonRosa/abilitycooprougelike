using BrannPack.AbilityHandling;
using BrannPack.Helpers.Attacks;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;


namespace BrannPack.Character.NonPlayable
{


    public static class PC_Scout
    {
        //public static PackedScene Prefab = (PackedScene)ResourceLoader.Load("res://path_to_prefab.tscn");
    }

    public class SwarmerSmash : Ability<SwarmerSmash>
    {
        public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStats.ModifiableStat>
        {
            {Stat.Damage, new DamageStat(15f, 1.2f)},
            {Stat.Cooldown, new CooldownStat(2f) },
            {Stat.FireRate, new FireRateStat(1f)},
            {Stat.Range, new RangeStat(1,3,.5f) }
        },null);
        public override string Name { get; protected set; } = "Swipe";
        public override string CodeName { get; protected set; } = "MELEE_SWARMER_SWIPE";
        public override string Description { get; protected set; } = "Swipe Nearby Enemies";
        public override string AdvancedDescription { get; protected set; }

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
            //an attack that takes 1/FireRate second to wind up (Add a cooldown to the character) and start the attack animation fot the character
            // Once that cooldown is completed, create a Collison box/capsule (with width X(left-right) and length(Front-back) of Y) directly infront of the front facing direction of the character master.
            // Get character masters of ALL characters within the box.
            // Damage all characters within the teams of master.CanDamageTeams

            //Before Attack Stuff
            // Get the attack wind-up time based on FireRate
            float windUpTime = 1f / master.Stats.GetStatByVariable<FireRateStat>(Stat.FireRate)?.CalculateTotal() ?? 1f;
            StatsHolder stats = Stats.CopyAndGetStatsByCriterea(
                        abilitySlot.CurrentUpgrades
                        );
            stats=stats.CopyAndAddAllStats(master.Stats,abilitySlot.ThisAbilityStats);

            AttackInfo attackInfo = new AttackInfo(master, null, (1, instance.Index, 0), false, stats);

            master.BeforeAttack(attackInfo, null);
            float damage = stats.GetStatByVariable<DamageStat>(Stat.Damage)?.CalculateTotal() ?? 10f;
            float critChance = stats.GetStatByVariable<ChanceStat>(Stat.CritChance)?.CalculateTotal() ?? 0f;
            float goodLuck = stats.GetStatByVariable<ChanceStat>(Stat.Luck)?.CalculateTotal() ?? 0f;
            float badLuck = stats.GetStatByVariable<ChanceStat>(Stat.BadLuck)?.CalculateTotal() ?? 0f;
            var rolls = AttackHelper.RollWithProcAndLucks(critChance, 1, goodLuck, badLuck);
            bool isCrit = rolls.IsSuccess;

            attackInfo.IsCrit = isCrit;

            if (!eventChain.TryAddEventInfo(attackInfo))return;

            // Add a cooldown equal to the wind-up time before executing the attack
            master.Cooldowns.AddCooldown((1, instance.Index, 10), windUpTime, true, (cooldown) =>
            {
                // Get the first collision shape
                var shape = master.Body.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
                // Get attack dimensions (Width X, Length Y)
                float characterWidth = 1f;//(shape?.Shape as RectangleShape2D)?.Size.x ?? 0f;
                float width = characterWidth * 0.75f;  // 75% of character width
                float length = stats.GetStatByVariable<RangeStat>(Stat.Range)?.CalculateTotal() ?? 1f;

                // Define attack direction & rotation
                Vector2 attackDirection = master.Body.MoveDirection.Normalized();
                Vector2 attackOrigin = master.Body.Position + attackDirection * (characterWidth / 2);
                Vector2 attackCenter = attackOrigin + attackDirection * (length / 2);

                float rotationAngle = attackDirection.Angle(); // Get attack direction as rotation

                // Get characters in rotated hitbox
                List<BaseCharacterBody> hitTargets = AttackHelper.GetCharactersInRotatedBox(master.Body,attackCenter, width, length, rotationAngle);

                // Damage all valid targets
                foreach (var hitTarget in hitTargets)
                {
                    if (master.CanDamageTeams.Contains(hitTarget.CharacterMaster.Team))
                    {
                        

                        DamageInfo damageInfo = new DamageInfo(master, hitTarget.CharacterMaster, (1, instance.Index, 0), damage, isCrit);
                        master.DealDamage(hitTarget.CharacterMaster, damageInfo, eventChain);
                    }
                }

                // Run post-attack logic
                master.AfterAttack(attackInfo, eventChain);
            });

            // Start the attack animation
            master.Body.AnimSprite.Play("Attack");
        }
    }

}
