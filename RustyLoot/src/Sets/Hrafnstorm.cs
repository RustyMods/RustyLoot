using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static partial class Hrafnstorm
{
    public static void Setup()
    {
        Sprite icon = SpriteManager.RegisterSprite("bow_wood1.png")!;
        EpicLoot.RegisterAsset("HunterBow", icon);
        
        AbilityProxyDefinition proxy = new AbilityProxyDefinition("ArrowRain", AbilityActivationMode.Triggerable, typeof(HrafnstormProxy));
        proxy.Ability.IconAsset = "HunterBow";
        proxy.Ability.Cooldown = 10f;
        proxy.Register();

        MagicEffect def = new MagicEffect("ArrowRain");
        def.Requirements.NoRoll = true;
        def.Ability = "ArrowRain";
        def.Register();

        var chest = new LegendaryInfo(LegendaryType.Legendary, "HunterChest", "$mod_epicloot_hunterchest", "$mod_epicloot_hunterchest_desc");
        chest.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        chest.GuaranteedEffectCount = 6;
        
        var legs = new LegendaryInfo(LegendaryType.Legendary, "HunterLegs", "$mod_epicloot_hunterlegs", "$mod_epicloot_hunterlegs_desc");
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;

        var cape = new LegendaryInfo(LegendaryType.Legendary, "HunterCape", "$mod_epicloot_huntercape", "$mod_epicloot_huntercape_desc");
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        cape.GuaranteedEffectCount = 6;
        
        var helmet = new LegendaryInfo(LegendaryType.Legendary, "HunterHelmet", "$mod_epicloot_hunterhelmet", "$mod_epicloot_hunterhelmet_desc");
        helmet.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        helmet.GuaranteedEffectCount = 6;
        
        var set = new MagicSet("HunterSet", LegendaryType.Mythic);
        set.AddItems(helmet, chest, legs, cape);
        set.SetBonuses.Add(2, EffectType.AddBowsSkill, 10, 20);
        set.SetBonuses.Add(3, EffectType.ModifyDodgeStaminaUse, 10, 20);
        set.SetBonuses.Add(4, "ArrowRain");
        set.Register();
        set.Serialize();

    }
    
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (__instance.GetItemPrefab("ArrowWood") is not { } itemPrefab || !itemPrefab.TryGetComponent(out ItemDrop component)) return;
            
            EpicLoot.RegisterAsset("ArrowWood", component.m_itemData.GetIcon());
        }
    }
}