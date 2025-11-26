using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class Lifebloom
{
    public static void Setup()
    {
        var def = new MagicEffect("Lifebloom");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shield);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary,
            ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Rare = new ValueDef(10, 15, 1);
        def.ValuesPerRarity.Epic = new ValueDef(15, 20, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(20, 25, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(25, 30, 1);
        def.Register();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
    private static class Lifebloom_Player_OnDamaged_Patch
    {
        private static void Postfix(Player __instance, HitData hit)
        {
            if (!MagicEffect.IsEnabled("Lifebloom")) return;

            if (__instance.HasActiveMagicEffect("Lifebloom", out float modifier, 0.01f))
            {
                float dmg = hit.GetTotalDamage();
                float heal = dmg * 0.15f;

                float chance = Mathf.Clamp01(modifier);
                float roll = UnityEngine.Random.value;
                bool trig = chance > roll;

                if (trig)
                {
                    __instance.GetSEMan().AddStatusEffect("SE_Rejuvenate".GetStableHashCode(), true, 0, heal);
                    __instance.m_adrenalinePopEffects.Create(__instance.transform.position, Quaternion.identity);
                }

                if (MagicEffect.ShowLogs("Lifebloom"))
                {
                    RustyLootPlugin.LogDebug(
                        $"[Lifebloom] dmg:{dmg:0.#} heal:{heal:0.#} chance:{chance:0.###} roll:{roll:0.###} trig:{trig}"
                    );
                }
            }
        }
    }
}