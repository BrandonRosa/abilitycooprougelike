using BrannPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.BaseCharacter;

namespace AbilityCoopRougelike.Scripts.Enteties.BaseItem.Tier0
{
    public class EOHealth : Item
    {
        public static Dictionary<string, ItemFilter> ItemPools;
        public static List<ItemSet> ItemSets;

        private static ItemTier Tier;
        private static ItemSubTier SubTier;
        private static List<ItemTierModifier> DefaultModifiers;
        private static string Name;
        private static string CodeName;
        private static string Description;
        private static string AdvancedDescription;
        private static bool RequiresConfirmation;
        private static Dictionary<EffectTag, float> EffectWeights;
        public void Subscribe(BaseCharacter character)
        {
            character.OnStatCalculation += ApplyStats;
        }

        private void ApplyStats(object sender, StatHookEventArgs e)
        {
            e.MaxHealthMultAdd += 0.03f; // 3% health boost
        }
    }
}
