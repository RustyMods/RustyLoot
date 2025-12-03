using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot;

public static class Rejuvenate
{
    public static readonly Sprite icon = SpriteManager.RegisterSprite("rejuvenate.png")!;
    public static void Setup()
    {
        var def = new MagicEffect("Rejuvenate");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.OneHandedWeapon, ItemDrop.ItemData.ItemType.TwoHandedWeapon, ItemDrop.ItemData.ItemType.Bow, ItemDrop.ItemData.ItemType.Shield, ItemDrop.ItemData.ItemType.Shoulder, ItemDrop.ItemData.ItemType.Trinket);
        def.Requirements.AllowedRarities.Add(ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Epic = new ValueDef(1, 5, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(10, 15, 1);
        def.Register();
        
        SE_Rejuvenate? se = ScriptableObject.CreateInstance<SE_Rejuvenate>();
        se.name = "SE_Rejuvenate";
        se.m_name = "$mod_epicloot_rejuvenate";
        se.m_tooltip = "$mod_epicloot_rejuvenate_desc";
        se.m_ttl = 12f;
        se.m_healthOverTime = 5;
        se.m_healthOverTimeInterval = 1f;
        se.m_icon = icon;
        se.Register();
    }
    

    public class SE_Rejuvenate : SE_Stats
    {
        public override void Setup(Character character)
        {
            m_healthOverTime = Mathf.Max(character.GetMaxHealth() * 0.15f, 5f);
            m_name = string.Format(Localization.instance.Localize(m_name), m_healthOverTime).Replace("%", string.Empty);
            base.Setup(character);

            if (MagicEffect.ShowLogs("Rejuvenate"))
            {
                RustyLootPlugin.LogDebug($"[SE_Rejuvenate]: heal amount: {m_healthOverTime}, hp/tick: {m_healthOverTimeTickHP}, duration: {m_healthOverTimeDuration}");
            }
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
                float chance = Mathf.Clamp01(modifier);
                float roll = UnityEngine.Random.value;

                if (MagicEffect.ShowLogs("Rejuvenate"))
                {
                    RustyLootPlugin.LogDebug($"[Rejuvenate] mod:{modifier:0.###}({modifier*100:0.#}%) chance:{chance:0.###} roll:{roll:0.###} trig:{roll <= chance}");
                }
                
                if (chance > roll)
                {
                    player.GetSEMan().AddStatusEffect("SE_Rejuvenate".GetStableHashCode(), true);
                }
            }
        }
    }
}