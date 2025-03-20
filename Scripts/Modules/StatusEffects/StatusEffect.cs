using BrannPack.Character;
using BrannPack.ItemHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.StatusEffectHandling
{
    public class StatusEffectStack
    {
        public BaseCharacter Owner;
        public StatusEffectInfo statusEffectInfo;
        public float Duration;
        public float Count;
    }
    public abstract class StatusEffect<T> : StatusEffect where T : StatusEffect<T>
    {
        public static T instance { get; private set; }

        public StatusEffect()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class StatusEffect
    {
        public abstract string Name { get; init; }
        public abstract string CodeName { get; init; }
        public abstract string Description { get; init; }

        public abstract HashSet<StatusEffectTag> Tags { get; init; }

        public abstract Type StatusEffectInfo { get; init; }

        public abstract void Init();

        public abstract void Update();


    }

    public class StatusEffectInfo
    {
        public BaseCharacter Source;
        public BaseCharacter Target;
        public StatusEffect StatusEffect;
        public float InitialDuration;
        public float InitialCount;
    }


    public enum StatusEffectTag
    {
        //Basic Category
        IsBuff, IsDebuff, IsNeutral,

        //Debuff Type
        IsDamageDealing, IsDamageDown, IsMovementRestriction, IsDefenseDown,

        //BuffType
        IsDamageUp, IsMovementUp, IsDefenseUp
    }
}
