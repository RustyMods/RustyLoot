using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class ModifyTrinketDuration
{
    public static void Setup()
    {
        MagicItemEffectDefinition def = new MagicItemEffectDefinition("ModifyTrinketDuration", "$mod_epicloot_modifytrinketduration", "$mod_epicloot_modifytrinketduration_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Trinket);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new  ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Epic = new ValueDef(10, 20, 1);
        def.ValuesPerRarity.Legendary =  new ValueDef(20, 30, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(30, 40, 1);
        def.Register();
        def.Serialize();
    }

    [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.Setup))]
    private static class StatusEffect_Setup_Patch
    {
        private static void Postfix(StatusEffect __instance)
        {
            if (!DefinitionExtensions.IsEnabled("ModifyTrinketDuration")) return;

            if (__instance.m_character is not Player player) return;
            if (player.HasActiveMagicEffect("ModifyTrinketDuration", out float modifier))
            {
                if (player.m_trinketItem is { m_equipped: true } trinket)
                {
                    if (trinket.m_shared.m_fullAdrenalineSE is { } adrenalineSE)
                    {
                        if (__instance.NameHash() == adrenalineSE.NameHash())
                        {
                            float originalDuration = __instance.m_ttl;
                            __instance.m_ttl *= 1 + modifier / 100;
                            RustyLootPlugin.RustyLootLogger.LogWarning($"Modified trinket duration: {__instance.name}, {originalDuration} seconds to {__instance.m_ttl} seconds");
                        }
                    }
                }
            }
        }
    }
}