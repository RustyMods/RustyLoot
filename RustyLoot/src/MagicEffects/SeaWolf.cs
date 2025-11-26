using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class SeaWolf
{
    public static void Setup()
    {
        var def = new MagicEffect("SeaWolf");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Rare = new ValueDef(3, 5, 1);
        def.ValuesPerRarity.Epic = new ValueDef(4, 8, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(7, 12, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(10, 15, 1);
        def.Register();
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    private static class Character_RPC_Damage_Patch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (!MagicEffect.IsEnabled("SeaWolf")) return;
            if (!__instance.m_nview.IsOwner() || hit.GetAttacker() is not Player player) return;
            if (player.HasActiveMagicEffect("SeaWolf", out float modifier, 0.01f) && player.GetSEMan().HaveStatusEffect(SEMan.s_statusEffectWet))
            {
                float baseDamage = hit.GetTotalDamage();
                float mod = 1 + modifier;
                hit.ApplyModifier(mod);

                if (MagicEffect.ShowLogs("SeaWolf"))
                {
                    RustyLootPlugin.LogDebug(
                        $"[SeaWolf] attacker:{player.GetPlayerName()} target:{__instance.m_name} base:{baseDamage:0.#} mod:{modifier:0.#}x => final:{hit.GetTotalDamage():0.#}"
                    );
                }
            }
        }
    }
}