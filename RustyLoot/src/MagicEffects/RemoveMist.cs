using EpicLootAPI;
using HarmonyLib;

namespace RustyLoot;

public static class MistVision
{
    public static void Setup()
    {
        var def = new MagicItemEffectDefinition("RemoveMist", "$mod_epicloot_removemist", "$mod_epicloot_removemist_desc");
        def.Requirements.AllowedItemNames.Add("$item_demister");
        def.Requirements.AllowedRarities.Add(ItemRarity.Legendary, ItemRarity.Mythic);
        def.Register();
        def.Serialize();
    }
    
    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    private static class Humanoid_EquipItem_Patch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item)
        {
            if (__instance is not Player player || !ParticleMist.m_instance) return;
            ParticleMist.m_instance.enabled = !player.HasActiveMagicEffect("RemoveMist", out float _);
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    private static class Humanoid_UnequipItem_Patch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item)
        {
            if (__instance is not Player player || !ParticleMist.m_instance) return;
            ParticleMist.m_instance.enabled = !player.HasActiveMagicEffect("RemoveMist", out float _);
        }
    }
}