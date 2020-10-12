using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLineCarrot : BulletLine
{
    [SerializeField] private ParticleSystem trailFX;

    public override void Launch(Vector2 velocity) 
    {
        base.Launch(velocity);
        
        trailFX.Play();
    }

    protected override void ReturnToPool() 
    {
        trailFX.Stop();

        base.ReturnToPool();
    }
}
