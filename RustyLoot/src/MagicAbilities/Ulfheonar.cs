using System.Collections.Generic;
using EpicLootAPI;
using UnityEngine;

namespace RustyLoot;

public static partial class Ulfheonar
{
    private static readonly EffectList flamethrowerEffects = new EffectList();

    public class UlfheonarProxy : Proxy
    {
        public override void Activate()
        {
            if (IsOnCooldown()) return;
            SetCooldownEndTime(GetTime() + Cooldown);
            List<Player> list = new List<Player>();
            Player.GetPlayersInRange(Player.transform.position, 50f, list);
            list.Add(Player);
            foreach (Player? player in list)
            {
                player.GetSEMan().AddStatusEffect("SE_Flamethrower".GetStableHashCode(), true);
            }
        }
    }

    public class SE_Flamethrower : SE_Stats
    {
        public float damageModifier = 1.1f;
        
        public override void Setup(Character character)
        {
            base.Setup(character);
            Trigger();
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            hit.ApplyModifier(damageModifier);
        }

        public override string GetTooltipString()
        {
            string tooltip = base.GetTooltipString();
            tooltip += $"\n$mod_epicloot_damage_taken: <color=orange>+{(damageModifier - 1) * 100}</color>%";
            return tooltip;
        }

        public void Trigger()
        {
            Vector3 head = m_character.GetHeadPoint();
            Quaternion lookYaw = m_character.GetLookYaw();
            Vector3 forward = lookYaw * Vector3.forward;

            HitData hitData = new HitData();
            hitData.SetAttacker(m_character);
            hitData.m_damage.m_fire = 50f;
            hitData.m_damage.m_chop = 20f;
            
            flamethrowerEffects.Create(head, lookYaw, m_character.m_head);

            Collider[] results = new Collider[10];
            int size = Physics.OverlapSphereNonAlloc(head + forward * 5f, 3f, results);
            for (int index = 0; index < size; index++)
            {
                Collider collider = results[index];
                IDestructible? destructible = collider.GetComponentInParent<IDestructible>();
                if (destructible is null) continue;
                Vector3 diff = (collider.transform.position - head).normalized;
                float angle = Vector3.Angle(forward, diff);

                if (angle <= 45f)
                {
                    float distance = Vector3.Distance(head, collider.transform.position);
                    if (distance <= 10f)
                    {
                        destructible.Damage(hitData);
                    }
                }
            }
        }
    }
}