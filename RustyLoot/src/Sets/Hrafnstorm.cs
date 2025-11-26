using System;
using EpicLootAPI;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RustyLoot.Sets;

public static class Hrafnstorm
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
        chest.IsSetItem = true;
        chest.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        chest.GuaranteedEffectCount = 6;
        
        var legs = new LegendaryInfo(LegendaryType.Legendary, "HunterLegs", "$mod_epicloot_hunterlegs", "$mod_epicloot_hunterlegs_desc");
        legs.IsSetItem = true;
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;

        var cape = new LegendaryInfo(LegendaryType.Legendary, "HunterCape", "$mod_epicloot_huntercape", "$mod_epicloot_huntercape_desc");
        cape.IsSetItem = true;
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        cape.GuaranteedEffectCount = 6;
        
        var helmet = new LegendaryInfo(LegendaryType.Legendary, "HunterHelmet", "$mod_epicloot_hunterhelmet", "$mod_epicloot_hunterhelmet_desc");
        helmet.IsSetItem = true;
        helmet.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        helmet.GuaranteedEffectCount = 6;
        
        var set = new MagicSet("HunterSet", LegendaryType.Mythic);
        set.SetBonuses.Add(2, EffectType.AddBowsSkill, 10, 20);
        set.SetBonuses.Add(3, EffectType.ModifyDodgeStaminaUse, 10, 20);
        set.SetBonuses.Add(4, "ArrowRain");
        set.AddItems(helmet, chest, legs, cape);
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
    
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
    private static class Projectile_OnHit_Patch
    {
        private static void Prefix(Projectile __instance)
        {
            if (__instance.m_owner is not Player player) return;
            if (player != Player.m_localPlayer) return;
            if (__instance.m_type is not ProjectileType.Arrow) return;
            
            __instance.m_onHit += (collider, point, water) =>
            {
                OnHit?.Invoke(__instance, collider, point, water);
            };
        }
    }
    
    public static event Action<Projectile, Collider?, Vector3, bool>? OnHit;

    public class HrafnstormProxy : Proxy
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
            hitData.ApplyModifier(0.5f);

            if (TryTrigger(projectile, hitPoint, hitData))
            {
                SetCooldownEndTime(GetTime() - Cooldown);
            }
        }

        private static bool TryTrigger(GameObject projectile, Vector3 pos, HitData hitData)
        {
            for (int i = 0; i < 10; ++i)
            {
                // Spread radius (5–10 meters)
                float radius = Random.Range(3f, 10f);
                Vector2 offset = Random.insideUnitCircle * radius;

                // Spawn high above target
                Vector3 spawnPos = pos + new Vector3(offset.x, Random.Range(35f, 55f), offset.y);

                // Instantiate projectile
                var go = UnityEngine.Object.Instantiate(projectile.gameObject, spawnPos, Quaternion.identity);
                if (!go.TryGetComponent(out Projectile arrow)) continue;

                // Target is the impact point (arrow falls toward it)
                Vector3 dir = (pos - spawnPos).normalized;

                // Velocity (higher downward bias = stronger rain)
                float speed = Random.Range(25f, 35f);
                Vector3 velocity = dir * speed;

                // Setup projectile
                arrow.Setup(null, velocity, 10f, hitData, null, null);
            }
            return true;
        }
    }
}