using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot;

public class Rejuvenate
{
    public static void Setup()
    {
        var def = new MagicItemEffectDefinition("Rejuvenate", "$mod_epicloot_rejuvenate", "$mod_epicloot_rejuvenate_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.OneHandedWeapon, ItemDrop.ItemData.ItemType.TwoHandedWeapon, ItemDrop.ItemData.ItemType.Bow, ItemDrop.ItemData.ItemType.Shield);
        def.Requirements.AllowedRarities.Add(ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Epic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(10, 15, 1);
        def.Register();
        def.Serialize();
        
        var se = ScriptableObject.CreateInstance<SE_Rejuvenate>();
        se.name = "SE_Rejuvenate";
        se.m_name = "$mod_epicloot_rejuvenate";
        se.m_tooltip = "$mod_epicloot_rejuvenate_desc";
        se.m_ttl = 12f;
        se.Register();
    }

    public class SE_Rejuvenate : SE_Stats
    {
        public override void SetLevel(int itemLevel, float skillLevel)
        {
            m_healthOverTimeInterval = 1f;
            m_healthOverTime = skillLevel;
            m_healthOverTimeDuration = m_ttl;
            m_healthOverTimeTicks = m_healthOverTimeDuration / m_healthOverTimeInterval;
            m_healthOverTimeTickHP = m_healthOverTime / m_healthOverTimeTicks;
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDamaged))]
    private static class Character_OnDamaged_Patch
    {
        private static void Postfix(Character __instance, HitData hit)
        {
            if (!DefinitionExtensions.IsEnabled("Rejuvenate")) return;

            RustyLootPlugin.RustyLootLogger.LogWarning("Rejuvenate");
            if (hit.GetAttacker() is not Player player) return;
            if (player.HasActiveMagicEffect("Rejuvenate", out float modifier))
            {
                float totalDamage = hit.GetTotalDamage();
                float fifteenPercent = totalDamage * 0.15f;
                float random = UnityEngine.Random.value;
                if (Mathf.Clamp01(modifier / 100) > random)
                {
                    player.GetSEMan().AddStatusEffect("SE_Rejuvenate".GetStableHashCode(), true, 0, fifteenPercent);
                    player.m_adrenalinePopEffects.Create(__instance.transform.position, Quaternion.identity);
                    RustyLootPlugin.RustyLootLogger.LogWarning($"Rejuvenate {fifteenPercent}");
                }
            }
        }
    }
}