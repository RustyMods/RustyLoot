using System.Collections.Generic;
using UnityEngine;
using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;

namespace RustyLoot;

public static partial class Ulfheonar
{
    public static void Setup()
    {
        GameObject? prefab = AssetBundleManager.LoadAsset<GameObject>("ascension_bundle", "vfx_flamethrower_purple");
        prefab!.transform.localScale *= 1.2f;
        PrefabManager.RegisterPrefab(prefab);
        flamethrowerEffects.m_effectPrefabs = new List<EffectList.EffectData>()
        {
            new ()
            {
                m_prefab = prefab,
                m_attach = true,
            }
        }.ToArray();

        Sprite icon = SpriteManager.RegisterSprite("bone_skull.png")!;
        EpicLoot.RegisterAsset("BoneSkull", icon);
        
        SE_Flamethrower? flamethrower = ScriptableObject.CreateInstance<SE_Flamethrower>();
        flamethrower.name = "SE_Flamethrower";
        flamethrower.m_name = "$mod_epicloot_ulfheonar";
        flamethrower.m_tooltip = "$mod_epicloot_ulfheonar_desc";
        flamethrower.m_speedModifier = 0.2f;
        flamethrower.m_percentigeDamageModifiers.m_blunt = 0.5f;
        flamethrower.m_percentigeDamageModifiers.m_pierce = 0.5f;
        flamethrower.m_percentigeDamageModifiers.m_slash = 0.5f;
        flamethrower.m_ttl = 300f;
        flamethrower.m_icon = icon;
        flamethrower.Register();
        
        var head = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarHelmet", "$mod_epicloot_ulfheonar_helm", "$mod_epicloot_ulfheonar_helm_desc");
        head.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        head.GuaranteedEffectCount = 6;
        head.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyDamageLowHealth, 15, 20), new GuaranteedMagicEffect(EffectType.AvoidDamageTakenLowHealth, 10, 20));

        var cape = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarCape", "$mod_epicloot_ulfheonar_cape", "$mod_epicloot_ulfheonar_cape_desc");
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        cape.GuaranteedEffectCount = 6;
        cape.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyPhysicalDamage, 10, 20), new GuaranteedMagicEffect(EffectType.ModifyStaggerDuration, 10, 20));

        var legs = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarLegs", "$mod_epicloot_ulfheonar_legs", "$mod_epicloot_ulfheonar_legs_desc");
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;
        legs.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyMovementSpeedLowHealth, 10, 20), new GuaranteedMagicEffect(EffectType.AvoidDamageTakenLowHealth, 10, 20));
        
        var claws = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarClaws", "$mod_epicloot_ulfheonar_claws", "$mod_epicloot_ulfheonar_claws_desc");
        claws.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.TwoHandedWeapon);
        claws.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Unarmed);
        claws.GuaranteedEffectCount = 6;
        claws.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.AddUnarmedSkill, 10, 20), new GuaranteedMagicEffect(EffectType.ModifyAttackSpeed, 10, 20));

        var set = new MagicSet("Ulfheonar", LegendaryType.Mythic);
        set.AddItems(head, claws, legs, cape);
        set.SetBonuses.Add(2, EffectType.AddMovementSkills, 10, 20);
        set.SetBonuses.Add(2, EffectType.IncreaseHeatResistance, 10, 20);
        set.SetBonuses.Add(3, EffectType.ModifyDamage, 10, 25);
        set.SetBonuses.Add(4, "Ulfheonar");
        set.Register();
        set.Serialize();
        
        AbilityProxyDefinition proxy = new AbilityProxyDefinition("Ulfheonar", AbilityActivationMode.Activated, typeof(UlfheonarProxy));
        proxy.Ability.Cooldown = 600f;
        proxy.Ability.IconAsset = "BoneSkull";
        proxy.Register();
        
        MagicItemEffectDefinition effectDef = new MagicItemEffectDefinition("Ulfheonar", "$mod_epicloot_ulfheonar_desc", "$mod_epicloot_ulfheonar_desc");
        effectDef.Requirements.NoRoll = true;
        effectDef.Ability = "Ulfheonar";
        effectDef.Register();
    }
}