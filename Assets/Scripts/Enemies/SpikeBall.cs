using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBall : Yume
{
    public override void OnTouchEvent(PlayerDreamPhase player)
    {
        base.OnTouchEvent(player);

        player.SetDamage(transform.position, 1);
    }

}
