using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class ForsakenBlow
{
    public static void Setup()
    {
        var def = new MagicEffect("ForsakenBlow");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shield);
        def.Requirements.AllowedRarities.All();
        def.ValuesPerRarity.Magic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(3, 8, 1);
        def.ValuesPerRarity.Epic = new ValueDef(6, 12, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(10, 15, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(13, 20, 1);
        def.Register();
    }

    [HarmonyPatch(typeof(Character), nameof(Character.BlockAttack))]
    private static class ForsakenBlow_Character_BlockAttack_Patch
    {
        private static void Postfix(Character __instance)
        {
            if (!MagicEffect.IsEnabled("ForsakenBlow")) return;
            if (__instance is not Player player) return;
            if (player.HasActiveMagicEffect("ForsakenBlow", out float modifier))
            {
                float before = player.m_guardianPowerCooldown;

                player.m_guardianPowerCooldown -= modifier;

                float after = player.m_guardianPowerCooldown;

                if (MagicEffect.ShowLogs("ForsakenBlow")) 
                {
                    RustyLootPlugin.LogDebug($"[ForsakenBlow] block: cooldown {before:0.#} -{modifier:0.#} => {after:0.#}");
                }            
            }
        }
    }
}