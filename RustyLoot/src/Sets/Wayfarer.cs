using System.Collections.Generic;
using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot.Sets;

public static class Wayfarer
{
    public static void Setup()
    {
        var head = new LegendaryInfo(LegendaryType.Mythic, "WayfarerHelmet", "$mod_epicloot_wayfarer_helm", "$mod_epicloot_wayfarer_helm_desc");
        head.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        head.GuaranteedEffectCount = 6;

        var chest = new LegendaryInfo(LegendaryType.Mythic, "WayfarerChest", "$mod_epicloot_wayfarer_chest", "$mod_epicloot_wayfarer_chest_desc");
        chest.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        chest.GuaranteedEffectCount = 6;

        var legs = new LegendaryInfo(LegendaryType.Mythic, "WayfarerLegs", "$mod_epicloot_wayfarer_legs", "$mod_epicloot_wayfarer_legs_desc");
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;
        
        var cape = new LegendaryInfo(LegendaryType.Mythic, "WayfarerCape", "$mod_epicloot_wayfarer_cape", "$mod_epicloot_wayfarer_cape_desc");
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        cape.GuaranteedEffectCount = 6;
        
        var set = new MagicSet("Wayfarer", LegendaryType.Mythic);
        set.AddItems(head, chest, legs, cape);
        set.SetBonuses.Add(2, EffectType.AddMovementSkills, 10, 20);
        set.SetBonuses.Add(2, EffectType.AddPhysicalResistancePercentage, 10, 20);
        set.SetBonuses.Add(3, EffectType.ModifyDiscoveryRadius, 10, 25);
        set.SetBonuses.Add(4, "Wayfarer");
        set.Register();
        set.Serialize();
        
        Sprite capeHoodIcon = SpriteManager.RegisterSprite("cape_hood_darkyellow.png")!;
        EpicLoot.RegisterAsset("CapeHood", capeHoodIcon);
        
        AbilityProxyDefinition proxy = new AbilityProxyDefinition("Wayfarer", AbilityActivationMode.Activated, typeof(WayfarerProxy));
        proxy.Ability.Cooldown = 600f;
        proxy.Ability.IconAsset = "CapeHood";
        proxy.Register();
        
        MagicItemEffectDefinition effectDef = new MagicItemEffectDefinition("Wayfarer", "$mod_epicloot_wayfarer_desc", "$mod_epicloot_wayfarer_desc");
        effectDef.Requirements.NoRoll = true;
        effectDef.Ability = "Wayfarer";
        effectDef.Register();
        
        SE_Wayfarer? se = ScriptableObject.CreateInstance<SE_Wayfarer>();
        se.name = "SE_Wayfarer";
        se.m_ttl = 300f;
        se.m_name = "$mod_epicloot_wayfarer";
        se.m_tooltip = "$mod_epicloot_wayfarer_desc";
        se.m_addMaxCarryWeight = 300f;
        se.m_icon = capeHoodIcon;
        se.Register();
    }

    public class WayfarerProxy : Proxy
    {
        public override void Activate()
        {
            if (IsOnCooldown()) return;
            SetCooldownEndTime(GetTime() + Cooldown);
            List<Player> list = new List<Player>();
            Player.GetPlayersInRange(Player.transform.position, 10f, list);
            list.Add(Player);
            foreach (Player? player in list)
            {
                player.GetSEMan().AddStatusEffect("SE_Wayfarer".GetStableHashCode(), true);
            }
        }
    }


    public class SE_Wayfarer : SE_Stats
    {
        public override void Setup(Character character)
        {
            if (character is Player player)
            {
                m_startEffects = player.m_skillLevelupEffects;
            }
            base.Setup(character);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentMovementModifier))]
    private static class Wayfarer_Player_GetEquipmentMovementModifier
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            if (__result > 0) return;
            if (__instance.GetSEMan().HaveStatusEffect("SE_Wayfarer".GetStableHashCode()))
            {
                __result = 0f;
            }
        }
    }
}