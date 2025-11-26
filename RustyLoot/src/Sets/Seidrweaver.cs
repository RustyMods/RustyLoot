using System.Collections.Generic;
using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot.Sets;

public static class Seidrweaver
{
    public static void Setup()
    {
        var se = ScriptableObject.CreateInstance<SE_Seidrweaver>();
        se.name = "SE_Seidrweaver";
        se.m_name = "$mod_epicloot_seidrweaver";
        se.m_tooltip = "$mod_epicloot_seidrweaver_desc";
        se.m_healthOverTime = 100f;
        se.m_healthOverTimeDuration = 120f;
        se.Register();

        var head = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverHelmet", "$mod_epicloot_seidrweaver_helm", "$mod_epicloot_seidrweaver_helm_desc");
        head.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        head.GuaranteedEffectCount = 6;
        head.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.IncreaseEitr, 15, 20), new GuaranteedMagicEffect(EffectType.ModifyEitrRegen, 10, 20));

        var cape = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverCape", "$mod_epicloot_seidrweaver_cape", "$mod_epicloot_seidrweaver_cape_desc");
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        cape.GuaranteedEffectCount = 6;
        cape.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.AddBloodMagicSkill, 10, 20), new GuaranteedMagicEffect(EffectType.ModifyAttackEitrUse, 10, 20));

        var legs = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverLegs", "$mod_epicloot_seidrweaver_legs", "$mod_epicloot_seidrweaver_legs_desc");
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;
        legs.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyEitrRegenLowHealth, 10, 20), new GuaranteedMagicEffect(EffectType.EitrWeave, 10, 20));
        
        var chest = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverChest", "$mod_epicloot_seidrweaver_chest", "$mod_epicloot_seidrweaver_chest_desc");
        chest.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        chest.GuaranteedEffectCount = 6;
        chest.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyElementalDamage, 10, 20), new GuaranteedMagicEffect(EffectType.AddElementalResistancePercentage, 10, 20));

        var set = new MagicSet("Siedrweaver", LegendaryType.Mythic);
        set.AddItems(head, chest, legs, cape);
        set.SetBonuses.Add(2, EffectType.IncreaseEitr, 10, 20);
        set.SetBonuses.Add(2, EffectType.ModifyEitrRegen, 10, 20);
        set.SetBonuses.Add(3, EffectType.AddBloodMagicSkill, 10, 25);
        set.SetBonuses.Add(4, "Siedrweaver");
        set.Register();
        set.Serialize();
        
        AbilityProxyDefinition proxy = new AbilityProxyDefinition("Siedrweave", AbilityActivationMode.Activated, typeof(SeidrweaverProxy));
        proxy.Ability.Cooldown = 600f;
        proxy.Ability.IconAsset = "BoneSkull";
        proxy.Register();
        
        MagicItemEffectDefinition effectDef = new MagicItemEffectDefinition("Siedrweave", "$mod_epicloot_siedrweave_desc", "$mod_epicloot_siedrweave_desc");
        effectDef.Requirements.NoRoll = true;
        effectDef.Ability = "Siedrweave";
        effectDef.Register();
    }

    public class SE_Seidrweaver : SE_Stats
    {
        
    }
    
    public static EffectListRef healEffect = new("fx_DvergerMage_Support_start");


    public class SeidrweaverProxy : Proxy
    {
        public override void Activate()
        {
            if (!Player.HaveEitr(30))
            {
                Hud.instance.EitrBarEmptyFlash();
                return;
            }
            if (IsOnCooldown()) return;
            SetCooldownEndTime(GetTime() + Cooldown);
            List<Player> list = new List<Player>();
            Player.GetPlayersInRange(Player.transform.position, 50f, list);
            list.Add(Player);
            foreach (Player? player in list)
            {
                player.GetSEMan().AddStatusEffect("SE_Seidrweaver".GetStableHashCode(), true);
            }

            healEffect.Create(Player.transform.position, Quaternion.identity);
        }
    }
}