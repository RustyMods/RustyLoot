using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot;

public static class Bleed
{
    public static void Setup()
    {
        var se = ScriptableObject.CreateInstance<SE_Bleed>();
        se.name = "SE_Bleed";
        se.m_name = "$mod_epicloot_bleed";
        se.m_tooltip = "$mod_epicloot_bleed_desc";
        se.damagePerTick = 1f;
        se.maxDamagePerTick = 10f;
        se.m_ttl = 10f;
        se.Register();
        
        var def = new MagicEffect("Bleed");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.OneHandedWeapon, ItemDrop.ItemData.ItemType.TwoHandedWeapon, ItemDrop.ItemData.ItemType.Bow, ItemDrop.ItemData.ItemType.Torch, ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Magic = new ValueDef(5, 8, 1);
        def.ValuesPerRarity.Rare = new ValueDef(7, 12, 1);
        def.ValuesPerRarity.Epic = new ValueDef(11, 15, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(14, 19, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(18, 25, 1);
        def.Register();
    }

    public class SE_Bleed : StatusEffect
    {
        public float damageTimer;
        public float damagePerTick = 1f;
        public float maxDamagePerTick = 10f;

        public Character? attacker;

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);

            if (damagePerTick <= 0) return;
            
            damageTimer += dt;
            if (damageTimer < 1f) return;
            damageTimer = 0.0f;

            HitData hit = new HitData();
            hit.m_hitType = HitData.HitType.Self;
            if (attacker != null) hit.SetAttacker(attacker);
            hit.m_damage.m_damage = damagePerTick;
            m_character.Damage(hit);
            WorldText.instance?.ShowText(m_character.GetTopPoint(), $"{damagePerTick} $mod_epicloot_bleed_msg", Color.red);
            damagePerTick--;
            if (damagePerTick <= 0)
            {
                m_ttl = 0;
            }
        }

        public void Increment()
        {
            damagePerTick = Mathf.Max(damagePerTick + 1, maxDamagePerTick);
            ResetTime();
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    private static class Bleed_Character_RPC_Damage
    {
        private static void Postfix(Character __instance, HitData hit)
        {
            if (!MagicEffect.IsEnabled("Bleed")) return;
            if (hit.GetAttacker() is not Player player || hit.m_hitType is HitData.HitType.Self) return;
            if (!player.HasActiveMagicEffect("Bleed", out float modifier, 0.01f)) return;
            
            float chance = Mathf.Clamp01(modifier);
            float roll = UnityEngine.Random.value;
            bool trigger = chance > roll;

            if (trigger)
            {
                if (__instance.GetSEMan().GetStatusEffect("SE_Bleed".GetStableHashCode()) is SE_Bleed bleed)
                {
                    bleed.Increment();
                    bleed.attacker = player;
                }
                else
                {
                    bleed = (SE_Bleed)__instance.GetSEMan().AddStatusEffect("SE_Bleed".GetStableHashCode());
                    bleed.attacker = player;
                }
            }

            if (MagicEffect.ShowLogs("Bleed"))
            {
                RustyLootPlugin.LogDebug($"[Bleed]: attacker: {player.GetPlayerName()}, chance: {chance}({chance * 100}), roll: {roll}, trigger: {trigger}");
            }

        }
    }
    
}