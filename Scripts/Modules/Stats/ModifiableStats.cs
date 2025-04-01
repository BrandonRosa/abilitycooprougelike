using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.Character.BaseCharacterBody;
using static BrannPack.ModifiableStats.AbilityStats;
using static BrannPack.ModifiableStats.CharacterStats;
using static System.Net.Mime.MediaTypeNames;

namespace BrannPack.ModifiableStats
{
    public abstract partial class ModifiableStat: Resource
    {
        public abstract float Total { get; protected set; }
        public abstract float BaseValue { get; protected set; }

        public event Action<float, float> ChangedTotal;

        protected abstract float TotalValueMath();
        public virtual float CalculateTotal()
        {
            float prevTotal = Total;
            Total = TotalValueMath();
            if (prevTotal != Total)
                ChangedTotal.Invoke(Total, prevTotal);
            return Total;
        }
        public abstract void ResetModifiedValues();

        public abstract ModifiableStat CopyBase();
    }
    public abstract partial class ModifiableStat<T>: ModifiableStat where T:ModifiableStat<T>
    {
        public override float Total { get; protected set; } = 0f;
        public override float BaseValue { get; protected set; } = 0f;

        


        public ModifiableStat(float baseValue) => BaseValue = baseValue;
        

        public override abstract void ResetModifiedValues();

        

        public abstract void AddCombinedStats(params T[] addStat);

        public T GetCombinedStat(params T[] addStat)
        {
            T temp = Copy();
            temp.AddCombinedStats(addStat);
            return temp;
        }

        public float GetCombinedTotal(params T[] addStat){return GetCombinedStat(addStat).CalculateTotal();}

        public float GetCombinedTotal(Stat statType ,params StatsHolder[] statsHolders) 
        {
            List<T> statList = new();
            foreach(StatsHolder statHolder in statsHolders)
            {
                statList.Add(statHolder.GetStatByVariable<T>(statType));
            }

            return GetCombinedTotal(statList.ToArray());
        }

        public abstract T Copy();

        public static T GetCombinedStat(T baseStat,T[] addStat)
        {
            return baseStat.GetCombinedStat(addStat);
        }

        public override ModifiableStat CopyBase()
        {
            return Copy();
        }
    }

    public partial class FullExpandedStat : ModifiableStat<FullExpandedStat>
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


