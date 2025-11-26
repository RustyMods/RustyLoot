using System.Collections.Generic;
using UnityEngine;
using EpicLootAPI;
using HarmonyLib;
using RustyLoot.Managers;

namespace RustyLoot.Sets;

public static class Ulfheonar
{
    public static readonly EffectList flamethrowerEffects = new EffectList();
    
    public static void Setup()
    {
        GameObject? prefab = AssetBundleManager.LoadAsset<GameObject>("ascension_bundle", "vfx_flamethrower_purple");
        prefab!.transform.localScale *= 1.2f;
        PrefabManager.RegisterPrefab(prefab);
        flamethrowerEffects.m_effectPrefabs = new List<EffectList.EffectData>()
        {
            new ()
            {
                m_prefab = prefab,
                m_attach = true,
            }
        }.ToArray();

        Sprite icon = SpriteManager.RegisterSprite("bone_skull.png")!;
        EpicLoot.RegisterAsset("BoneSkull", icon);
        
        SE_Flamethrower? flamethrower = ScriptableObject.CreateInstance<SE_Flamethrower>();
        flamethrower.name = "SE_Flamethrower";
        flamethrower.m_name = "$mod_epicloot_ulfheonar";
        flamethrower.m_tooltip = "$mod_epicloot_ulfheonar_desc";
        flamethrower.m_speedModifier = 0.1f;
        flamethrower.m_percentigeDamageModifiers.m_blunt = 0.1f;
        flamethrower.m_percentigeDamageModifiers.m_pierce = 0.1f;
        flamethrower.m_percentigeDamageModifiers.m_slash = 0.1f;
        flamethrower.m_ttl = 300f;
        flamethrower.m_icon = icon;
        flamethrower.Register();
        
        var head = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarHelmet", "$mod_epicloot_ulfheonar_helm", "$mod_epicloot_ulfheonar_helm_desc");
        head.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Helmet);
        head.GuaranteedEffectCount = 6;
        head.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyDamageLowHealth, 15, 20), new GuaranteedMagicEffect(EffectType.AvoidDamageTakenLowHealth, 10, 20));

        var cape = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarCape", "$mod_epicloot_ulfheonar_cape", "$mod_epicloot_ulfheonar_cape_desc");
        cape.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Chest);
        cape.GuaranteedEffectCount = 6;
        cape.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyPhysicalDamage, 10, 20), new GuaranteedMagicEffect(EffectType.ModifyStaggerDuration, 10, 20));

        var legs = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarLegs", "$mod_epicloot_ulfheonar_legs", "$mod_epicloot_ulfheonar_legs_desc");
        legs.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.Legs);
        legs.GuaranteedEffectCount = 6;
        legs.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.ModifyMovementSpeedLowHealth, 10, 20), new GuaranteedMagicEffect(EffectType.AvoidDamageTakenLowHealth, 10, 20));
        
        var claws = new LegendaryInfo(LegendaryType.Mythic, "UlfheonarClaws", "$mod_epicloot_ulfheonar_claws", "$mod_epicloot_ulfheonar_claws_desc");
        claws.Requirements.AddAllowedItemTypes(ItemDrop.ItemData.ItemType.TwoHandedWeapon);
        claws.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Unarmed);
        claws.GuaranteedEffectCount = 6;
        claws.GuaranteedMagicEffects.Add(new GuaranteedMagicEffect(EffectType.AddUnarmedSkill, 10, 20), new GuaranteedMagicEffect(EffectType.ModifyAttackSpeed, 10, 20));

        var set = new MagicSet("Ulfheonar", LegendaryType.Mythic);
        set.AddItems(head, claws, legs, cape);
        set.SetBonuses.Add(2, EffectType.AddMovementSkills, 10, 20);
        set.SetBonuses.Add(2, EffectType.IncreaseHeatResistance, 10, 20);
        set.SetBonuses.Add(3, EffectType.ModifyDamage, 10, 25);
        set.SetBonuses.Add(4, "Ulfheonar");
        set.Register();
        set.Serialize();
        
        AbilityProxyDefinition proxy = new AbilityProxyDefinition("Ulfheonar", AbilityActivationMode.Activated, typeof(UlfheonarProxy));
        proxy.Ability.Cooldown = 600f;
        proxy.Ability.IconAsset = "BoneSkull";
        proxy.Register();
        
        MagicItemEffectDefinition effectDef = new MagicItemEffectDefinition("Ulfheonar", "$mod_epicloot_ulfheonar_desc", "$mod_epicloot_ulfheonar_desc");
        effectDef.Requirements.NoRoll = true;
        effectDef.Ability = "Ulfheonar";
        effectDef.Register();
    }

    public class UlfheonarProxy : Proxy
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
                player.GetSEMan().AddStatusEffect("SE_Flamethrower".GetStableHashCode(), true);
            }
        }
    }

    public class SE_Flamethrower : SE_Stats
    {
        public override void Setup(Character character)
        {
            base.Setup(character);
            Trigger();
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            hit.ApplyModifier(1.1f);
        }

        public override string GetTooltipString()
        {
            string tooltip = base.GetTooltipString();
            tooltip += "\n$mod_epicloot_damage_taken: <color=orange>+10</color>%";
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