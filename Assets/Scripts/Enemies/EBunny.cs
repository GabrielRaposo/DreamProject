using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBunny : EnemyController
{
    private PlatformerBunny platformerBunny;
    private ShooterBunny shooterBunny;

    private void Awake() 
    {
        platformerBunny = platformerPhase.GetComponent<PlatformerBunny>();
        shooterBunny = shooterPhase.GetComponent<ShooterBunny>();
    }

    protected override IEnumerator TransitionToDream (Nightmatrix nightmatrix)
    {
        yield return base.TransitionToDream(nightmatrix);

        if (shooterBunny.state == ShooterBunny.State.Launched)
        {
            platformerBunny.SetRollingState(movement.normalized);
        }
    }

    protected override IEnumerator TransitionToNightmare (Nightmatrix nightmatrix)
    {
        yield return base.TransitionToNightmare(nightmatrix);

        if(platformerBunny.state == PlatformerBunny.State.Rolling)
        {
            shooterBunny.SetLaunchedState(- movement.normalized);
        }
    }
}
