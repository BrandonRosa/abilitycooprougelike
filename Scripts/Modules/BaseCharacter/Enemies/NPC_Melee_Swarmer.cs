using BrannPack.AbilityHandling;
using BrannPack.Helpers.Attacks;
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

        public override void UseAbility(BaseCharacterBody baseCharacter, AbilitySlot abilitySlot, AbilityUpgradeTree treeProgress, BaseCharacterBody target)
        {
            //float range = BlastRange.GetCombinedTotal(abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(BaseCharacterBody.Stat.Range));
            //float damage = Damage.GetCombinedTotal(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(BaseCharacterBody.Stat.Damage));
            //List<BaseCharacterBody> charactersInBlast = AttackHelper.GetCharactersInShotgunBlast(baseCharacter, baseCharacter.GetGlobalTransform(), baseCharacter.AttackDirection.Angle(), BlastWidth, range, 5);
            //foreach (BaseCharacterBody characterBody in charactersInBlast)
            //{
            //    if (baseCharacter.CharacterMaster.CanDamageTeams.Contains(characterBody.Team))
            //    {
            //        DamageInfo info = new DamageInfo();
            //        baseCharacter.CharacterMaster.DealDamage(characterBody, damage);
            //    }
            //}
        }
    }

}
