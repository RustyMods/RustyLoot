using BepInEx.Bootstrap;
using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class Seasonality
{
    public static bool isLoaded;
    public static void Setup()
    {
        isLoaded = Chainloader.PluginInfos.ContainsKey("RustyMods.Seasonality");
        if (!isLoaded) return;
        var def = new MagicEffect("Seasonality");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest, ItemDrop.ItemData.ItemType.Legs, ItemDrop.ItemData.ItemType.Helmet, ItemDrop.ItemData.ItemType.Shoulder);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Epic = new ValueDef(10, 15, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(15, 20, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(20, 25, 1);
        def.Register();
    }

    private enum Season
    {
        Spring, Summer, Fall, Winter
    }

    private static Season GetSeason()
    {
        if (!ZoneSystem.instance) return Season.Summer;
        foreach (string? key in ZoneSystem.instance.GetGlobalKeys())
        {
            if (key.StartsWith("season_"))
            {
                return key switch
                {
                    "season_spring" => Season.Spring,
                    "season_summer" => Season.Summer,
                    "season_fall" => Season.Fall,
                    "season_winter" => Season.Winter,
                    _ => Season.Summer,
                };
            }
        }
        return Season.Summer;
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    private static class EnvMan_IsCold_Patch
    {
        private static void Postfix(ref bool __result)
        {
            if (!MagicEffect.IsEnabled("Seasonality")) return;

            if (!isLoaded) return;
            if (!Player.m_localPlayer || GetSeason() is not Season.Summer || !Player.m_localPlayer.HasActiveMagicEffect("Seasonality", out float _)) return;
            float effectValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue("Seasonality");
            if (effectValue <= 0) return;
            __result = false;
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.RPC_Pick))]
    private static class Pickable_RPC_Pick_Patch
    {
        private static void Prefix(long sender, ref int bonus)
        {
            if (!MagicEffect.IsEnabled("Seasonality")) return;

            if (!isLoaded) return;
            if (!Player.m_localPlayer || Player.m_localPlayer.GetZDOID().UserID != sender) return;
            if (GetSeason() is not Season.Spring) return;
            float effectValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue("Seasonality");
            if (effectValue <= 0) return;
            int random = UnityEngine.Random.Range(0, 100);
            if (random > effectValue) return;
            ++bonus;
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    private static class Character_Damage_Patch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (!MagicEffect.IsEnabled("Seasonality")) return;

            if (!isLoaded) return;
            if (__instance is not Player player || !__instance.m_nview.IsValid() || hit.m_hitType is not HitData.HitType.Fall || !player.HasActiveMagicEffect("Seasonality", out float effectValue, 0.01f) || GetSeason() is not Season.Fall) return;
            hit.ApplyModifier(Mathf.Clamp01(1f - effectValue)); 
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.Internal_AddStatusEffect))]
    private static class SEMan_AddStatusEffect_Patch
    {
        private static bool Prefix(SEMan __instance, int nameHash)
        {
            if (!MagicEffect.IsEnabled("Seasonality")) return true;

            if (!isLoaded) return true;
            if (nameHash != SEMan.s_statusEffectWet || __instance.m_character is not Player player) return true;
            if (!player.HasActiveMagicEffect("Seasonality", out float _) || GetSeason() is not Season.Winter) return true;
            return false;
        }
    }
}