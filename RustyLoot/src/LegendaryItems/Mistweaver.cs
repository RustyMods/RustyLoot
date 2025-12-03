using EpicLootAPI;

namespace RustyLoot;

public static class Mistweaver
{
    public static void Setup()
    {
        var item = new Legendary(LegendaryType.Legendary, "Mistweaver");
        item.info.GuaranteedEffectCount = 6;
        item.info.GuaranteedMagicEffects.Add(
            new GuaranteedMagicEffect("MagicArrow"), 
            new GuaranteedMagicEffect(EffectType.Weightless),
            new GuaranteedMagicEffect(EffectType.AddBowsSkill, 10, 20)
            );
        item.info.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Bows);
        item.info.Requirements.AllowedRarities.Add(ItemRarity.Legendary, ItemRarity.Mythic);
        item.Register();
    }
}