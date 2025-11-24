using System;
using System.Collections.Generic;
using EpicLootAPI;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RustyLoot;

public partial class MagicAbilities
{
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (__instance.GetItemPrefab("ArrowWood") is not { } itemPrefab ||
                !itemPrefab.TryGetComponent(out ItemDrop component)) return;
            
            EpicLoot.RegisterAsset("ArrowWood", component.m_itemData.GetIcon());
        }
    }
    
    
    private static void SetupArrowRain()
    {
        var proxy = new AbilityProxyDefinition("ArrowRain", AbilityActivationMode.Triggerable, typeof(ArrowRain));
        proxy.Ability.IconAsset = "ArrowWood";
        proxy.Ability.Cooldown = 10f;
        proxy.Register();

        var def = new MagicItemEffectDefinition("ArrowRain", "$mod_epicloot_arrowrain", "$mod_epicloot_arrowrain_desc");
        def.Requirements.NoRoll = true;
        def.Ability = "ArrowRain";
        def.Register();

        var set = new LegendarySetInfo(LegendaryType.Legendary, "HunterSet", "$mod_epicloot_hunterset");
        set.SetBonuses.Add(2, EffectType.AddBowsSkill, 10, 20);
        set.SetBonuses.Add(3, EffectType.ModifyDodgeStaminaUse, 10, 20);
        set.SetBonuses.Add(4, "ArrowRain");
        set.LegendaryIDs.Add("HunterChest", "HunterLegs", "HunterCape", "HunterHelmet");
        set.Register();
        
        var chest = new LegendaryInfo(LegendaryType.Legendary, "HunterChest", "$mod_epicloot_hunterchest", "$mod_epicloot_hunterchest_desc");
        chest.IsSetItem = true;
        chest.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        chest.GuaranteedEffectCount = 6;
        chest.Register();
        
        var legs = new LegendaryInfo(LegendaryType.Legendary, "HunterLegs", "$mod_epicloot_hunterlegs", "$mod_epicloot_hunterlegs_desc");
        legs.IsSetItem = true;
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;
        legs.Register();

        var cape = new LegendaryInfo(LegendaryType.Legendary, "HunterCape", "$mod_epicloot_huntercape", "$mod_epicloot_huntercape_desc");
        cape.IsSetItem = true;
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        cape.GuaranteedEffectCount = 6;
        cape.Register();
        
        var helmet = new LegendaryInfo(LegendaryType.Legendary, "HunterHelmet", "$mod_epicloot_hunterhelmet", "$mod_epicloot_hunterhelmet_desc");
        helmet.IsSetItem = true;
        helmet.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        helmet.GuaranteedEffectCount = 6;
        helmet.Register();
    }

    // [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
    // private static class Projectile_OnHit_Patch
    // {
    //     private static void Prefix(Projectile __instance)
    //     {
    //         if (__instance.m_owner is not Player player) return;
    //         if (player != Player.m_localPlayer) return;
    //         __instance.m_onHit += (collider, point, water) =>
    //         {
    //             OnHit?.Invoke(__instance, collider, point, water);
    //         };
    //     }
    // }

    public static event Action<Projectile, Collider?, Vector3, bool>? OnHit;

    public class ArrowRain : Proxy
    {
        public override void Initialize(Player player, string id, float cooldown)
        {
            base.Initialize(player, id, cooldown);
            OnHit += OnProjectileHit;
        }

        public override void OnRemoved() => OnHit -= OnProjectileHit;

        private void OnProjectileHit(Projectile source, Collider? collider, Vector3 hitPoint, bool water)
        {
            if (ZNetScene.instance.GetPrefab(source.name.Replace("(Clone)", string.Empty)) is not { } projectile) return;
                
            HitData hitData = new HitData
            {
                m_damage = source.m_originalHitData.m_damage,
                m_pushForce = source.m_originalHitData.m_pushForce,
                m_backstabBonus = source.m_originalHitData.m_backstabBonus,
                m_ranged = true,
                m_hitType = HitData.HitType.PlayerHit
            };
            hitData.SetAttacker(Player);

            if (TryTrigger(projectile, hitPoint, hitData))
            {
                SetCooldownEndTime(GetTime() - Cooldown);
            }
        }

        private static bool TryTrigger(GameObject projectile, Vector3 pos, HitData hitData)
        {
            var characters = new List<Character>();
            Character.GetCharactersInRange(pos, 50f, characters);
            foreach (var character in characters)
            {
                if (character is null || character.IsPlayer() || character.IsTamed()) continue;

                Vector3 spawnPos = character.transform.position + Vector3.up * 50f;
                var go = UnityEngine.Object.Instantiate(projectile.gameObject, spawnPos, Quaternion.identity);
                if (!go.TryGetComponent(out Projectile component)) continue;
                Vector3 targetPos = character.transform.position;
                Vector3 velocity = (targetPos - spawnPos).normalized * Random.Range(10f, 20f);
                component.Setup(null, velocity, 10f, hitData, null, null);
            }
            return true;
        }
    }
}