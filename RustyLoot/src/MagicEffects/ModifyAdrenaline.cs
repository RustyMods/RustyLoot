using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class ModifyAdrenaline
{
    public static void Setup()
    {
        MagicEffect def = new MagicEffect("ModifyAdrenaline");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Trinket);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Epic = new ValueDef(10, 20, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(20, 30, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(30, 40, 1);
        def.Register();
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.AddAdrenaline))]
    private static class Player_AddAdrenaline_Patch
    {
        private static void Prefix(Player __instance, ref float v)
        {
            if (!MagicEffect.IsEnabled("ModifyAdrenaline")) return;
            
            if (v < 0) return;

            if (__instance.HasActiveMagicEffect("ModifyAdrenaline", out float modifier, 0.01f))
            {
                float before = v;
                v *= 1 + modifier;
                float after = v;

                if (MagicEffect.ShowLogs("ModifyAdrenaline"))
                {
                    RustyLootPlugin.LogDebug($"[ModifyAdrenaline] base:{before:0.#} mod:{modifier:0.#}% => {after:0.#}");
                }
            }
        }
    }
}