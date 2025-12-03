using System.Collections.Generic;
using EpicLootAPI;
using UnityEngine;

namespace RustyLoot;

public static partial class Seidrweaver
{
    private static readonly EffectListRef healEffect = new("fx_DvergerMage_Nova_ring");
    public class SE_Seidrweaver : SE_Stats
    {
        
    }

    
    public class SiedrweaverProxy : Proxy
    {
        public override void Activate()
        {
            if (!Player.HaveEitr(30))
            {
                Hud.instance.EitrBarEmptyFlash();
                return;
            }
            if (IsOnCooldown()) return;
            SetCooldownEndTime(GetTime() + Cooldown);
            List<Player> list = new List<Player>();
            Player.GetPlayersInRange(Player.transform.position, 50f, list);
            list.Add(Player);
            foreach (Player? player in list)
            {
                player.GetSEMan().AddStatusEffect("SE_Seidrweaver".GetStableHashCode(), true);
            }

            healEffect.Create(Player.GetHeadPoint(), Quaternion.identity);
        }
    }
}