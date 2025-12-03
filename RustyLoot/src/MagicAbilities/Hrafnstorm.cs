using System;
using EpicLootAPI;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RustyLoot;

public static partial class Hrafnstorm
{
    public static event Action<Projectile, Collider?, Vector3, bool>? OnHit;

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
                float radius = Random.Range(3f, 10f);
                Vector2 offset = Random.insideUnitCircle * radius;

                Vector3 spawnPos = pos + new Vector3(offset.x, Random.Range(35f, 55f), offset.y);

                var go = UnityEngine.Object.Instantiate(projectile.gameObject, spawnPos, Quaternion.identity);
                if (!go.TryGetComponent(out Projectile arrow)) continue;

                Vector3 dir = (pos - spawnPos).normalized;

                float speed = Random.Range(25f, 35f);
                Vector3 velocity = dir * speed;

                arrow.Setup(null, velocity, 10f, hitData, null, null);
            }
            return true;
        }
    }
}