using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrannPack.Ability
{
    public class Ability
    {
        private float BaseCooldown;
        private float CurrentCooldown;
        private float NoSpamCooldown;
        private float MaxCharges;
        private float CurrentCharges;

        private bool CanCharge;
        private bool IsInfiniteUse;
        private bool IsMultiPrompt;

        private AbilityUpgradeTree UpgradeTree;

        //private image ArtWork;
        private static string Description;
        private static string AdvancedDescription;
        private static string Name;
        private static List<AbilityUpgrade> AbilityUpgrades;



    }

    public class AbilityUpgradeTree
    {
        private Dictionary<AbilityUpgrade, bool> IsUpgraded;
    }

    public class AbilityUpgrade
    {
        private List<AbilityUpgrade> Upgrades;
        private AbilityUpgrade Previous;
        private String Name;
        private String Description;
        private String AdvancedDescription;
        private int APCost;
        private int LockCost;
    }

    public static class AbilityStats
    {
        public abstract class AbilityStat
        {
            public float Total { get; protected set; } = 0f;
            public float BaseValue { get; protected set; } = 0f;

            public AbilityStat(float baseValue) => BaseValue = baseValue;
            protected abstract float TotalValueMath();

            public abstract void ResetModifiedValues();
            public float CalculateTotal() { return Total = TotalValueMath(); }
        }

        public class FullExpandedStat : AbilityStat
        {
            public float AdditionalScaling;
            public float? MinimumValue;
            public float? MaximumValue;

            public float AdditionalBaseValue;

            public List<float> MultipliedPercentageIncreases = new List<float>();
            public List<float> MultipliedPercentageDecreases = new List<float>();
            public float AdditivePercentageChange = 0f;

            public float AdditiveFlatNonScaledChange = 0f;

            public FullExpandedStat(float baseValue, float additionalScaling = 1f, float? minimumValue = null, float? maximumValue = null) : base(baseValue) => (AdditionalScaling, MinimumValue, MaximumValue) = (additionalScaling, minimumValue, maximumValue);

            protected override float TotalValueMath()
            {
                float tempTotal = (BaseValue + AdditionalBaseValue * AdditionalScaling) * (1f + AdditivePercentageChange + (1f - MultipliedPercentageIncreases.Aggregate(1f, (total, next) => total * next)) - (1f - MultipliedPercentageDecreases.Aggregate(1f, (total, next) => total * next))) + AdditiveFlatNonScaledChange;
                float tempMin = MinimumValue ?? tempTotal;
                float tempMax = MaximumValue ?? tempTotal;
                return
                    Mathf.Clamp(tempTotal, tempMin, tempMax);
            }

            public override void ResetModifiedValues() { AdditionalBaseValue = 0f; AdditivePercentageChange = 0f; MultipliedPercentageIncreases.Clear(); MultipliedPercentageDecreases.Clear(); AdditiveFlatNonScaledChange = 0f; }
            public void ChangeAdditionalBaseValue(float changeValue, bool undo = false) { if (!undo) AdditionalBaseValue += changeValue; else AdditionalBaseValue -= changeValue; }

            public void ChangeMultipliedPercentage(float changeValue, bool undo = false)
            {
                if (changeValue == 0)
                    return;
                if (changeValue > 0)
                {
                    if (!undo) MultipliedPercentageIncreases.Add(1f - changeValue); else MultipliedPercentageIncreases.Remove(1f - changeValue);
                }
                else
                {
                    if (!undo) MultipliedPercentageDecreases.Add(1f - changeValue); else MultipliedPercentageDecreases.Remove(1f - changeValue);
                }
            }

            public void ChangeAdditivePercentage(float changeValue, bool undo = false) { if (!undo) AdditivePercentageChange += changeValue; else AdditivePercentageChange -= changeValue; }

            public void ChangeFlatNonScaled(float changeValue, bool undo = false) { if (!undo) AdditiveFlatNonScaledChange += changeValue; else AdditiveFlatNonScaledChange -= changeValue; }
        }

        //Damage is Calculated like this: Total Damage= (BaseDamage+AdditionalDamage*DamageScaling)*(1f+DamagePercentIncrease-DamagePercentDecreases);
        //AdditionalBase
        //AdditionalScaling
        //FlatPercentIncrease
        //ScalingPercentDecrease
        public class DamageStat : AbilityStat
        {
            public float DamageScaling;
            public float MinimumDamage = 0f;

            public float AdditionalDamage = 0f;
            public float DamagePercentIncrease = 0f;
            public List<float> DamagePercentDecreases = new List<float>();

            public DamageStat(float baseValue, float damageScaling, float minimumDamage = 0f) : base(baseValue) => (DamageScaling, MinimumDamage) = (damageScaling, minimumDamage);

            protected override float TotalValueMath() { return Mathf.Max(MinimumDamage, BaseValue + AdditionalDamage * DamageScaling) * (1f + DamagePercentIncrease - (1f - DamagePercentDecreases.Aggregate(1f, (total, next) => total * next))); }

            public override void ResetModifiedValues() { AdditionalDamage = 0f; DamagePercentIncrease = 0f; DamagePercentDecreases.Clear(); }
            public void ChangeAdditionalDamage(float changeValue, bool undo = false) { if (!undo) AdditionalDamage += changeValue; else AdditionalDamage -= changeValue; }
            public void ChangeDamageByPercent(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) DamagePercentIncrease += percentChange;
                    else DamagePercentDecreases.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) DamagePercentIncrease -= percentChange;
                    else DamagePercentDecreases.Remove(1f - percentChange);
                }
            }

        }
        // FireRate*(1f+FireRateUpPercentage)

        public class FireRateStat : AbilityStat
        {
            public float FireRateMinimum = 1f;

            public float FireRateUpPercentage = 0f;
            public List<float> FireRateDownPercentages = new List<float>();

            //Reduction Calculation has to be done carfully. a 25% reduction and a 45% reduction would end up being 58.75% (41.25% of the original) 
            public FireRateStat(float baseValue, float fireRateMinimum = 1f) : base(baseValue) => FireRateMinimum = fireRateMinimum;
            protected override float TotalValueMath() { return Mathf.Max(FireRateMinimum, (BaseValue * (1f + FireRateUpPercentage - (1f - FireRateDownPercentages.Aggregate(1f, (total, next) => total * next))))); }

            public override void ResetModifiedValues() { FireRateUpPercentage = 0f; }
            public void ChangeFireRatePercentage(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) FireRateUpPercentage += percentChange;
                    else FireRateDownPercentages.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) FireRateUpPercentage -= percentChange;
                    else FireRateDownPercentages.Remove(1f - percentChange);
                }
            }
        }
        //ProjectileSpeed*(1f+ProjectileSpeedUpPercentage);
        //FlatPercentage
        //ScalingDownPercentage
        //Min
        //Max?
        public class ProjectileSpeedStat : AbilityStat
        {
            public float ProjectileSpeedUpPercentage;
            public List<float> ProjectileSpeedDownPercentage = new List<float>();
            public float ProjectileSpeedMinumum = 0f;
            public float? ProjectileSpeedMaximum;

            public ProjectileSpeedStat(float baseValue, float projectileSpeedMinumum = 0f, float? projectileSpeedMaximum = null) : base(baseValue) => (ProjectileSpeedMinumum, ProjectileSpeedMaximum) = (projectileSpeedMinumum, projectileSpeedMaximum);
            protected override float TotalValueMath()
            {
                float tempTotal = (BaseValue) * (1f + ProjectileSpeedUpPercentage - (1f - ProjectileSpeedDownPercentage.Aggregate(1f, (total, next) => total * next)));
                float tempMax = ProjectileSpeedMaximum ?? tempTotal;
                return
                    Mathf.Clamp(tempTotal, ProjectileSpeedMinumum, tempMax);
            }

            public override void ResetModifiedValues() { ProjectileSpeedUpPercentage = 0f; ProjectileSpeedDownPercentage.Clear(); }

            public void ChangeMultipliedPercentage(float changeValue, bool undo = false)
            {
                if (changeValue == 0)
                    return;
                if (changeValue > 0)
                {
                    if (!undo) ProjectileSpeedUpPercentage += changeValue; else ProjectileSpeedUpPercentage -= changeValue;
                }
                else
                {
                    if (!undo) ProjectileSpeedDownPercentage.Add(1f - changeValue); else ProjectileSpeedDownPercentage.Remove(1f - changeValue);
                }
            }
        }

        //ProcChance*(1f+ChanceUpPercentage);
        //FlatPercentage
        //Minimum
        public class ChanceStat : AbilityStat
        {
            public float ChanceUpPercentage;
            public float MinimumChance = 0f;

            public ChanceStat(float baseValue, float minimumChance = 0f) : base(baseValue) => ChanceUpPercentage = minimumChance;

            protected override float TotalValueMath() { return Mathf.Max(MinimumChance, BaseValue * (1f + ChanceUpPercentage)); }
            public override void ResetModifiedValues() { ChanceUpPercentage = 0f; }
            public void ChangeProcChanceUpPercentage(float changeValue, bool undo = false) { if (!undo) ChanceUpPercentage += changeValue; else ChanceUpPercentage -= changeValue; }
        }



        //Charges+AdditionalCharges
        public class ChargeStat : AbilityStat
        {
            public float ChargeScaling;
            public float MinimumCharges = 1f;

            public float AdditionalCharges = 0f;
            public float ChargePercentFlat = 0f;
            //public List<float> ChargePercentDecreases = new List<float>();

            public ChargeStat(float baseValue, float chargeScaling = 1f, float minimumCharges = 1f) : base(baseValue) => (ChargeScaling, MinimumCharges) = (chargeScaling, minimumCharges);

            protected override float TotalValueMath() { return Mathf.Max(MinimumCharges, BaseValue + AdditionalCharges * ChargeScaling) * (1f + ChargePercentFlat); }

            public override void ResetModifiedValues() { AdditionalCharges = 0f; ChargePercentFlat = 0f; }
            public void ChangeAdditionalCharges(float changeValue, bool undo = false) { if (!undo) AdditionalCharges += changeValue; else AdditionalCharges -= changeValue; }
            public void ChangeChargesByFlatPercent(float changeValue, bool undo = false) { if (!undo) AdditionalCharges += changeValue; else AdditionalCharges -= changeValue; }

        }

        //Cooldown*(1f+CooldownIncreasedPercentage-CooldownReductionPercentage);
        public float Cooldown;
        //CooldownReductionPercentage HAS to be handled CAREFULLY. I want it to range from (0 to inf) but I want the scaling to be slow.
        //So Having to 50% reductions wont make it 100% itl be 50%*50%=25%.. So any modifications to CooldownReductionPercentage MUST be multiplied
        public float CooldownReductionPercentage;
        //CooldownIncreased Is different though. Instead well do it like this: 50%+50%=100%
        public float CooldownIncreasedPercentage;

        //SpamCooldown*(1f-SpamCooldwonIncreasedPercentageSpamCooldownReductionPercentage);
        public float SpamCooldown;
        public float SpamCooldownReductionPercentage;//Multiply to this one
        public float SpmCooldownIncreasedPercentage; //Add to this one

        //Range*(1f+RangeUpPercentage-RangeDownPercentage);
        public float Range;
        public float RangeUpPercentage; //Add to this one
        public float RangeDownPercentage; //Multiply to this one

        //Duration*(1f+DurationUpPercentage);
        public float Duration;
        public float DurationUpPercentage; //Add to this one
        public float DurationDownPercentage; //Multiply to this one

    }

    public enum AbilitySlot
    {
        Primary,Secondary,Utility,Special,Ult
    }
}
