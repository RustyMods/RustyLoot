using System.Collections.Generic;
using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static partial class Wayfarer
{
    public class WayfarerProxy : Proxy
    {
        public override void Activate()
        {
            if (IsOnCooldown()) return;
            SetCooldownEndTime(GetTime() + Cooldown);
            List<Player> list = new List<Player>();
            Player.GetPlayersInRange(Player.transform.position, 10f, list);
            list.Add(Player);
            foreach (Player? player in list)
            {
                player.GetSEMan().AddStatusEffect("SE_Wayfarer".GetStableHashCode(), true);
            }
        }
    }

    public class SE_Wayfarer : SE_Stats
    {
        public override void Setup(Character character)
        {
            if (character is Player player)
            {
                m_startEffects = player.m_skillLevelupEffects;
            }
            base.Setup(character);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentMovementModifier))]
    private static class Wayfarer_Player_GetEquipmentMovementModifier
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            if (__result > 0) return;
            if (__instance.GetSEMan().HaveStatusEffect("SE_Wayfarer".GetStableHashCode()))
            {
                __result = 0f;
            }
        }
    }
}