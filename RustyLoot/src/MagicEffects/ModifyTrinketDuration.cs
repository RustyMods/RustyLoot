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

    [HarmonyPatch(typeof(Player), nameof(Player.AddAdrenaline))]
    private static class Player_AddAdrenaline_Patch
    {
        private static void Postfix(Player __instance)
        {
            if (__instance.HasActiveMagicEffect("ModifyTrinketDuration", out float modifier))
            {
                foreach (Player.StatusEffectLevel adrenalineEffect in __instance.m_adrenalineEffects)
                {
                    StatusEffect? effect = __instance.GetSEMan().GetStatusEffect(adrenalineEffect.m_se.NameHash());
                    if (effect != null)
                    {
                        var originalDuration = effect.m_ttl;
                        effect.m_ttl *= 1 + modifier / 100;
                        RustyLootPlugin.RustyLootLogger.LogWarning($"Modified trinket duration: {effect.name}, {originalDuration} seconds to {effect.m_ttl} seconds");
                    }
                }
            }
        }
    }
}