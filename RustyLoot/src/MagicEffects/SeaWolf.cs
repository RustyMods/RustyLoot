using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public class SeaWolf
{
    public static void Setup()
    {
        var def = new MagicItemEffectDefinition("SeaWolf", "$mod_epicloot_seawolf", "$mod_epicloot_seawolf_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(3, 5, 1);
        def.ValuesPerRarity.Epic = new ValueDef(4, 8, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(7, 12, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(10, 15, 1);
        def.Register();
        def.Serialize();
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    private static class Character_RPC_Damage_Patch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (hit.GetAttacker() is Player player && player.HasActiveMagicEffect("SeaWolf", out float modifier) && EnvMan.IsWet())
            {
                hit.ApplyModifier(1 + modifier / 100);
            }
        }
    }
}