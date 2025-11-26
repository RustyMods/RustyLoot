using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class SyncFiles
{
    private static void UpdateSyncedFiles()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        foreach (MagicEffect effect in MagicEffect.MagicEffects)
        {
            MagicEffect.SyncEffect(effect.definition);
        }

        foreach (var set in MagicSet.syncedSets.Keys)
        {
            MagicSet.SyncSet(set);
        }
        
        foreach (LegendaryInfo? item in MagicSet.syncedItems.Keys)
        {
            MagicSet.SyncItem(item);
        }
    }
    
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
    private static class ZNet_Start_Patch
    {
        private static void Postfix() => UpdateSyncedFiles();
    }
}