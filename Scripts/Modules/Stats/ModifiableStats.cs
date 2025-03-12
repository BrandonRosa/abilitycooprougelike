using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.ModifialbeStats
{
    public abstract class ModifiableStat
    {
        public float Total { get; protected set; } = 0f;
        public float BaseValue { get; protected set; } = 0f;

        public ModifiableStat(float baseValue) => BaseValue = baseValue;
        protected abstract float TotalValueMath();

        public abstract void ResetModifiedValues();
        public float CalculateTotal() { return Total = TotalValueMath(); }
    }

    public class FullExpandedStat : ModifiableStat
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

    public static class AbilityStats
    {
        
        

        //Damage is Calculated like this: Total Damage= (BaseDamage+AdditionalDamage*DamageScaling)*(1f+DamagePercentIncrease-DamagePercentDecreases);
        //AdditionalBase
        //AdditionalScaling
        //FlatPercentIncrease
        //ScalingPercentDecrease
        public class DamageStat : ModifiableStat
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

        public class FireRateStat : ModifiableStat
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
        public class ProjectileSpeedStat : ModifiableStat
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
        public class ChanceStat : ModifiableStat
        {
            public float ChanceUpPercentage;
            public float MinimumChance = 0f;

            public ChanceStat(float baseValue, float minimumChance = 0f) : base(baseValue) => ChanceUpPercentage = minimumChance;

            protected override float TotalValueMath() { return Mathf.Max(MinimumChance, BaseValue * (1f + ChanceUpPercentage)); }
            public override void ResetModifiedValues() { ChanceUpPercentage = 0f; }
            public void ChangeProcChanceUpPercentage(float changeValue, bool undo = false) { if (!undo) ChanceUpPercentage += changeValue; else ChanceUpPercentage -= changeValue; }
        }



        //Charges+AdditionalCharges
        public class ChargeStat : ModifiableStat
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

        //CooldownReductionPercentage HAS to be handled CAREFULLY. I want it to range from (0 to inf) but I want the scaling to be slow.
        //So Having to 50% reductions wont make it 100% itl be 50%*50%=25%.. So any modifications to CooldownReductionPercentage MUST be multiplied

        //CooldownIncreased Is different though. Instead well do it like this: 50%+50%=100%
        public class CooldownStat : ModifiableStat
        {
            public float CooldownScaling;
            public float MinimumCooldown = 0f;

            public float AdditionalCooldown = 0f;
            public float CooldownPercentIncrease = 0f;
            public List<float> CooldownPercentDecreases = new List<float>();

            public CooldownStat(float baseValue, float CooldownScaling, float minimumCooldown = 0f) : base(baseValue) => (CooldownScaling, MinimumCooldown) = (CooldownScaling, minimumCooldown);

            protected override float TotalValueMath() { return Mathf.Max(MinimumCooldown, BaseValue + AdditionalCooldown * CooldownScaling) * (1f + CooldownPercentIncrease - (1f - CooldownPercentDecreases.Aggregate(1f, (total, next) => total * next))); }

            public override void ResetModifiedValues() { AdditionalCooldown = 0f; CooldownPercentIncrease = 0f; CooldownPercentDecreases.Clear(); }
            public void ChangeAdditionalCooldown(float changeValue, bool undo = false) { if (!undo) AdditionalCooldown += changeValue; else AdditionalCooldown -= changeValue; }
            public void ChangeCooldownByPercent(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) CooldownPercentIncrease += percentChange;
                    else CooldownPercentDecreases.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) CooldownPercentIncrease -= percentChange;
                    else CooldownPercentDecreases.Remove(1f - percentChange);
                }
            }

        }


        //SpamCooldown*(1f-SpamCooldwonIncreasedPercentageSpamCooldownReductionPercentage);


        //Range*(1f+RangeFlatPercentage-RangeDownPercentage);
        /* SOFT CAP FORMULA/ DIMINISHES WAY TOO FAST
        //SLOWS DOWN LIKE CRAZY AFTER REACHING MAX/2
        public class RangeStat : ModifiableStat
        {
            public float RangeMinimum = 1f;
            public float SoftMax;

            public float RangeFlatPercentage = 0f;
            public List<float> RangeDownPercentages = new List<float>();

            //Reduction Calculation has to be done carfully. a 25% reduction and a 45% reduction would end up being 58.75% (41.25% of the original) 
            public RangeStat(float baseValue, float softMax, float rangeMinimum = 1f) : base(baseValue) => (RangeMinimum,SoftMax) = (rangeMinimum,softMax);
            protected override float TotalValueMath() 
            {
                float totalUnmodChange= BaseValue*(RangeFlatPercentage - (1f - RangeDownPercentages.Aggregate(1f, (total, next) => total * next)));
                return Mathf.Max(RangeMinimum, (BaseValue + (totalUnmodChange*(SoftMax-BaseValue)/(totalUnmodChange+(SoftMax-BaseValue))))); 
            }

            public override void ResetModifiedValues() { RangeFlatPercentage = 0f; RangeDownPercentages.Clear(); }
            public void ChangeRangePercentage(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) RangeFlatPercentage += percentChange;
                    else RangeDownPercentages.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) RangeFlatPercentage -= percentChange;
                    else RangeDownPercentages.Remove(1f - percentChange);
                }
            }
        }
        */
        public class RangeStat : ModifiableStat
        {
            public float RangeMinimum = 1f;
            public float SoftMax;
            public float ScaleConstant = 1.8f;

            public float RangeFlatPercentage = 0f;
            public List<float> RangeDownPercentages = new List<float>();

            //Reduction Calculation has to be done carfully. a 25% reduction and a 45% reduction would end up being 58.75% (41.25% of the original) 
            public RangeStat(float baseValue, float softMax, float rangeMinimum = 1f, float scaleConstant=1.8f) : base(baseValue) => (RangeMinimum, SoftMax,ScaleConstant) = (rangeMinimum, softMax,scaleConstant);
            protected override float TotalValueMath()
            {
                float totalUnmodChange = BaseValue * (RangeFlatPercentage - (1f - RangeDownPercentages.Aggregate(1f, (total, next) => total * next)));
                return Mathf.Max(RangeMinimum, SoftMax-(SoftMax - BaseValue)*Mathf.Exp(-ScaleConstant*totalUnmodChange/SoftMax));
            }

            public override void ResetModifiedValues() { RangeFlatPercentage = 0f; RangeDownPercentages.Clear(); }
            public void ChangeRangePercentage(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) RangeFlatPercentage += percentChange;
                    else RangeDownPercentages.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) RangeFlatPercentage -= percentChange;
                    else RangeDownPercentages.Remove(1f - percentChange);
                }
            }
        }

        //Duration*(1f+DurationUpPercentage);
        public class DurationStat : ModifiableStat
        {
            public float DurationMinimum = 1f;
            public float SoftMax;
            public float ScaleConstant = 1.5f;

            public float DurationFlatPercentage = 0f;
            public List<float> DurationDownPercentages = new List<float>();

            //Reduction Calculation has to be done carfully. a 25% reduction and a 45% reduction would end up being 58.75% (41.25% of the original) 
            public DurationStat(float baseValue, float softMax, float durationMinimum = 1f, float scaleConstant = 1.5f) : base(baseValue) => (DurationMinimum, SoftMax, ScaleConstant) = (durationMinimum, softMax, scaleConstant);
            protected override float TotalValueMath()
            {
                float totalUnmodChange = BaseValue * (DurationFlatPercentage - (1f - DurationDownPercentages.Aggregate(1f, (total, next) => total * next)));
                return Mathf.Max(DurationMinimum, SoftMax - (SoftMax - BaseValue) * Mathf.Exp(-ScaleConstant * totalUnmodChange / SoftMax));
            }

            public override void ResetModifiedValues() { DurationFlatPercentage = 0f; DurationDownPercentages.Clear(); }
            public void ChangeDurationPercentage(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) DurationFlatPercentage += percentChange;
                    else DurationDownPercentages.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) DurationFlatPercentage -= percentChange;
                    else DurationDownPercentages.Remove(1f - percentChange);
                }
            }
        }

    }

    public static class CharacterStats
    {
        public class MaxHealthStat : ModifiableStat
        {
            public float MaxHealthScaling;
            public float MinimumMaxHealth = 0f;

            public float AdditionalMaxHealth = 0f;
            public float MaxHealthPercentIncrease = 0f;
            public List<float> MaxHealthPercentDecreases = new List<float>();

            public MaxHealthStat(float baseValue, float MaxHealthScaling, float minimumMaxHealth = 0f) : base(baseValue) => (MaxHealthScaling, MinimumMaxHealth) = (MaxHealthScaling, minimumMaxHealth);

            protected override float TotalValueMath() { return Mathf.Max(MinimumMaxHealth, BaseValue + AdditionalMaxHealth * MaxHealthScaling) * (1f + MaxHealthPercentIncrease - (1f - MaxHealthPercentDecreases.Aggregate(1f, (total, next) => total * next))); }

            public override void ResetModifiedValues() { AdditionalMaxHealth = 0f; MaxHealthPercentIncrease = 0f; MaxHealthPercentDecreases.Clear(); }
            public void ChangeAdditionalMaxHealth(float changeValue, bool undo = false) { if (!undo) AdditionalMaxHealth += changeValue; else AdditionalMaxHealth -= changeValue; }
            public void ChangeMaxHealthByPercent(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) MaxHealthPercentIncrease += percentChange;
                    else MaxHealthPercentDecreases.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) MaxHealthPercentIncrease -= percentChange;
                    else MaxHealthPercentDecreases.Remove(1f - percentChange);
                }
            }

        }
    }
}
