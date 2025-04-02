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
        public static PackedScene Prefab = (PackedScene)ResourceLoader.Load("res://path_to_prefab.tscn");
    }

    public class SwarmerSmash : Ability<SwarmerSmash>
    {
        public override StatsByCritera<AbilityUpgrade> Stats { get; set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStats.ModifiableStat>
        {
            {Stat.Damage, new DamageStat(15f, 1.2f)},
            {Stat.Cooldown, new CooldownStat(2f) },
            {Stat.FireRate, new FireRateStat(1f)},
            {Stat.Range, new RangeStat(1,3,.5f) }
        },null);
        //private float BlastWidth = 15;
        //protected RangeStat BlastRange = new RangeStat(1.2f, 8f, .5f);
        //protected DamageStat Damage = new DamageStat(30, .9f);
        //protected CooldownStat Cooldown = new CooldownStat(14f);
        //protected CooldownStat SpamCooldown = new CooldownStat(.1f);
        //protected ChargeStat Charges = new ChargeStat(2f);

        //public AbilityUpgrade SSG_U1_Cooldown=
        public override BaseCharacterBody UpdateTarget()
        {
            
        }

        public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUpgradeTree treeProgress, BaseCharacterBody target, EventChain eventChain = default)
        {
            //an attack that takes 1/FireRate second to wind up (Add a cooldown to the character) and start the attack animation fot the character
            // Once that cooldown is completed, create a Collison box/capsule (with width X(left-right) and length(Front-back) of Y) directly infront of the front facing direction of the character master.
            // Get character masters of ALL characters within the box.
            // Damage all characters within the teams of master.CanDamageTeams

            //Before Attack Stuff
            // Get the attack wind-up time based on FireRate
            float windUpTime = 1f / master.Stats.GetStatByVariable<FireRateStat>(Stat.FireRate)?.CalculateTotal() ?? 1f;
            StatsHolder stats = Stats.CopyAndGetStatsByCriterea(
                        treeProgress.IsUpgraded
                            .Where(kvp => kvp.Value) // Filter for true values
                            .Select(kvp => kvp.Key) // Select only the keys
                            .ToHashSet() // Convert to HashSet<T>
                        );
            stats=stats.CopyAndAddAllStats(master.Stats,abilitySlot.ThisAbilityStats);

            AttackInfo attackInfo = new AttackInfo(master, null, (1, instance.Index, 0), false, stats);


            master.BeforeAttack(attackInfo, eventChain);

            // Add a cooldown equal to the wind-up time before executing the attack
            master.Cooldowns.AddCooldown((1, instance.Index, 10), windUpTime, true, (cooldown) =>
            {
                // Get attack dimensions (Width X, Length Y)
                float characterWidth = master.CharacterBody.HitboxWidth;
                float width = characterWidth * 0.75f; // Set width to 75% of character's hitbox width
                float length = stats.GetStatByVariable<RangeStat>(Stat.Range)?.CalculateTotal() ?? 1f;

                // Define the attack origin and direction
                Vector2 attackDirection = master.CharacterBody.ForwardDirection.Normalized(); // Ensure it's normalized
                Vector2 attackOrigin = master.CharacterBody.Position + attackDirection * (characterWidth / 2); // Adjust origin to front

                // Calculate the attack box/capsule center
                Vector2 attackCenter = attackOrigin + attackDirection * (length / 2); // Center it in front of the character

                // Get all character masters within the area
                List<CharacterMaster> hitTargets = GetCharactersInBox(attackCenter, width, length);

                // Damage all valid targets
                foreach (var hitTarget in hitTargets)
                {
                    if (master.CanDamageTeams.Contains(hitTarget.Team))
                    {
                        float damage = stats.GetStatByVariable<DamageStat>(Stat.Damage)?.CalculateTotal() ?? 10f;
                        bool isCrit = stats.GetStatByVariable<ChanceStat>(Stat.CritChance)?.Roll() ?? false;

                        DamageInfo damageInfo = new DamageInfo(master, hitTarget, (1, instance.Index, 0), damage, isCrit);
                        master.DealDamage(hitTarget, damageInfo, eventChain);
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
