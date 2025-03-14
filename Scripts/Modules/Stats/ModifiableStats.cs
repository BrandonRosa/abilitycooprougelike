using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.Character.BaseCharacter;
using static BrannPack.ModifialbeStats.AbilityStats;
using static System.Net.Mime.MediaTypeNames;

namespace BrannPack.ModifialbeStats
{
    public abstract class ModifiableStat<T> where T:ModifiableStat<T>
    {
        public float Total { get; protected set; } = 0f;
        public float BaseValue { get; protected set; } = 0f;

        public event Action<float,float> ChangedTotal;

        public ModifiableStat(float baseValue) => BaseValue = baseValue;
        protected abstract float TotalValueMath();

        public abstract void ResetModifiedValues();

        public float CalculateTotal() 
        {
            float prevTotal = Total;
            Total = TotalValueMath();
            if (prevTotal != Total)
                ChangedTotal?.Invoke(Total, prevTotal);
            return Total;
        }

        public abstract T GetCombinedStat(T[] addStat);

        public float GetCombinedTotal(T[] addStat){return GetCombinedStat(addStat).CalculateTotal();}

        public abstract T Copy();

        public static T GetCombinedStat(T baseStat,T[] addStat)
        {
            return baseStat.GetCombinedStat(addStat);
        }
    }

    public class FullExpandedStat : ModifiableStat<FullExpandedStat>
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


        public override FullExpandedStat GetCombinedStat(FullExpandedStat[] addStat)
        {

            FullExpandedStat tempCombined = this.Copy();

            foreach (FullExpandedStat stat in addStat)
            {
                tempCombined.AdditionalBaseValue += stat.AdditionalBaseValue;

                tempCombined.MultipliedPercentageIncreases.AddRange(stat.MultipliedPercentageIncreases);
                tempCombined.MultipliedPercentageDecreases.AddRange(stat.MultipliedPercentageDecreases);
                tempCombined.AdditivePercentageChange += stat.AdditivePercentageChange;

                tempCombined.AdditiveFlatNonScaledChange += stat.AdditiveFlatNonScaledChange;

            }

            return tempCombined;

        }

