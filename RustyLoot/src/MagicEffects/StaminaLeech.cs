using System.Collections.Generic;
using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class StaminaLeech
{
    public static void Setup()
    {
        var def = new MagicEffect("StaminaLeech");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.TwoHandedWeapon, ItemDrop.ItemData.ItemType.Shield);
        def.Requirements.AllowedRarities.All();
        def.ValuesPerRarity.Magic.Set(1, 2, 0.5f);
        def.ValuesPerRarity.Rare.Set(1, 3, 0.5f);
        def.ValuesPerRarity.Epic.Set(1, 5, 0.5f);
        def.ValuesPerRarity.Legendary.Set(1, 10, 0.5f);
        def.ValuesPerRarity.Mythic.Set(1, 15, 0.5f);
        def.Register();
        
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.AddHitPoint))]
    private static class Attack_AddHitPoint_Patch
    {
        private static float m_lastStaminaLeech;
        
        private static void Postfix(Attack __instance, GameObject go)
        {
            if (!MagicEffect.IsEnabled("StaminaLeech")) return;
            
            if (__instance.m_character is not Player player) return;
            if (go.TryGetComponent(out Character character))
            {
                if (m_lastStaminaLeech + 1f > Time.time) return;
                
                if (player.HasActiveMagicEffect("StaminaLeech", out float modifier, 0.01f))
                {
                    float staminaUse = __instance.GetAttackStamina();
                    float mod = staminaUse * modifier;
                    
                    player.AddStamina(mod);
                    WorldText.instance?.ShowText(character.GetTopPoint(), $"+{mod:0.0} $item_food_stamina", Color.yellow);
                    m_lastStaminaLeech = Time.time;

                    if (MagicEffect.ShowLogs("StaminaLeech"))
                    {
                        RustyLootPlugin.LogDebug($"[StaminaLeech]: attacker: {player.GetPlayerName()}, target:{character.m_name}, staminaUse: {staminaUse}, value:{modifier}({modifier * 100}%), staminaReturn:{mod}");
                    }
                }
            }
        }
    }
}