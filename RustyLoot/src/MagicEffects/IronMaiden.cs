using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;
using UnityEngine;

namespace RustyLoot;

public static class IronMaiden
{
    public static void Setup()
    {
        var def = new MagicItemEffectDefinition("IronMaiden", "$mod_epicloot_ironmaiden", "$mod_epicloot_ironmaiden_desc");
        def.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet, ItemDrop.ItemData.ItemType.Chest, ItemDrop.ItemData.ItemType.Legs);
        def.Requirements.AllowedRarities.Add(ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
        def.ValuesPerRarity.Epic = new ValueDef(5, 10, 1);
        def.ValuesPerRarity.Legendary = new ValueDef(8, 12, 1);
        def.ValuesPerRarity.Mythic = new ValueDef(10, 15, 1);
        def.Register();
        def.Serialize();
        
        SE_IronMaiden? status = ScriptableObject.CreateInstance<SE_IronMaiden>();
        status.name = "SE_IronMaiden";
        status.m_name = "$mod_epicloot_ironmaiden";
        status.m_tooltip = "$mod_epicloot_ironmaiden_desc";
        status.m_ttl = 180f;
        status.Register();
        
    }

    public class SE_IronMaiden : SE_Stats
    {
        public override void SetLevel(int itemLevel, float skillLevel)
        {
            m_addArmor = skillLevel;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
    private static class Player_OnDamaged_Patch
    {
        private static void Postfix(Player __instance, HitData hit)
        {
            if (__instance.HasActiveMagicEffect("IronMaiden", out float modifier))
            {
                var totalDamage = hit.GetTotalDamage();
                var totalHealth = __instance.GetMaxHealth();
                var quarter = totalHealth * 0.25f;
                if (totalDamage > quarter)
                {
                    __instance.GetSEMan().AddStatusEffect("SE_IronMaiden".GetStableHashCode(), true, skillLevel: modifier);
                    __instance.m_adrenalinePopEffects.Create(__instance.transform.position, Quaternion.identity);
                }
            }
        }
    }
}