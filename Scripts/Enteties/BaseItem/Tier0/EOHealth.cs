using BrannPack;
using BrannPack.ItemHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static BrannPack.BaseCharacter;

namespace AbilityCoopRougelike.Items
{
    public class EOHealth : Item<EOHealth>
    {
        public override ItemTier Tier { get; init; } =;
        public override ItemSubTier SubTier { get; init; } =;
        public override ItemTierModifier[] DefaultModifiers { get; init; } =;
        public override ItemTierModifier[] PossibleModifiers { get; init; } =;
        public override string Name { get; init; } = "Essence of Health";
        public override string CodeName { get; init; } = "EO_Health";
        public override string Description { get; init; } ="Slightly Increase Max Health";
        public override string AdvancedDescription { get; init; } ="";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int> 
        {
            {EffectTag.IsDefensive,3 }, //Gaining Health Should Be a 3
            {EffectTag.IsHealthEnabler,3 }, //Gives more Health
            //{EffectTag.IsHealthBoost,3 }, //If it were to enhance future Healthboosts, then this would be a 3, but it technically it doesnt (only way to is to boost base HP)
            {EffectTag.IsLowHPBoost,2 }, //Gives you more room to work with if youre low on HP
            //{EffectTag.IsTealEnabler,1 } //Doest give you Teal items, it IS a teal item.
        };

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
