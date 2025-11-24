using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class ModifyAdrenaline
{
    public static void Setup()
    {
        MagicItemEffectDefinition def = new MagicItemEffectDefinition("ModifyAdrenaline", "$mod_epicloot_modifyadrenaline", "$mod_epicloot_modifyadrenaline_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Trinket);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Epic = new ValueDef(10, 20, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(20, 30, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(30, 40, 1);
        def.Register();
        def.Serialize();
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.AddAdrenaline))]
    private static class Player_AddAdrenaline_Patch
    {
        private static void Prefix(Player __instance, ref float v)
        {
            if (!DefinitionExtensions.IsEnabled("ModifyAdrenaline")) return;
            
            if (v < 0) return;
            
            if (__instance.HasActiveMagicEffect("ModifyAdrenaline", out float modifier))
            {
                v *= 1 + modifier / 100;
            }
            RustyLootPlugin.RustyLootLogger.LogWarning($"AddAdrenaline {v}");
        }
    }
}