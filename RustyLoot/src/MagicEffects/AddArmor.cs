using System.Linq;
using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class AddArmor
{
    private static MagicEffect effect = null!;
    public static void Setup()
    {
        effect = new MagicEffect("AddArmor");
        effect.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet, ItemDrop.ItemData.ItemType.Chest, ItemDrop.ItemData.ItemType.Legs, ItemDrop.ItemData.ItemType.Shoulder);
        effect.Requirements.AllowedRarities.All();
        effect.ValuesPerRarity.Magic.Set(1, 2, 1);
        effect.ValuesPerRarity.Rare = new ValueDef(1, 3, 1);
        effect.ValuesPerRarity.Epic =  new ValueDef(1, 4, 1);
        effect.ValuesPerRarity.Legendary = new ValueDef(1, 5, 1);
        effect.ValuesPerRarity.Mythic = new ValueDef(1, 6, 1);
        effect.Register();
    }


    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetArmor), typeof(int), typeof(float))]
    private static class AddArmor_ItemData_GetAmor_Patch
    {
        [HarmonyPriority(Priority.First)]
        private static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            if (!effect.IsEnabled()) return;
            if (__instance.GetMagicItem() is { } magicItem)
            {
                MagicItemEffect? armor = magicItem.Effects.FirstOrDefault(x => x.EffectType == "AddArmor");
                if (armor == null) return;
                
                float add = armor.EffectValue;
                float before = __result;

                __result += add;

                if (effect.ShowLogs())
                {
                    RustyLootPlugin.LogDebug($"[AddArmor] item:{__instance.m_shared.m_name} base:{before:0.#} +{add:0.#} => {__result:0.#}");
                }
            }
        }
    }
}