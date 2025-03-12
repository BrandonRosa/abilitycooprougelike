using BrannPack.ItemHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Tiers
{
    public abstract class ItemTier<T> : ItemTier where T : ItemTier<T>
    {
        public static T instance { get; private set; }

        public ItemTier()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class ItemTier
    {
        public abstract string Name { get; init; }
        public abstract string CodeName { get; init; }

        //Lowest Possible Vanilla Power Level is 1
        public abstract float AveragePowerLevel { get; init; }

        //Lowest Possible in Vanilla is 1
        public abstract float Conversion { get; init; }
        protected abstract HashSet<Item> AllItemsInTier { get; set; }
        protected abstract HashSet<Item> AllUnlockedItems { get; set; }
        public abstract HashSet<ItemSubTier> SubTiers { get; init; }
    }

    public class Tier0 : ItemTier<Tier0>
    {
        public override string Name { get; init; } = "Teal";
        public override string CodeName { get; init; } = "Tier0";

        public override float AveragePowerLevel { get; init; } = 1f;

        public override float Conversion { get; init; } = 1f;
        public override HashSet<ItemSubTier> SubTiers { get; init; } = new HashSet<ItemSubTier> { ItemSubTier.Essences,ItemSubTier.Promices };
        protected override HashSet<Item> AllItemsInTier { get; set; }
        protected override HashSet<Item> AllUnlockedItems { get; set; }

    }

    public class Tier1 : ItemTier<Tier1>
    {
        public override string Name { get; init; } = "White";
        public override string CodeName { get; init; } = "Tier1";

        public override float AveragePowerLevel { get; init; } = 2.5f;

        public override float Conversion { get; init; } = 3f;
        public override HashSet<ItemSubTier> SubTiers { get; init; } = new HashSet<ItemSubTier> { };
        protected override HashSet<Item> AllItemsInTier { get; set; }
        protected override HashSet<Item> AllUnlockedItems { get; set; }

    }

    public class Tier2 : ItemTier<Tier2>
    {
        public override string Name { get; init; } = "Green";
        public override string CodeName { get; init; } = "Tier2";

        public override float AveragePowerLevel { get; init; } = 3.75f;

        public override float Conversion { get; init; } = 6f;
        public override HashSet<ItemSubTier> SubTiers { get; init; } = new HashSet<ItemSubTier> { };
        protected override HashSet<Item> AllItemsInTier { get; set; }
        protected override HashSet<Item> AllUnlockedItems { get; set; }

    }

    public class TierA : ItemTier<TierA>
    {
        public override string Name { get; init; } = "Active";
        public override string CodeName { get; init; } = "TierA";

        public override float AveragePowerLevel { get; init; } = 6.5625f;

        public override float Conversion { get; init; } = 12f;
        public override HashSet<ItemSubTier> SubTiers { get; init; } = new HashSet<ItemSubTier> { };
        protected override HashSet<Item> AllItemsInTier { get; set; }
        protected override HashSet<Item> AllUnlockedItems { get; set; }

    }

    public class Tier3 : ItemTier<Tier3>
    {
        public override string Name { get; init; } = "Yellow";
        public override string CodeName { get; init; } = "Tier3";

        public override float AveragePowerLevel { get; init; } = 9.84375f;

        public override float Conversion { get; init; } = 18f;
        public override HashSet<ItemSubTier> SubTiers { get; init; } = new HashSet<ItemSubTier> { };
        protected override HashSet<Item> AllItemsInTier { get; set; }
        protected override HashSet<Item> AllUnlockedItems { get; set; }

    }

    public class Tier4 : ItemTier<Tier4>
    {
        public override string Name { get; init; } = "Red";
        public override string CodeName { get; init; } = "Tier4";

        public override float AveragePowerLevel { get; init; } = 14.765625f;

        public override float Conversion { get; init; } = 36f;
        public override HashSet<ItemSubTier> SubTiers { get; init; } = new HashSet<ItemSubTier> { };
        protected override HashSet<Item> AllItemsInTier { get; set; }
        protected override HashSet<Item> AllUnlockedItems { get; set; }

    }


    public abstract class ItemSubTier<TTier, TSubTier> : ItemSubTier
      where TTier : ItemTier<TTier>
      where TSubTier : ItemSubTier<TTier, TSubTier>
    {
        public static TSubTier instance { get; private set; }

        protected ItemSubTier(TTier parentTier)
        {
            if (instance != null)
                throw new InvalidOperationException($"Singleton class \"{typeof(TSubTier).Name}\" inheriting ItemSubTier was instantiated twice");

            instance = this as TSubTier;
            ParentTier = parentTier;
        }

        public override ItemTier ParentTier { get; init; }
    }

    public abstract class ItemSubTier
    {
        public static ItemSubTier Essences;
        public static ItemSubTier Promices;
        public static ItemSubTier Elite;

        public abstract string Name { get; init; }
        public abstract string CodeNam { get; init; }
        protected abstract HashSet<Item> AllItemsInTier { get; set; }
        protected abstract HashSet<Item> AllUnlockedItems { get; set; }
        public abstract ItemTier ParentTier { get; init; }
    }

}
