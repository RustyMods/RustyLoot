using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class Sturdy
{
    public static void Setup()
    {
        var def = new MagicEffect("Sturdy");
        def.Requirements.AllowedRarities.Add(ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Crossbows);
        def.ValuesPerRarity.Magic.Set(1, 1);
        def.ValuesPerRarity.Rare.Set(1, 1);
        def.ValuesPerRarity.Epic.Set(10, 20);
        def.ValuesPerRarity.Legendary.Set(20, 30);
        def.ValuesPerRarity.Mythic.Set(30, 50);
        def.Register();
    }
    
    [HarmonyPatch(typeof(Character), nameof(Character.ApplyPushback), typeof(Vector3), typeof(float))]
    private static class Character_ApplyPushback_Patch
    {
        private static void Prefix(Character __instance, ref float pushForce)
        {
            if (__instance is not Player player) return;

            if (!player.HasActiveMagicEffect("Sturdy", out float modifier, 0.01f)) return;

            float og = pushForce;
            var mod = Mathf.Clamp01(1 - modifier);
            pushForce *= mod;
            
            if (MagicEffect.ShowLogs("Sturdy"))
            {
                RustyLootPlugin.LogDebug(
                    $"[Sturdy]: original: {og}, mod: {mod}({mod * 100}%), pushforce: {pushForce}");
            }
        }
    }
}