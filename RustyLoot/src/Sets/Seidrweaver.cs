using System.Collections.Generic;
using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot;

public static partial class Seidrweaver
{
    public static void Setup()
    {
        var icon = SpriteManager.RegisterSprite("mushroom_big_red.png")!;
        
        var se = ScriptableObject.CreateInstance<SE_Seidrweaver>();
        se.name = "SE_Seidrweaver";
        se.m_name = "$mod_epicloot_siedrweaver";
        se.m_tooltip = "$mod_epicloot_siedrweaver_desc";
        se.m_healthOverTime = 200f;
        se.m_healthOverTimeInterval = 1f;
        se.m_ttl = 120f;
        se.m_icon =  icon;
        se.Register();
        
        EpicLoot.RegisterAsset("Siedrweaver", icon);

        var head = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverHelmet", "$mod_epicloot_siedrweaver_helm", "$mod_epicloot_siedrweaver_helm_desc");
        head.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        head.GuaranteedEffectCount = 6;
        head.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.IncreaseEitr, 15, 20), new GuaranteedMagicEffect(EffectType.ModifyEitrRegen, 10, 20));

        var cape = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverCape", "$mod_epicloot_siedrweaver_cape", "$mod_epicloot_siedrweaver_cape_desc");
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        cape.GuaranteedEffectCount = 6;
        cape.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.AddBloodMagicSkill, 10, 20), new GuaranteedMagicEffect(EffectType.ModifyAttackEitrUse, 10, 20));

        var legs = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverLegs", "$mod_epicloot_siedrweaver_legs", "$mod_epicloot_siedrweaver_legs_desc");
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;
        legs.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyEitrRegenLowHealth, 10, 20), new GuaranteedMagicEffect(EffectType.EitrWeave, 10, 20));
        
        var chest = new LegendaryInfo(LegendaryType.Mythic, "SiedrweaverChest", "$mod_epicloot_siedrweaver_chest", "$mod_epicloot_siedrweaver_chest_desc");
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
        
        AbilityProxyDefinition proxy = new AbilityProxyDefinition("Siedrweaver", AbilityActivationMode.Activated, typeof(SiedrweaverProxy));
        proxy.Ability.Cooldown = 600f;
        proxy.Ability.IconAsset = "Siedrweaver";
        proxy.Register();
        
        MagicItemEffectDefinition effectDef = new MagicItemEffectDefinition("Siedrweaver", "$mod_epicloot_siedrweaver_desc", "$mod_epicloot_siedrweaver_desc");
        effectDef.Requirements.NoRoll = true;
        effectDef.Ability = "Siedrweaver";
        effectDef.Register();
    }
}