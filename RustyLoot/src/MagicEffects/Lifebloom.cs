using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class Lifebloom
{
    public static void Setup()
    {
        var def = new MagicItemEffectDefinition("Lifebloom", "$mod_epicloot_lifebloom", "$mod_epicloot_lifeboom_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shield);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary,
            ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Rare = new ValueDef(10, 15, 1);
        def.ValuesPerRarity.Epic = new ValueDef(15, 20, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(20, 25, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(25, 30, 1);
        def.Register();
        def.Serialize();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
    private static class Lifebloom_Player_OnDamaged_Patch
    {
        private static void Postfix(Player __instance, HitData hit)
        {
            if (!DefinitionExtensions.IsEnabled("Lifebloom")) return;

            if (__instance.HasActiveMagicEffect("Lifebloom", out float modifier))
            {
                float totalDamage = hit.GetTotalDamage();
                float fifteen = totalDamage * 0.15f;
                float random = UnityEngine.Random.value;
                if (Mathf.Clamp01(modifier / 100) > random)
                {
                    __instance.GetSEMan().AddStatusEffect("SE_Rejuvenate".GetStableHashCode(), true, 0, fifteen);
                    __instance.m_adrenalinePopEffects.Create(__instance.transform.position, Quaternion.identity);
                    RustyLootPlugin.RustyLootLogger.LogWarning($"Lifebloom {fifteen}");
                }
            }
        }
    }
}