using EpicLootAPI;
using HarmonyLib;
using UnityEngine;

namespace RustyLoot;

public static class PiercingShot
{
    public static void Setup()
    {
        MagicEffect def = new MagicEffect("PiercingShot");
        def.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Bows, Skills.SkillType.Crossbows);
        def.Requirements.AllowedRarities.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.Register();
    }
    
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
    private static class Projectile_OnHit_Patch
    {
        private static void Prefix(Projectile __instance)
        {
            if (!MagicEffect.IsEnabled("PiercingShot")) return;

            if (__instance.m_didBounce || __instance.m_owner is not Player player || !EpicLoot.HasActiveMagicEffectOnWeapon(player, __instance.m_weapon, "PiercingShot", out float _)) return;
            if (__instance.m_type is not (ProjectileType.Arrow or ProjectileType.Bolt)) return;

            __instance.m_onHit += (collider, point, water) =>
            {
                if (water) return;

                string normalizedName = __instance.name.Replace("(Clone)", string.Empty);
                GameObject? prefab = ZNetScene.instance.GetPrefab(normalizedName);
                if (prefab == null)
                {
                    RustyLootPlugin.LogWarning($"PiercingShot Failed to find projectile: {normalizedName}");
                    return;
                }
                
                Vector3 velocity = __instance.GetVelocity();
                Vector3 direction = velocity.normalized;
                Vector3 spawnPosition = point + direction * 2f;
                
                Vector3 closest = collider.ClosestPoint(spawnPosition);
                float offset = __instance.m_weapon.m_shared.m_skillType is Skills.SkillType.Crossbows ? 2f : 0.5f;
                spawnPosition = closest + direction * offset;
                
                GameObject projectile = __instance.gameObject;
                GameObject? go = UnityEngine.Object.Instantiate(prefab, spawnPosition, projectile.transform.rotation);
                Projectile? component = go.GetComponent<Projectile>();
                component.m_didBounce = true;
                component.m_doOwnerRaytest = false; // prevent it from changing projectile start point to owner location
                
                component.Setup(__instance.m_owner, velocity, __instance.m_hitNoise, __instance.m_originalHitData, null, null);
            };
        }
    }
}