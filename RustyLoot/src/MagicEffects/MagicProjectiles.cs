using System.Collections.Generic;
using BepInEx.Configuration;
using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class MagicProjectiles
{
    private static ItemDrop? arrow;
    private static ItemDrop? bolt;

    public static void SetupMagicArrow()
    {
        Clone magicArrow = new Clone("ArrowFrost", "RS_MagicArrow");
        magicArrow.OnCreated += prefab =>
        {
            if (!prefab.TryGetComponent(out ItemDrop component)) return;
            component.m_itemData.m_shared.m_name = "$item_arrow_magic";
            component.m_itemData.m_shared.m_description = "$item_arrow_magic_desc";

            var projectile = component.m_itemData.m_shared.m_attack.m_attackProjectile;
            if (projectile == null)
            {
                Debug.LogError("Magic Arrow projectile is null");
                return;
            }

            var newProjectile = Object.Instantiate(projectile, RustyLootPlugin.root.transform, false);
            newProjectile.name = "bow_projectile_magic";
            
            var model = newProjectile.transform.Find("model");
            if (model == null)
            {
                Debug.LogError("Magic Arrow model is null");
                return;
            }

            var renderer = model.GetComponent<MeshRenderer>();
            List<Material> newMaterials = new();
            foreach (var material in renderer.sharedMaterials)
            {
                var newMat = new Material(material);
                newMat.name = material.name + "_magic_arrow";
                var baseColor = material.GetColor("_EmissionColor");
                newMat.SetColor("_EmissionColor", baseColor * 5f);
                newMaterials.Add(newMat);
            }
            renderer.sharedMaterials = newMaterials.ToArray();
            renderer.materials = newMaterials.ToArray();


            var tip = newProjectile.transform.Find("Sphere");
            if (tip == null)
            {
                Debug.LogError("Magic arrow tip is null");
                return;
            }

            var tipRenderer = tip.GetComponent<MeshRenderer>();
            tipRenderer.sharedMaterials = newMaterials.ToArray();
            tipRenderer.materials = newMaterials.ToArray();
            
            component.m_itemData.m_shared.m_attack.m_attackProjectile = newProjectile;
            PrefabManager.RegisterPrefab(newProjectile);
            
            arrow = component;
        };
        
        var def = new MagicItemEffectDefinition("MagicArrow", "$mod_epicloot_magicarrow", "$mod_epicloot_magicarrow_desc");
        def.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Bows);
        def.Requirements.AllowedRarities.Add(ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.Register();
        def.Serialize();

        var pierceDmg = RustyLootPlugin.config("Magic Arrow", "Pierce Damage", 26f, "Set magic arrow pierce damage");
        var frostDmg = RustyLootPlugin.config("Magic Arrow", "Frost Damage", 52f, "Set magic arrow frost damage");
        arrowEitrUse = RustyLootPlugin.config("Magic Arrow", "Eitr Use", 10f, "Set magic arrow eitr cost");

        pierceDmg.SettingChanged += (sender, args) => OnConfigChange();
        frostDmg.SettingChanged +=  (sender, args) => OnConfigChange();

        void OnConfigChange()
        {
            if (arrow == null) return;
            arrow.m_itemData.m_shared.m_damages.m_pierce = pierceDmg.Value;
            arrow.m_itemData.m_shared.m_damages.m_frost = frostDmg.Value;
        }
        
        OnConfigChange();
    }

    public static void SetupMagicBolt()
    {
        Clone magicArrow = new Clone("BoltCarapace", "RS_MagicBolt");
        magicArrow.OnCreated += prefab =>
        {
            if (!prefab.TryGetComponent(out ItemDrop component)) return;
            component.m_itemData.m_shared.m_name = "$item_bolt_magic";
            component.m_itemData.m_shared.m_description = "$item_bolt_magic_desc";

            GameObject? projectile = component.m_itemData.m_shared.m_attack.m_attackProjectile;
            if (projectile == null)
            {
                Debug.LogError("Magic Bolt projectile is null");
                return;
            }

            GameObject? newProjectile = Object.Instantiate(projectile, RustyLootPlugin.root.transform, false);
            newProjectile.name = "arbalest_projectile_magic";
            
            Transform? model = newProjectile.transform.Find("default");
            if (model == null)
            {
                Debug.LogError("Magic Bolt model is null");
                return;
            }

            MeshRenderer? renderer = model.GetComponent<MeshRenderer>();
            List<Material> newMaterials = new();
            foreach (var material in renderer.sharedMaterials)
            {
                Material newMat = new Material(material);
                newMat.name = material.name + "_magic_arrow";
                newMat.EnableKeyword("_EMISSION");
                newMat.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * 5f);
                newMaterials.Add(newMat);
            }
            renderer.sharedMaterials = newMaterials.ToArray();
            renderer.materials = newMaterials.ToArray();
            
            component.m_itemData.m_shared.m_attack.m_attackProjectile = newProjectile;
            PrefabManager.RegisterPrefab(newProjectile);
            bolt = component;
        };
        
        var def = new MagicItemEffectDefinition("MagicBolt", "$mod_epicloot_magicbolt", "$mod_epicloot_magicbolt_desc");
        def.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Crossbows);
        def.Requirements.AllowedRarities.Add(ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.Register();
        def.Serialize();
        
        var pierceDmg = RustyLootPlugin.config("Magic Bolt", "Pierce Damage", 26f, "Set magic arrow pierce damage");
        var frostDmg = RustyLootPlugin.config("Magic Bolt", "Frost Damage", 52f, "Set magic arrow frost damage");
        boltEitrUse = RustyLootPlugin.config("Magic Bolt", "Eitr Use", 10f, "Set magic arrow eitr cost");

        pierceDmg.SettingChanged += (sender, args) => OnConfigChange();
        frostDmg.SettingChanged +=  (sender, args) => OnConfigChange();

        void OnConfigChange()
        {
            if (bolt == null) return;
            bolt.m_itemData.m_shared.m_damages.m_pierce = pierceDmg.Value;
            bolt.m_itemData.m_shared.m_damages.m_frost = frostDmg.Value;
        }

        OnConfigChange();
    }

    private static ConfigEntry<float>? arrowEitrUse;
    private static float arrowEitrCost => arrowEitrUse?.Value ?? 10f;
    private static ConfigEntry<float>? boltEitrUse;
    private static float boltEitrCost => boltEitrUse?.Value ?? 10f;

    [HarmonyPatch(typeof(Attack), nameof(Attack.HaveAmmo))]
    private static class Attack_HaveAmmo_Patch
    {
        private static bool Prefix(Humanoid character, ItemDrop.ItemData weapon, ref bool __result)
        {
            if (character is not Player player) return true;
            if (weapon.m_shared.m_skillType is not (Skills.SkillType.Bows or Skills.SkillType.Crossbows)) return true;
            if (string.IsNullOrEmpty(weapon.m_shared.m_ammoType)) return true;
            if (player.GetAmmoItem() is { } ammoItem && player.GetInventory().ContainsItem(ammoItem) && ammoItem.m_shared.m_ammoType == weapon.m_shared.m_ammoType) return true;

            if (arrow != null && weapon.m_shared.m_ammoType == arrow.m_itemData.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, weapon, "MagicArrow", out float _) || !player.HaveEitr(arrowEitrCost)) return true;
            }
            else if (bolt != null && weapon.m_shared.m_ammoType == bolt.m_itemData.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, weapon, "MagicBolt", out float _) || !player.HaveEitr(boltEitrCost)) return true;
            }
            else
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.FindAmmo))]
    private static class Attack_FindAmmo_Patch
    {
        private static void Postfix(Humanoid character, ItemDrop.ItemData weapon, ref ItemDrop.ItemData? __result)
        {
            if (!DefinitionExtensions.IsEnabled("MagicArrow") && !DefinitionExtensions.IsEnabled("MagicBolt")) return;

            if (__result != null || character is not Player player) return;
            if (weapon.m_shared.m_skillType is not (Skills.SkillType.Bows or Skills.SkillType.Crossbows)) return;

            if (arrow != null && arrow.m_itemData.m_shared.m_ammoType == weapon.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, weapon, "MagicArrow", out float _) || !player.HaveEitr(arrowEitrCost)) return;
                __result = arrow.m_itemData.Clone();
            }
            else if (bolt != null && bolt.m_itemData.m_shared.m_ammoType == weapon.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, weapon, "MagicBolt", out float _) || !player.HaveEitr(boltEitrCost)) return;
                __result = bolt.m_itemData.Clone();
            }
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.UseAmmo))]
    private static class Attack_UseAmmo_Patch
    {
        private static bool Prefix(Attack __instance, ref bool __result)
        {
            if (!DefinitionExtensions.IsEnabled("MagicArrow") && !DefinitionExtensions.IsEnabled("MagicBolt")) return true;

            if (__instance.m_character is not Player player) return true;
            if (player.GetAmmoItem() is {} ammo && ammo.m_shared.m_ammoType == __instance.m_weapon.m_shared.m_ammoType) return true;

            if (arrow != null && __instance.m_weapon.m_shared.m_ammoType == arrow.m_itemData.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, __instance.m_weapon, "MagicArrow", out float _) || !player.HaveEitr(arrowEitrCost)) return true;

                __result = true;
                __instance.m_ammoItem = arrow.m_itemData.Clone();
                player.UseEitr(arrowEitrCost);
            }            
            else if (bolt != null && __instance.m_weapon.m_shared.m_ammoType == bolt.m_itemData.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, __instance.m_weapon, "MagicBolt", out float _) ||
                    !player.HaveEitr(boltEitrCost)) return true;

                __result = true;
                __instance.m_ammoItem = bolt.m_itemData.Clone();
                player.UseEitr(boltEitrCost);
            }
            else return true;
            
            return false;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.EquipAmmoItem))]
    private static class Attack_EquipAmmoItem_Patch
    {
        private static bool Prefix(Humanoid character, ItemDrop.ItemData weapon, ref bool __result)
        {
            if (!DefinitionExtensions.IsEnabled("MagicArrow") && !DefinitionExtensions.IsEnabled("MagicBolt")) return true;

            if (character is not Player player) return true;

            if (player.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType) is {} ammo && ammo.m_shared.m_ammoType == weapon.m_shared.m_ammoType) return true;
            
            if (arrow != null && weapon.m_shared.m_ammoType == arrow.m_itemData.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, weapon, "MagicArrow", out float _) || !player.HaveEitr(arrowEitrCost)) return true;
            }
            else if (bolt != null && weapon.m_shared.m_ammoType == bolt.m_itemData.m_shared.m_ammoType)
            {
                if (!EpicLoot.HasActiveMagicEffectOnWeapon(player, weapon, "MagicBolt", out float _) || !player.HaveEitr(boltEitrCost)) return true;
            }
            else return true;
            
            __result = true;
            return false;
        }
    }
}