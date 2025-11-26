using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class ModifyTrinketDuration
{
    public static void Setup()
    {
        MagicEffect def = new MagicEffect("ModifyTrinketDuration");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Trinket);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new  ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Epic = new ValueDef(10, 20, 1);
        def.ValuesPerRarity.Legendary =  new ValueDef(20, 30, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(30, 40, 1);
        def.Register();
    }

    [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.Setup))]
    private static class StatusEffect_Setup_Patch
    {
        private static void Postfix(StatusEffect __instance)
        {
            if (!MagicEffect.IsEnabled("ModifyTrinketDuration")) return;

            if (__instance.m_character is not Player player) return;
            if (player.HasActiveMagicEffect("ModifyTrinketDuration", out float modifier, 0.01f))
            {
                if (player.m_trinketItem is { m_equipped: true } trinket && trinket.m_shared.m_fullAdrenalineSE is { } adrenalineSE && __instance.NameHash() == adrenalineSE.NameHash())
                {
                    float before = __instance.m_ttl;
                    __instance.m_ttl *= 1 + modifier;
                    float after = __instance.m_ttl;

                    if (MagicEffect.ShowLogs("ModifyTrinketDuration"))
                    {
                        RustyLootPlugin.LogDebug($"[ModifyTrinketDuration] {__instance.name}: {before:0.#}s +{(after-before):0.#}s => {after:0.#}s ({modifier:0.#}%)");
                    }
                }
            }
        }
    }
}