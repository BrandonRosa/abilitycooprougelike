﻿using BrannPack.AbilityHandling;
using BrannPack.Helpers.Attacks;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;

namespace BrannPack.Character.Playable
{
    

    public static class PC_Scout
    {
        //public static PackedScene Prefab { get; set; } = (PackedScene)ResourceLoader.Load("res://path_to_prefab.tscn");
    }

    public class ScoutShotGun : Ability<ScoutShotGun>
    {
        private float BlastWidth = 15;
        protected static RangeStat BlastRange = new RangeStat(1.2f, 8f, .5f);
        protected static DamageStat Damage = new DamageStat(30, .9f);
        protected static CooldownStat Cooldown = new CooldownStat(14f);
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

        public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, EventChain eventChain = null)
        {
            GD.Print("SHOTGUN");
            float range=BlastRange.GetCombinedTotal(abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(Stat.Range));
            float damage = Damage.GetCombinedTotal(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(Stat.Damage));
            List<BaseCharacterBody> charactersInBlast=AttackHelper.GetCharactersInShotgunBlast(master.Body, master.Body.GetGlobalTransform(), master.Body.AttackDirection.Angle(), BlastWidth, range, 5);
            foreach(BaseCharacterBody characterBody in charactersInBlast)
            {
                if (master.CanDamageTeams.Contains(characterBody.CharacterMaster.Team))
                {
                    DamageInfo info = new DamageInfo(master,characterBody.CharacterMaster,(1,this.Index,0),damage,false);
                    master.DealDamage(characterBody.CharacterMaster, info,null);
                }
            }
        }

    }

}
