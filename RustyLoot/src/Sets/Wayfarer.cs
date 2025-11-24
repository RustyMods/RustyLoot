using System.Collections.Generic;
using EpicLootAPI;

namespace RustyLoot.Sets;

public class Wayfarer
{
    public static void Setup()
    {
        var set = new LegendarySetInfo(LegendaryType.Mythic, "Wayfarer", "$mod_epicloot_wayfarer");
        set.LegendaryIDs.Add("WayfarerHelmet", "WayfarerChest", "WayfarerLegs", "WayfarerCape");
        set.SetBonuses.Add(2, EffectType.AddMovementSkills, 10, 20);
        set.SetBonuses.Add(2, EffectType.AddPhysicalResistancePercentage, 10, 20);
        set.SetBonuses.Add(3, EffectType.ModifyDiscoveryRadius, 10, 25);
        set.SetBonuses.Add(4, "Wayfarer");
        
        var ability = new AbilityDefinition("Wayfarer", "gdkingheart", 1000f, "SE_Wayfarer");
        var effectDef = new MagicItemEffectDefinition("Wayfarer", "Wayfarer", "Increases carry weight and removed movement speed penalties");
        effectDef.Requirements.NoRoll = true;
        effectDef.Ability = "Wayfarer";
        
        var head = new LegendaryInfo(LegendaryType.Mythic, "WayfarerHelmet", "Wayfarer's Headdress", "The wayfarer lights the way for his fellow vikings");
        head.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        head.GuaranteedEffectCount = 6;
        head.IsSetItem = true;

        var chest = new LegendaryInfo(LegendaryType.Mythic, "WayfarerChest", "Wayfarer's Chestpiece", "The wayfarer is nimble and light on their toes");
        chest.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        chest.GuaranteedEffectCount = 6;
        chest.IsSetItem = true;

        var legs = new LegendaryInfo(LegendaryType.Mythic, "WayfarerLegs", "Wayfarer's Pantaloons", "The wayfarer explores everything and anything");
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;
        legs.IsSetItem = true;

        var cape = new LegendaryInfo(LegendaryType.Mythic, "WayfarerCape", "Wayfarer's Cape", "The wayfarer knows of all breezes and gusts");
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Shoulder);
        cape.GuaranteedEffectCount = 6;
        cape.IsSetItem = true;
    }


    public class SE_Wayfarer : SE_Stats
    {
        public override void Setup(Character character)
        {
            base.Setup(character);
            ApplyToAllies();
        }
    
        public void ApplyToAllies()
        {
            float range = 10f;
            List<Player> players = new();
            Player.GetPlayersInRange(m_character.transform.position, range, players);
            foreach (Player? player in players)
            {
                player.GetSEMan().AddStatusEffect(m_nameHash, true);
            }
        }
    }
}