using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class ModifyAdrenalineCost
{
    public static void Setup()
    {
        MagicItemEffectDefinition def = new MagicItemEffectDefinition("ModifyAdrenalineCost", "$mod_epicloot_modifyadrenalinecost", "$mod_epicloot_modifyadrenalinecost_desc");
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
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentMaxAdrenaline))]
    private static class Character_GetEquipmentMaxAdrenaline_Patch
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            if (__instance.HasActiveMagicEffect("ModifyAdrenalineCost", out float modifier))
            {
                __result = Mathf.Min(0, __result * Mathf.Clamp01(1f - modifier / 100));
            }
        }
    }
}