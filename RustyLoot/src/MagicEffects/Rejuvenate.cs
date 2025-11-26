using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot;

public class Rejuvenate
{
    public static void Setup()
    {
        var def = new MagicEffect("Rejuvenate");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.OneHandedWeapon, ItemDrop.ItemData.ItemType.TwoHandedWeapon, ItemDrop.ItemData.ItemType.Bow, ItemDrop.ItemData.ItemType.Shield, ItemDrop.ItemData.ItemType.Shoulder, ItemDrop.ItemData.ItemType.Trinket);
        def.Requirements.AllowedRarities.Add(ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Epic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(10, 15, 1);
        def.Register();

        var icon = SpriteManager.RegisterSprite("rejuvenate.png")!;
        
        var se = ScriptableObject.CreateInstance<SE_Rejuvenate>();
        se.name = "SE_Rejuvenate";
        se.m_name = "$mod_epicloot_rejuvenate";
        se.m_tooltip = "$mod_epicloot_rejuvenate_desc";
        se.m_ttl = 12f;
        se.m_icon = icon;
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
            
            m_name = string.Format(Localization.instance.Localize(m_name), skillLevel).Replace("%", string.Empty);
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    private static class Character_RPC_Damage
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (!MagicEffect.IsEnabled("Rejuvenate")) return;

            if (!__instance.m_nview.IsOwner() || hit.GetAttacker() is not Player player) return;
            
            if (player.HasActiveMagicEffect("Rejuvenate", out float modifier, 0.01f))
            {
                float totalDamage = hit.GetTotalDamage();
                float healAmount = totalDamage * 0.15f;
                float chance = Mathf.Clamp01(modifier);
                float roll = UnityEngine.Random.value;

                if (MagicEffect.ShowLogs("Rejuvenate"))
                {
                    RustyLootPlugin.LogDebug(
                        $"[Rejuvenate] dmg:{totalDamage:0.#} heal:{healAmount:0.#} mod:{modifier:0.###}({modifier*100:0.#}%) " +
                        $"chance:{chance:0.###} roll:{roll:0.###} trig:{roll <= chance}"
                    );
                }
                
                if (chance > roll)
                {
                    player.GetSEMan().AddStatusEffect("SE_Rejuvenate".GetStableHashCode(), true, 0, healAmount);
                    player.m_adrenalinePopEffects.Create(__instance.transform.position, Quaternion.identity);
                }
            }
        }
    }
}