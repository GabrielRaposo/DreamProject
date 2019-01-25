﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBall : Enemy
{
    protected override void OnTouchEvent(PlayerController player, Vector2 contactPosition)
    {
        base.OnTouchEvent(player, contactPosition);

        player.SetDamage(contactPosition, 1);
    }

}