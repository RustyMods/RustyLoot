using EpicLootAPI;

namespace RustyLoot;

public static class Hafrnlight
{
    public static void Setup()
    {
        var item = new Legendary(LegendaryType.Legendary, "Hafrnlight");
        item.info.GuaranteedEffectCount = 6;
        item.info.GuaranteedMagicEffects.Add(
            new GuaranteedMagicEffect("MistVision")
        );
        item.info.Requirements.AllowedItemNames.Add("$item_demister");
        item.info.Requirements.AllowedRarities.Add(ItemRarity.Legendary, ItemRarity.Mythic);
        item.Register();
    }
}