        public override FullExpandedStat Copy()
        {
            FullExpandedStat newCombinedStat = new FullExpandedStat(BaseValue, AdditionalScaling, MinimumValue, MaximumValue);
            newCombinedStat.AdditionalBaseValue = AdditionalBaseValue;
            newCombinedStat.MultipliedPercentageIncreases = new List<float>(MultipliedPercentageDecreases);
            newCombinedStat.MultipliedPercentageDecreases = new List<float>(MultipliedPercentageDecreases);
            newCombinedStat.AdditivePercentageChange = AdditivePercentageChange;
            newCombinedStat.AdditiveFlatNonScaledChange = AdditiveFlatNonScaledChange;
            return newCombinedStat;
        }
    }

    public class EffectivenessStat: ModifiableStat<EffectivenessStat>
    {
        public float? MinimumValue;
        public float? MaximumValue;

        public List<float> MultipliedPercentageIncreases = new List<float>();
        public List<float> MultipliedPercentageDecreases = new List<float>();
        public float AdditivePercentageChange = 0f;


        public EffectivenessStat(float baseValue=1f, float? minimumValue = null, float? maximumValue = null) : base(baseValue) => ( MinimumValue, MaximumValue) = ( minimumValue, maximumValue);

        protected override float TotalValueMath()
        {
            float tempTotal = BaseValue+(AdditivePercentageChange + (1f - MultipliedPercentageIncreases.Aggregate(1f, (total, next) => total * next)) - (1f - MultipliedPercentageDecreases.Aggregate(1f, (total, next) => total * next))) ;
            float tempMin = MinimumValue ?? tempTotal;
            float tempMax = MaximumValue ?? tempTotal;
            return
                Mathf.Clamp(tempTotal, tempMin, tempMax);
        }

        public override void ResetModifiedValues() { AdditivePercentageChange = 0f; MultipliedPercentageIncreases.Clear(); MultipliedPercentageDecreases.Clear(); }
        

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

        public override EffectivenessStat GetCombinedStat(EffectivenessStat[] addStat)
        {

            EffectivenessStat tempCombined = this.Copy();

            foreach (EffectivenessStat stat in addStat)
            {

                tempCombined.MultipliedPercentageIncreases.AddRange(stat.MultipliedPercentageIncreases);
                tempCombined.MultipliedPercentageDecreases.AddRange(stat.MultipliedPercentageDecreases);
                tempCombined.AdditivePercentageChange += stat.AdditivePercentageChange;

            }

            return tempCombined;

        }

        public override EffectivenessStat Copy()
        {
            EffectivenessStat newCombinedStat = new EffectivenessStat(BaseValue, MinimumValue, MaximumValue);
            newCombinedStat.MultipliedPercentageIncreases = new List<float>(MultipliedPercentageDecreases);
            newCombinedStat.MultipliedPercentageDecreases = new List<float>(MultipliedPercentageDecreases);
            newCombinedStat.AdditivePercentageChange = AdditivePercentageChange;
            return newCombinedStat;
        }

    }

    public static class AbilityStats
    {
        
        

        //Damage is Calculated like this: Total Damage= (BaseDamage+AdditionalDamage*DamageScaling)*(1f+DamagePercentIncrease-DamagePercentDecreases);
        //AdditionalBase
        //AdditionalScaling
        //FlatPercentIncrease
        //ScalingPercentDecrease
        public class DamageStat : ModifiableStat<DamageStat>
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

            public override DamageStat GetCombinedStat(DamageStat[] addStat)
            {

                DamageStat tempCombined = this.Copy();

                foreach (DamageStat stat in addStat)
                {
                    tempCombined.AdditionalDamage += stat.AdditionalDamage;

                    tempCombined.DamagePercentDecreases.AddRange(stat.DamagePercentDecreases);
                    tempCombined.DamagePercentIncrease += stat.DamagePercentIncrease;

                }

                return tempCombined;

            }

            public override DamageStat Copy()
            {
                DamageStat newCombinedStat = new DamageStat(BaseValue, DamageScaling, MinimumDamage);
                newCombinedStat.AdditionalDamage = AdditionalDamage;
                
                newCombinedStat.DamagePercentDecreases = new List<float>(DamagePercentDecreases);
                newCombinedStat.DamagePercentIncrease = DamagePercentIncrease;
                
                return newCombinedStat;
            }

        }
        // FireRate*(1f+FireRateUpPercentage)

        public class FireRateStat : ModifiableStat<FireRateStat>
        {
            public float FireRateMinimum = 1f;

            public float FireRateUpPercentage = 0f;
            public List<float> FireRateDownPercentages = new List<float>();

            //Reduction Calculation has to be done carfully. a 25% reduction and a 45% reduction would end up being 58.75% (41.25% of the original) 
            public FireRateStat(float baseValue, float fireRateMinimum = 1f) : base(baseValue) => FireRateMinimum = fireRateMinimum;
            protected override float TotalValueMath() { return Mathf.Max(FireRateMinimum, (BaseValue * (1f + FireRateUpPercentage - (1f - FireRateDownPercentages.Aggregate(1f, (total, next) => total * next))))); }

            public override void ResetModifiedValues() { FireRateUpPercentage = 0f; FireRateDownPercentages.Clear(); }
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

            public override FireRateStat GetCombinedStat(FireRateStat[] addStat)
            {

                FireRateStat tempCombined = this.Copy();

                foreach (FireRateStat stat in addStat)
                {

                    tempCombined.FireRateDownPercentages.AddRange(stat.FireRateDownPercentages);
                    tempCombined.FireRateUpPercentage += stat.FireRateUpPercentage;

                }

                return tempCombined;

            }

            public override FireRateStat Copy()
            {
                FireRateStat newCombinedStat = new FireRateStat(BaseValue, FireRateScaling, MinimumFireRate);
                newCombinedStat.AdditionalFireRate = AdditionalFireRate;

                newCombinedStat.FireRatePercentDecreases = new List<float>(FireRatePercentDecreases);
                newCombinedStat.FireRatePercentIncrease = FireRatePercentIncrease;

                return newCombinedStat;
            }
        }
        //ProjectileSpeed*(1f+ProjectileSpeedUpPercentage);
        //FlatPercentage
        //ScalingDownPercentage
        //Min
        //Max?
        public class ProjectileSpeedStat : ModifiableStat<ProjectileSpeedStat>
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

            public override ProjectileSpeedStat GetCombinedStat(ProjectileSpeedStat[] addStat)
            {

                ProjectileSpeedStat tempCombined = this.Copy();

                foreach (ProjectileSpeedStat stat in addStat)
                {

                    tempCombined.ProjectileSpeedDownPercentage.AddRange(stat.ProjectileSpeedDownPercentage);
                    tempCombined.ProjectileSpeedUpPercentage += stat.ProjectileSpeedUpPercentage;

                }

                return tempCombined;

            }

            public override ProjectileSpeedStat Copy()
            {
                ProjectileSpeedStat newCombinedStat = new ProjectileSpeedStat(BaseValue, ProjectileSpeedMinumum, ProjectileSpeedMaximum);

                newCombinedStat.ProjectileSpeedDownPercentage = new List<float>(ProjectileSpeedDownPercentage);
                newCombinedStat.ProjectileSpeedUpPercentage = ProjectileSpeedUpPercentage;

                return newCombinedStat;
            }
        }

        //ProcChance*(1f+ChanceUpPercentage);
        //FlatPercentage
        //Minimum
        public class ChanceStat : ModifiableStat<ChanceStat>
        {
            public float ChanceUpPercentage;
            public float MinimumChance = 0f;

            public ChanceStat(float baseValue, float minimumChance = 0f) : base(baseValue) => ChanceUpPercentage = minimumChance;

            protected override float TotalValueMath() { return Mathf.Max(MinimumChance, BaseValue * (1f + ChanceUpPercentage)); }
            public override void ResetModifiedValues() { ChanceUpPercentage = 0f; }
            public void ChangeProcChanceUpPercentage(float changeValue, bool undo = false) { if (!undo) ChanceUpPercentage += changeValue; else ChanceUpPercentage -= changeValue; }

            public override ChanceStat GetCombinedStat(ChanceStat[] addStat)
            {

                ChanceStat tempCombined = this.Copy();

                foreach (ChanceStat stat in addStat)
                {
                    tempCombined.ChanceUpPercentage += stat.ChanceUpPercentage;
                }

                return tempCombined;

            }

            public override ChanceStat Copy()
            {
                ChanceStat newCombinedStat = new ChanceStat(BaseValue, MinimumChance);
                newCombinedStat.ChanceUpPercentage = ChanceUpPercentage;

                return newCombinedStat;
            }
        }



        //Charges+AdditionalCharges
        public class ChargeStat : ModifiableStat<ChargeStat>
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

            public override ChargeStat GetCombinedStat(ChargeStat[] addStat)
            {

                ChargeStat tempCombined = this.Copy();

                foreach (ChargeStat stat in addStat)
                {
                    tempCombined.AdditionalCharges += stat.AdditionalCharges;

                    tempCombined.ChargePercentFlat += stat.ChargePercentFlat;

                }

                return tempCombined;

            }

            public override ChargeStat Copy()
            {
                ChargeStat newCombinedStat = new ChargeStat(BaseValue, ChargeScaling, MinimumCharges);
                newCombinedStat.AdditionalCharges = AdditionalCharges;
                newCombinedStat.ChargePercentFlat = ChargePercentFlat;

                return newCombinedStat;
            }
        }

        //Cooldown*(1f+CooldownIncreasedPercentage-CooldownReductionPercentage);

        //CooldownReductionPercentage HAS to be handled CAREFULLY. I want it to range from (0 to inf) but I want the scaling to be slow.
        //So Having to 50% reductions wont make it 100% itl be 50%*50%=25%.. So any modifications to CooldownReductionPercentage MUST be multiplied

        //CooldownIncreased Is different though. Instead well do it like this: 50%+50%=100%
        public class CooldownStat : ModifiableStat<CooldownStat>
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
            public override CooldownStat GetCombinedStat(CooldownStat[] addStat)
            {

                CooldownStat tempCombined = this.Copy();

                foreach (CooldownStat stat in addStat)
                {
                    tempCombined.AdditionalCooldown += stat.AdditionalCooldown;

                    tempCombined.CooldownPercentDecreases.AddRange(stat.CooldownPercentDecreases);
                    tempCombined.CooldownPercentIncrease += stat.CooldownPercentIncrease;

                }

                return tempCombined;

            }

            public override CooldownStat Copy()
            {
                CooldownStat newCombinedStat = new CooldownStat(BaseValue, CooldownScaling, MinimumCooldown);
                newCombinedStat.AdditionalCooldown = AdditionalCooldown;

                newCombinedStat.CooldownPercentDecreases = new List<float>(CooldownPercentDecreases);
                newCombinedStat.CooldownPercentIncrease = CooldownPercentIncrease;

                return newCombinedStat;
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
        public class RangeStat : ModifiableStat<RangeStat>
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
            public override RangeStat GetCombinedStat(RangeStat[] addStat)
            {

                RangeStat tempCombined = this.Copy();

                foreach (RangeStat stat in addStat)
                {

                    tempCombined.RangeDownPercentages.AddRange(stat.RangeDownPercentages);
                    tempCombined.RangeFlatPercentage += stat.RangeFlatPercentage;

                }

                return tempCombined;

            }

            public override RangeStat Copy()
            {
                RangeStat newCombinedStat = new RangeStat(BaseValue, SoftMax, ScaleConstant);

                newCombinedStat.RangeDownPercentages = new List<float>(RangeDownPercentages);
                newCombinedStat.RangeFlatPercentage = RangeFlatPercentage;

                return newCombinedStat;
            }
        }

        //Duration*(1f+DurationUpPercentage);
        public class DurationStat : ModifiableStat<DurationStat>
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
            public override DurationStat GetCombinedStat(DurationStat[] addStat)
            {

                DurationStat tempCombined = this.Copy();

                foreach (DurationStat stat in addStat)
                {

                    tempCombined.DurationDownPercentages.AddRange(stat.DurationDownPercentages);
                    tempCombined.DurationFlatPercentage += stat.DurationFlatPercentage;

                }

                return tempCombined;

            }

            public override DurationStat Copy()
            {
                DurationStat newCombinedStat = new DurationStat(BaseValue, SoftMax, ScaleConstant);

                newCombinedStat.DurationDownPercentages = new List<float>(DurationDownPercentages);
                newCombinedStat.DurationFlatPercentage = DurationFlatPercentage;

                return newCombinedStat;
            }
        }


    }

    public static class CharacterStats
    {
        public class RegenStat : ModifiableStat<RegenStat>
        {
            public float RegenScaling;
            public float MinimumRegen = 0f;

            public float AdditionalRegen = 0f;
            public float RegenPercentIncrease = 0f;
            public List<float> RegenPercentDecreases = new List<float>();

            public RegenStat(float baseValue, float regenScaling, float minimumRegen = 0f) : base(baseValue) => (RegenScaling, MinimumRegen) = (regenScaling, minimumRegen);

            protected override float TotalValueMath() { return Mathf.Max(MinimumRegen, BaseValue + AdditionalRegen * RegenScaling) * (1f + RegenPercentIncrease - (1f - RegenPercentDecreases.Aggregate(1f, (total, next) => total * next))); }

            public override void ResetModifiedValues() { AdditionalRegen = 0f; RegenPercentIncrease = 0f; RegenPercentDecreases.Clear(); }
            public void ChangeAdditionalRegen(float changeValue, bool undo = false) { if (!undo) AdditionalRegen += changeValue; else AdditionalRegen -= changeValue; }
            public void ChangeRegenByPercent(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) RegenPercentIncrease += percentChange;
                    else RegenPercentDecreases.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) RegenPercentIncrease -= percentChange;
                    else RegenPercentDecreases.Remove(1f - percentChange);
                }
            }
            public override RegenStat GetCombinedStat(RegenStat[] addStat)
            {

                RegenStat tempCombined = this.Copy();

                foreach (RegenStat stat in addStat)
                {
                    tempCombined.AdditionalRegen += stat.AdditionalRegen;

                    tempCombined.RegenPercentDecreases.AddRange(stat.RegenPercentDecreases);
                    tempCombined.RegenPercentIncrease += stat.RegenPercentIncrease;

                }

                return tempCombined;

            }

            public override RegenStat Copy()
            {
                RegenStat newCombinedStat = new RegenStat(BaseValue, RegenScaling, MinimumRegen);
                newCombinedStat.AdditionalRegen = AdditionalRegen;

                newCombinedStat.RegenPercentDecreases = new List<float>(RegenPercentDecreases);
                newCombinedStat.RegenPercentIncrease = RegenPercentIncrease;

                return newCombinedStat;
            }

        }
        public class MaxHealthStat : ModifiableStat<MaxHealthStat>
        {
            public float MaxHealthScaling;
            public float MinimumMaxHealth = 0f;

            public float AdditionalMaxHealth = 0f;
            public float MaxHealthPercentIncrease = 0f;
            public List<float> MaxHealthPercentDecreases = new List<float>();

            public MaxHealthStat(float baseValue, float maxHealthScaling = 1f, float minimumMaxHealth = 0f) : base(baseValue) => (MaxHealthScaling, MinimumMaxHealth) = (maxHealthScaling, minimumMaxHealth);

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
            public override MaxHealthStat GetCombinedStat(MaxHealthStat[] addStat)
            {

                MaxHealthStat tempCombined = this.Copy();

                foreach (MaxHealthStat stat in addStat)
                {
                    tempCombined.AdditionalMaxHealth += stat.AdditionalMaxHealth;

                    tempCombined.MaxHealthPercentDecreases.AddRange(stat.MaxHealthPercentDecreases);
                    tempCombined.MaxHealthPercentIncrease += stat.MaxHealthPercentIncrease;

                }

                return tempCombined;

            }

            public override MaxHealthStat Copy()
            {
                MaxHealthStat newCombinedStat = new MaxHealthStat(BaseValue, MaxHealthScaling, MinimumMaxHealth);
                newCombinedStat.AdditionalMaxHealth = AdditionalMaxHealth;

                newCombinedStat.MaxHealthPercentDecreases = new List<float>(MaxHealthPercentDecreases);
                newCombinedStat.MaxHealthPercentIncrease = MaxHealthPercentIncrease;

                return newCombinedStat;
            }

        }
        public abstract class HealthType
        {
            public abstract HealthCatagory Catagory { get; protected set; }

            public float CurrentValue;
            public MaxHealthStat MaxValue;
            public EffectivenessStat DamageResistance;
            public EffectivenessStat ObtainGain;
            public HealthType(float startingHealth, MaxHealthStat maxHealthStat, EffectivenessStat damageResistance = null)
            {
                (CurrentValue, MaxValue, DamageResistance) = (startingHealth, maxHealthStat, damageResistance);
                MaxValue.ChangedTotal += MaxChanged;
            }

            //public static int DamagePriority { get; set; }   //Damage goes from HighPriority to Low Priority
            public float TakeDamage(float damage, EffectivenessStat additionalDamageResistance = null)
            {
                float overDamage = Mathf.Min(0f, CurrentValue - damage);
                CurrentValue = Mathf.Max(0f,CurrentValue- damage);
                return overDamage;
            }

            public float AddCurrentValue(float addValue)
            {
                addValue = addValue * ObtainGain.Total;
                float overAdd = Mathf.Max(0f, CurrentValue + addValue- MaxValue.Total);
                CurrentValue = Mathf.Min(MaxValue.Total, CurrentValue + addValue);
                return CurrentValue;
            }

            public abstract void NaturalCurrentChange();
            protected abstract void MaxChanged(float newValue,float oldValue);
        }

        public class Health : HealthType
        {
            public RegenStat NaturalRegen; //In HP/s
            public float NaturalRegenRate = 0f; //In seconds
            public float SecondsUntilNextRegenTick = 0f;
            public override HealthCatagory Catagory { get; protected set; } = HealthCatagory.Health;
            public Health(float startingHealth, MaxHealthStat maxHealthStat, EffectivenessStat damageResistance = null) : base(startingHealth, maxHealthStat, damageResistance)
            {
            }

            public override void NaturalCurrentChange()// Do natural regen code here..
            {
                throw new NotImplementedException();
            }

            protected override void MaxChanged(float newValue, float oldValue)
            {
                throw new NotImplementedException();
            }
        }
        public class Armor : HealthType
        {
            public RegenStat NaturalRegen; //In HP/s
            public float NaturalRegenRate = 0f; //In seconds
            public float SecondsUntilNextRegenTick = 0f;

            public override HealthCatagory Catagory { get; protected set; } = HealthCatagory.Armor;
            public Armor(float startingHealth, MaxHealthStat maxHealthStat, EffectivenessStat damageResistance = null) : base(startingHealth, maxHealthStat, damageResistance)
            {
            }

            public override void NaturalCurrentChange()// Do natural regen code here..
            {
                throw new NotImplementedException();
            }

            protected override void MaxChanged(float newValue, float oldValue)
            {
                throw new NotImplementedException();
            }
        }

        public class Shield : HealthType
        {
            public RegenStat NaturalRegen; //In HP/s
            public float NaturalRegenRate = 0f; //In seconds
            public float SecondsUntilNextRegenTick = 0f;

            public override HealthCatagory Catagory { get; protected set; } = HealthCatagory.Shield;
            public Shield(float startingHealth, MaxHealthStat maxHealthStat, EffectivenessStat damageResistance = null) : base(startingHealth, maxHealthStat, damageResistance)
            {
            }

            public override void NaturalCurrentChange()// Do natural regen code here..
            {
                throw new NotImplementedException();
            }

            protected override void MaxChanged(float newValue, float oldValue)
            {
                throw new NotImplementedException();
            }
        }

        public class Barrier : HealthType
        {
            public RegenStat NaturalRegen; //In HP/s
            public float NaturalRegenRate = 0f; //In seconds
            public float SecondsUntilNextRegenTick = 0f;

            public override HealthCatagory Catagory { get; protected set; } = HealthCatagory.Barrier;
            public Barrier(float startingHealth, MaxHealthStat maxHealthStat, EffectivenessStat damageResistance = null) : base(startingHealth, maxHealthStat, damageResistance)
            {
            }

            public override void NaturalCurrentChange()// Do natural regen code here..
            {
                throw new NotImplementedException();
            }

            protected override void MaxChanged(float newValue, float oldValue)
            {
                throw new NotImplementedException();
            }
        }

        public enum HealthCatagory
        {
            Health, CelledHealth, Armor, Shield, CelledShield, Gaurd, Barrier //Max Health has no limit. Max Armor usually can only be equal to MaxHealth. Shield has no limit.  Barrier is at most Health+Shied
        }

    }
}
