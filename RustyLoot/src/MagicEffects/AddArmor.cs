using System.Linq;
using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class AddArmor
{
    public static void Setup()
    {
        var def = new MagicItemEffectDefinition("AddArmor", "$mod_epicloot_addarmor", "$mod_epicloot_addarmor_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet, ItemDrop.ItemData.ItemType.Chest,
            ItemDrop.ItemData.ItemType.Legs, ItemDrop.ItemData.ItemType.Shoulder);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary,
            ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 2, 1);
        def.ValuesPerRarity.Rare = new ValueDef(1, 3, 1);
        def.ValuesPerRarity.Epic =  new ValueDef(1, 4, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(1, 6, 1);
        def.Register();
        def.Serialize();
    }


    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetArmor), typeof(int), typeof(float))]
    private static class AddArmor_ItemData_GetAmor_Patch
    {
        [HarmonyPriority(Priority.First)]
        private static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            if (__instance.GetMagicItem() is { } magicItem)
            {
                MagicItemEffect? armor = magicItem.Effects.FirstOrDefault(x => x.EffectType == "AddArmor");
                if (armor == null) return;
                __result += armor.EffectValue;
            }
        }
    }
}