using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class ModifyAdrenalineCost
{
    public static void Setup()
    {
        MagicEffect def = new MagicEffect("ModifyAdrenalineCost");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Trinket);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new  ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Epic = new ValueDef(10, 20, 1);
        def.ValuesPerRarity.Legendary =  new ValueDef(20, 30, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(30, 40, 1);
        def.Register();
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentMaxAdrenaline))]
    private static class Character_GetEquipmentMaxAdrenaline_Patch
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            if (!MagicEffect.IsEnabled("ModifyAdrenalineCost")) return;

            if (__instance.HasActiveMagicEffect("ModifyAdrenalineCost", out float modifier, 0.01f))
            {
                float before = __result;

                float reductionMult = Mathf.Clamp(1f - modifier, 0.5f, 1f);
                __result *= reductionMult;

                if (MagicEffect.ShowLogs("ModifyAdrenalineCost"))
                {
                    RustyLootPlugin.LogDebug($"[ModifyAdrenalineCost] base:{before:0.#} mod:{modifier:0.#} mult:{reductionMult:0.###} => {__result:0.#}");
                }
            }
        }
    }
}