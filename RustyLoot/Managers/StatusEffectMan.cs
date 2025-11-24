using System.Collections.Generic;
using HarmonyLib;

namespace RustyLoot.Managers;

public static class StatusEffectMan
{
    private static readonly Dictionary<string, StatusEffect> statusEffects = new();
    
    public static void Register(this StatusEffect status) => statusEffects[status.name] = status;

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Prefix(ObjectDB __instance)
        {
            foreach (StatusEffect? effect in statusEffects.Values)
            {
                if (__instance.m_StatusEffects.Contains(effect)) continue;
                __instance.m_StatusEffects.Add(effect);
            }
        }
    }
}