using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class Honeybound
{
    public static void Setup()
    {
        var def = new MagicEffect("Honeybound");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet, ItemDrop.ItemData.ItemType.Chest, ItemDrop.ItemData.ItemType.Legs, ItemDrop.ItemData.ItemType.Shoulder);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 3, 1);
        def.ValuesPerRarity.Rare = new ValueDef(2, 4, 1);
        def.ValuesPerRarity.Epic = new ValueDef(3, 5, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(4, 6, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(7, 10, 1);
        def.Register();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
    private static class Player_EatFood_Patch
    {
        private static void Postfix(Player __instance, ItemDrop.ItemData item, bool __result)
        {
            if (!MagicEffect.IsEnabled("Honeybound")) return;

            if (!__result) return;
            
            if (__instance.HasActiveMagicEffect("Honeybound", out float modifier))
            {
                foreach (Player.Food? food in __instance.m_foods)
                {
                    if (food.m_item.m_shared.m_name != item.m_shared.m_name) continue;
                    float before = food.m_time;
                    food.m_time *= 1 + modifier / 100f;
                    float after = food.m_time;

                    if (MagicEffect.ShowLogs("Honeybound"))
                    {
                        RustyLootPlugin.LogDebug($"[Honeybound] food:{item.m_shared.m_name} time {before:0.#} +{(after-before):0.#} => {after:0.#} ({modifier:0.#}%)");
                    }
                    return;
                }    
            }
        }
    }
}