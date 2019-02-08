using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NenemyGoomba : Nenemy
{
    protected override void OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);

        hitbox.gameObject.SetActive(false);
        TakeDamage(hitbox.damage);
    }

    public override void OnTouchEvent(PlayerNightmarePhase player)
    {
        base.OnTouchEvent(player);

        player.SetDamage(1);
    }
}