        public override FullExpandedStat AddCombinedStats(params FullExpandedStat[] addStat)
        {

            foreach (FullExpandedStat stat in addStat)
            {
                AdditionalBaseValue += stat.AdditionalBaseValue;

                MultipliedPercentageIncreases.AddRange(stat.MultipliedPercentageIncreases);
                MultipliedPercentageDecreases.AddRange(stat.MultipliedPercentageDecreases);
                AdditivePercentageChange += stat.AdditivePercentageChange;

                AdditiveFlatNonScaledChange += stat.AdditiveFlatNonScaledChange;

            }

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

    public partial class EffectivenessStat: ModifiableStat<EffectivenessStat>
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

        public override EffectivenessStat AddCombinedStats(params EffectivenessStat[] addStat)
        {

            EffectivenessStat tempCombined = this.Copy();

            foreach (EffectivenessStat stat in addStat)
            {

                MultipliedPercentageIncreases.AddRange(stat.MultipliedPercentageIncreases);
                MultipliedPercentageDecreases.AddRange(stat.MultipliedPercentageDecreases);
                AdditivePercentageChange += stat.AdditivePercentageChange;

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

        //public class StatsHolder<T>
        //{
        //    public T Owner;
        //    private Dictionary<Stat, ModifiableStat> _stats = new();

        //    public static event Action<T, Stat, ModifiableStat> RefreshAbilityStatVariable;
        //    public static event Action<T, Stat, ModifiableStat, float, float> StatUpdatedWithNewTotal;

        //    public StatsHolder()
        //    {
        //        // Initialize dictionary with all possible _stats
        //        _stats[Stat.Chance] = new ChanceStat(0f);
        //        _stats[Stat.Damage] = new DamageStat(0f,1f);
        //        _stats[Stat.FireRate] = new FireRateStat(0f);
        //        _stats[Stat.ProjectileSpeed] = new ProjectileSpeedStat(0f);
        //        _stats[Stat.ProcChance] = new ChanceStat(0f);
        //        _stats[Stat.CritDamage] = new DamageStat(0f, 1f);
        //        _stats[Stat.Charges] = new ChargeStat(1f);
        //        _stats[Stat.Cooldown] = new CooldownStat(0f);
        //        _stats[Stat.SpamCooldown] = new CooldownStat(0f);
        //        _stats[Stat.Range] = new RangeStat(1f,10f);
        //        _stats[Stat.Duration] = new DurationStat(1f,10f);
        //        _stats[Stat.Luck] = new ChanceStat(0f);
        //        _stats[Stat.Lifesteal] = new EffectivenessStat();

        //        // Hook up event listeners for all _stats
        //        foreach (var kvp in _stats)
        //        {
        //            Stat statKey = kvp.Key;
        //            kvp.Value.ChangedTotal += (newTotal, prevTotal) =>
        //                StatUpdatedWithNewTotal?.Invoke(Owner, statKey, kvp.Value, newTotal, prevTotal);
        //        }
        //    }

        //    public ModifiableStat GetStatByVariable(Stat statType)
        //    {
        //        return _stats.TryGetValue(statType, out var stat) ? stat : null;
        //    }

        //    public TStat GetStatByVariable<TStat>(Stat statType) where TStat : ModifiableStat
        //    {
        //        return _stats.TryGetValue(statType, out var stat) ? stat as TStat : null;
        //    }

        //    public void RecalculateByStatVariable(Stat statType)
        //    {
        //        if (!_stats.TryGetValue(statType, out var stat)) return;

        //        stat.ResetModifiedValues();
        //        RefreshAbilityStatVariable?.Invoke(Owner, statType, stat);
        //        stat.CalculateTotal();
        //    }

        //    public void RecalculateAndAddStats(Stat statType, ModifiableStat otherStat)
        //    {
        //        if (!_stats.TryGetValue(statType, out var stat)) return;

        //        stat.ResetModifiedValues();
        //        stat.AddCombinedStats(otherStat);

        //        RefreshAbilityStatVariable?.Invoke(Owner, statType, stat);
        //        stat.CalculateTotal();
        //    }

        //    public void RecalculateAllStats()
        //    {
        //        foreach (var statType in _stats.Keys)
        //        {
        //            RecalculateByStatVariable(statType);
        //        }
        //    }
        //}

        public class StatsHolder
        {
            public static Dictionary<Stat, ModifiableStat> DefaultZeroStats = new Dictionary<Stat, ModifiableStat>()
            {
                {Stat.Chance, new ChanceStat(0f)},
                {Stat.Damage, new DamageStat(0f, 1f)},
                {Stat.FireRate, new FireRateStat(0f)},
                {Stat.ProjectileSpeed, new ProjectileSpeedStat(0f)},
                {Stat.ProcChance, new ChanceStat(0f)},
                {Stat.CritDamage, new DamageStat(0f, 1f)},
                {Stat.Charges, new ChargeStat(1f)}, 
                {Stat.Cooldown, new CooldownStat(0f)},
                {Stat.SpamCooldown, new CooldownStat(0f)}, 
                {Stat.Range, new RangeStat(1f, 10f)}, 
                {Stat.Duration, new DurationStat(1f, 10f)}, 
                {Stat.Luck, new ChanceStat(0f)}, 
                {Stat.ChainLifesteal, new EffectivenessStat(0f,0f)},
                {Stat.Lifesteal, new EffectivenessStat(0f,0f)},
                {Stat.MoveSpeed, new MoveSpeedStat(1,10) }
            };
            private Dictionary<Stat, ModifiableStat> _stats = new();

            public event Action<Stat, ModifiableStat> RefreshAbilityStatVariable;
            public event Action<Stat, ModifiableStat, float, float> StatUpdatedWithNewTotal;

            public StatsHolder()
            {
            }

            public StatsHolder(Dictionary<Stat, ModifiableStat> stats)
            {
                _stats=stats
                // Hook up event listeners for all _stats
                foreach (var kvp in stats)
                {
                    Stat statKey = kvp.Key;
                    kvp.Value.ChangedTotal += (newTotal, prevTotal) =>
                        StatUpdatedWithNewTotal?.Invoke(statKey, kvp.Value, newTotal, prevTotal);
                }
            }

            public StatsHolder(params Stat[] zeroStats)
            {
                foreach (Stat stat in zeroStats)
                {
                    if (DefaultZeroStats.TryGetValue(stat, out var defaultStat))
                    {
                        _stats[stat] = defaultStat.CopyBase();
                        _stats[stat].ChangedTotal += (newTotal, prevTotal) =>
                        StatUpdatedWithNewTotal?.Invoke( stat, _stats[stat], newTotal, prevTotal);
                    }
                }
            }

            public void SetStat<T>(Stat stat,T modifiableStat) where T : ModifiableStat
            {
                if(_stats.ContainsKey(stat))
                    _stats[stat].ChangedTotal-= (newTotal, prevTotal) =>
                        StatUpdatedWithNewTotal?.Invoke(stat, _stats[stat], newTotal, prevTotal);

                _stats[stat] = modifiableStat;

                modifiableStat.ChangedTotal+= (newTotal, prevTotal) =>
                        StatUpdatedWithNewTotal?.Invoke(stat, modifiableStat, newTotal, prevTotal);
            }

            public ModifiableStat GetStatByVariable(Stat statType)
            {
                return _stats.TryGetValue(statType, out var stat) ? stat : null;
            }

            public TStat GetStatByVariable<TStat>(Stat statType) where TStat : ModifiableStat
            {
                return _stats.TryGetValue(statType, out var stat) ? stat as TStat : null;
            }

            public void RecalculateByStatVariable(Stat statType)
            {
                if (!_stats.TryGetValue(statType, out var stat)) return;

                stat.ResetModifiedValues();
                RefreshAbilityStatVariable?.Invoke(statType, stat);
                stat.CalculateTotal();
            }

            public void RecalculateAndAddStats(Stat statType, ModifiableStat otherStat)
            {
                if (!_stats.TryGetValue(statType, out var stat)) return;

                stat.ResetModifiedValues();
                stat.AddCombinedStats(otherStat);

                RefreshAbilityStatVariable?.Invoke(statType, stat);
                stat.CalculateTotal();
            }

            public void RecalculateAllStats()
            {
                foreach (var statType in _stats.Keys)
                {
                    RecalculateByStatVariable(statType);
                }
            }

            public StatsHolder Copy()
            {
                Dictionary<Stat, ModifiableStat> newDict = new();
                foreach(var kvp in _stats)
                {
                    newDict[kvp.Key] = kvp.Value.CopyBase();
                }
                return new StatsHolder(newDict);
            }
        }

        public class StatsHolder<T> : StatsHolder
        {
            public T Owner;
            public static event Action<T, Stat, ModifiableStat> GlobalRefreshAbilityStatVariable;
            public static event Action<T, Stat, ModifiableStat, float, float> GlobalStatUpdatedWithNewTotal;

            public StatsHolder(T owner) : base()
            {
                Owner = owner;

                // Subscribe instance-level events to global events
                RefreshAbilityStatVariable += (stat, modStat) =>
                    GlobalRefreshAbilityStatVariable?.Invoke(Owner, stat, modStat);

                StatUpdatedWithNewTotal += (stat, modStat, newTotal, prevTotal) =>
                    GlobalStatUpdatedWithNewTotal?.Invoke(Owner, stat, modStat, newTotal, prevTotal);
            }

            public StatsHolder(T owner, Dictionary<Stat, ModifiableStat> stats) : base(stats)
            {
                Owner = owner;

                // Subscribe instance-level events to global events
                RefreshAbilityStatVariable += (stat, modStat) =>
                    GlobalRefreshAbilityStatVariable?.Invoke(Owner, stat, modStat);

                StatUpdatedWithNewTotal += (stat, modStat, newTotal, prevTotal) =>
                    GlobalStatUpdatedWithNewTotal?.Invoke(Owner, stat, modStat, newTotal, prevTotal);
            }

            public StatsHolder(T owner, params Stat[] zeroStats) : base(zeroStats)
            {
                Owner = owner;

                // Subscribe instance-level events to global events
                RefreshAbilityStatVariable += (stat, modStat) =>
                    GlobalRefreshAbilityStatVariable?.Invoke(Owner, stat, modStat);

                StatUpdatedWithNewTotal += (stat, modStat, newTotal, prevTotal) =>
                    GlobalStatUpdatedWithNewTotal?.Invoke(Owner, stat, modStat, newTotal, prevTotal);
            }
        }

        public partial class DamageStat : ModifiableStat<DamageStat>
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

            public override void AddCombinedStats(params DamageStat[] addStat)
            {
                foreach (DamageStat stat in addStat)
                {
                    AdditionalDamage += stat.AdditionalDamage;

                    DamagePercentDecreases.AddRange(stat.DamagePercentDecreases);
                    DamagePercentIncrease += stat.DamagePercentIncrease;

                }

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

        public partial class FireRateStat : ModifiableStat<FireRateStat>
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

            public override void AddCombinedStats(params FireRateStat[] addStat)
            {

                foreach (FireRateStat stat in addStat)
                {

                    FireRateDownPercentages.AddRange(stat.FireRateDownPercentages);
                    FireRateUpPercentage += stat.FireRateUpPercentage;

                }

            }

            public override FireRateStat Copy()
            {
                FireRateStat newCombinedStat = new FireRateStat(BaseValue, FireRateMinimum);
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
        public partial class ProjectileSpeedStat : ModifiableStat<ProjectileSpeedStat>
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

            public override void AddCombinedStats(ProjectileSpeedStat[] addStat)
            {

                foreach (ProjectileSpeedStat stat in addStat)
                {

                    ProjectileSpeedDownPercentage.AddRange(stat.ProjectileSpeedDownPercentage);
                    ProjectileSpeedUpPercentage += stat.ProjectileSpeedUpPercentage;

                }

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
        public partial class ChanceStat : ModifiableStat<ChanceStat>
        {
            public float ChanceUpPercentage;
            public float MinimumChance = 0f;

            public ChanceStat(float baseValue, float minimumChance = 0f) : base(baseValue) => ChanceUpPercentage = minimumChance;

            protected override float TotalValueMath() { return Mathf.Max(MinimumChance, BaseValue * (1f + ChanceUpPercentage)); }
            public override void ResetModifiedValues() { ChanceUpPercentage = 0f; }
            public void ChangeProcChanceUpPercentage(float changeValue, bool undo = false) { if (!undo) ChanceUpPercentage += changeValue; else ChanceUpPercentage -= changeValue; }

            public override void AddCombinedStats(params ChanceStat[] addStat)
            {

                foreach (ChanceStat stat in addStat)
                {
                    ChanceUpPercentage += stat.ChanceUpPercentage;
                }

            }

            public override ChanceStat Copy()
            {
                ChanceStat newCombinedStat = new ChanceStat(BaseValue, MinimumChance);
                newCombinedStat.ChanceUpPercentage = ChanceUpPercentage;

                return newCombinedStat;
            }
        }



        //Charges+AdditionalCharges
        public partial class ChargeStat : ModifiableStat<ChargeStat>
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

            public override void AddCombinedStats(ChargeStat[] addStat)
            {

                foreach (ChargeStat stat in addStat)
                {
                    AdditionalCharges += stat.AdditionalCharges;

                    ChargePercentFlat += stat.ChargePercentFlat;

                }

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
        public partial class CooldownStat : ModifiableStat<CooldownStat>
        {
            public float CooldownScaling;
            public float MinimumCooldown = 0f;

            public float AdditionalCooldown = 0f;
            public float CooldownPercentIncrease = 0f;
            public List<float> CooldownPercentDecreases = new List<float>();

            public CooldownStat(float baseValue, float minimumCooldown = .1f,float cooldownScaling=1f) : base(baseValue) => (CooldownScaling, MinimumCooldown) = (cooldownScaling, minimumCooldown);

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
            public override void AddCombinedStats(CooldownStat[] addStat)
            {

                foreach (CooldownStat stat in addStat)
                {
                    AdditionalCooldown += stat.AdditionalCooldown;

                    CooldownPercentDecreases.AddRange(stat.CooldownPercentDecreases);
                    CooldownPercentIncrease += stat.CooldownPercentIncrease;

                }

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
        public partial class RangeStat : ModifiableStat<RangeStat>
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
            public override void AddCombinedStats(RangeStat[] addStat)
            {

                foreach (RangeStat stat in addStat)
                {

                    RangeDownPercentages.AddRange(stat.RangeDownPercentages);
                    RangeFlatPercentage += stat.RangeFlatPercentage;

                }

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
        public partial class DurationStat : ModifiableStat<DurationStat>
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
            public override void AddCombinedStats(DurationStat[] addStat)
            {
                foreach (DurationStat stat in addStat)
                {

                    DurationDownPercentages.AddRange(stat.DurationDownPercentages);
                    DurationFlatPercentage += stat.DurationFlatPercentage;

                }

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
        public partial class MoveSpeedStat : ModifiableStat<MoveSpeedStat>
        {
            public float SpeedMinimum = 1f;
            public float SoftMax = 100f;
            public float ScaleConstant = 1.8f;

            public float SpeedFlatPercentage = 0f;
            public List<float> SlowResistPercentages = new List<float>();
            public List<float> SlowPercentages = new List<float>();

            //Reduction Calculation has to be done carfully. a 25% reduction and a 45% reduction would end up being 58.75% (41.25% of the original) 
            public MoveSpeedStat(float baseValue, float softMax, float speedMinimum = 1f, float scaleConstant = 1.8f) : base(baseValue) => (SpeedMinimum, SoftMax, ScaleConstant) = (speedMinimum, softMax, scaleConstant);
            protected override float TotalValueMath()
            {
                float slowResist =SlowResistPercentages.Aggregate(1f, (total, next) => total * next);
                float slow = SlowPercentages.Aggregate(1f, (total, next) => total * next);
                float totalUnmodChange = BaseValue * (1f+SpeedFlatPercentage - Mathf.Clamp(((1-slow)*slowResist),0f,1f));
                return Mathf.Max(SpeedMinimum, SoftMax - (SoftMax - BaseValue) * Mathf.Exp(-ScaleConstant * totalUnmodChange / SoftMax));
            }

            public override void ResetModifiedValues() { SpeedFlatPercentage = 0f; SlowPercentages.Clear(); }

            public void ChangeFlatSpeedPercentage(float percentChange,bool undo=false)
            {
                if (!undo)
                    SpeedFlatPercentage += percentChange;
                else
                    SpeedFlatPercentage -= percentChange;
            }
            public void ChangeSlowPercentage(float percentChange, bool undo = false)
            {
                if (!undo)
                {
                    if (percentChange >= 0) SlowResistPercentages.Add(1f-percentChange);
                    else SlowPercentages.Add(1f - percentChange);
                }
                else
                {
                    if (percentChange >= 0) SlowResistPercentages.Remove(1f - percentChange);
                    else SlowPercentages.Remove(1f - percentChange);
                }
            }
            public override void AddCombinedStats(MoveSpeedStat[] addStat)
            {

                foreach (MoveSpeedStat stat in addStat)
                {

                    SlowPercentages.AddRange(stat.SlowPercentages);
                    SlowResistPercentages.AddRange(stat.SlowResistPercentages);
                    SpeedFlatPercentage += stat.SpeedFlatPercentage;

                }

            }

            public override MoveSpeedStat Copy()
            {
                MoveSpeedStat newCombinedStat = new MoveSpeedStat(BaseValue, SoftMax, ScaleConstant);

                newCombinedStat.SlowPercentages = new List<float>(SlowPercentages);
                newCombinedStat.SpeedFlatPercentage = SpeedFlatPercentage;

                return newCombinedStat;
            }
        }
        public partial class RegenStat : ModifiableStat<RegenStat>
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
            public override void AddCombinedStats(RegenStat[] addStat)
            {

                foreach (RegenStat stat in addStat)
                {
                    AdditionalRegen += stat.AdditionalRegen;

                    RegenPercentDecreases.AddRange(stat.RegenPercentDecreases);
                    RegenPercentIncrease += stat.RegenPercentIncrease;

                }

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
        public partial class MaxHealthStat : ModifiableStat<MaxHealthStat>
        {
            public float MaxHealthScaling;
            public float MinimumMaxHealth = 0f;

            public float AdditionalMaxHealth = 0f;
            public List<MaxHealthStat> FollowingMaxHealth=new List<MaxHealthStat>();
            public float MaxHealthPercentIncrease = 0f;
            public List<float> MaxHealthPercentDecreases = new List<float>();

            public MaxHealthStat(float baseValue, float maxHealthScaling = 1f, float minimumMaxHealth = 0f) : base(baseValue) => (MaxHealthScaling, MinimumMaxHealth) = (maxHealthScaling, minimumMaxHealth);

            public void AddFollowingMaxHealth(MaxHealthStat maxHealthStat)
            {
                FollowingMaxHealth.Add(maxHealthStat);
                maxHealthStat.ChangedTotal += (float newTotal, float oldTotal) => CalculateTotal();
            }

            public bool RemoveFollowingMaxHealth(MaxHealthStat maxHealthStat)
            {
                bool removed=FollowingMaxHealth.Remove(maxHealthStat);
                maxHealthStat.ChangedTotal += (float newTotal, float oldTotal) => CalculateTotal();
                return removed;
            }

            protected override float TotalValueMath() { return Mathf.Max(MinimumMaxHealth, BaseValue +FollowingMaxHealth.Sum(maxhealth=>maxhealth.Total) + AdditionalMaxHealth * MaxHealthScaling) * (1f + MaxHealthPercentIncrease - (1f - MaxHealthPercentDecreases.Aggregate(1f, (total, next) => total * next))); }

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
            public override void AddCombinedStats(MaxHealthStat[] addStat)
            {

                foreach (MaxHealthStat stat in addStat)
                {
                    AdditionalMaxHealth += stat.AdditionalMaxHealth;

                    MaxHealthPercentDecreases.AddRange(stat.MaxHealthPercentDecreases);
                    MaxHealthPercentIncrease += stat.MaxHealthPercentIncrease;

                }

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
        public abstract partial class HealthType :Resource
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

            public event Action<float,float,float> AfterCurrentValueChange;
            public float AddCurrentValue(float addValue)
            {
                addValue = addValue * ObtainGain.Total;
                float overAdd = Mathf.Max(0f, CurrentValue + addValue- MaxValue.Total);
                CurrentValue = Mathf.Min(MaxValue.Total, CurrentValue + addValue);
                AfterCurrentValueChange?.Invoke(CurrentValue,addValue, overAdd);
                return CurrentValue;
            }

            public abstract void NaturalCurrentChange();
            protected abstract void MaxChanged(float newValue,float oldValue);
        }

        public partial class Health : HealthType
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
        public partial class Armor : HealthType
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

        public partial class Shield : HealthType
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

        public partial class BarrierHealth : HealthType
        {
            public RegenStat NaturalRegen; //In HP/s
            public float NaturalRegenRate = 0f; //In seconds
            public float SecondsUntilNextRegenTick = 0f;

            public override HealthCatagory Catagory { get; protected set; } = HealthCatagory.Barrier;
            public BarrierHealth(float startingHealth, MaxHealthStat maxHealthStat, EffectivenessStat damageResistance = null) : base(startingHealth, maxHealthStat, damageResistance)
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
            Health, CelledHealth, CursedHealth ,
                Armor, 
            Shield, CelledShield, Gaurd, 
                Barrier //Max Health has no limit. Max Armor usually can only be equal to MaxHealth. Shield has no limit.  BarrierHealth is at most Health+Shied
        }

    }
}
