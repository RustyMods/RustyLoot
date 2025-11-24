using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class ForsakenBlow
{
    public static void Setup()
    {
        var def = new MagicItemEffectDefinition("ForsakenBlow", "$mod_epicloot_forsakenblow",
            "$mod_epicloot_forsakenblow_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shield);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary,
            ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(3, 8, 1);
        def.ValuesPerRarity.Epic = new ValueDef(6, 12, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(10, 15, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(13, 20, 1);
        def.Register();
        def.Serialize();
    }

    [HarmonyPatch(typeof(Character), nameof(Character.BlockAttack))]
    private static class ForsakenBlow_Character_BlockAttack_Patch
    {
        private static void Postfix(Character __instance)
        {
            if (!DefinitionExtensions.IsEnabled("ForsakenBlow")) return;
            if (__instance is not Player player) return;
            if (player.HasActiveMagicEffect("ForsakenBlow", out float modifier))
            {
                player.m_guardianPowerCooldown -= modifier;
            }
        }
    }
}