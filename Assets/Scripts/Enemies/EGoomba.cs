using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EGoomba : EnemyController
{
    private PlatformerGoomba platformerGoomba;
    private ShooterGoomba shooterGoomba;

    private void Awake() 
    {
        platformerGoomba = platformerPhase.GetComponent<PlatformerGoomba>();
        shooterGoomba = shooterPhase.GetComponent<ShooterGoomba>();
    }

    protected override IEnumerator TransitionToDream (Nightmatrix nightmatrix)
    {
        yield return base.TransitionToDream(nightmatrix);
        
        if (shooterGoomba.vulnerable)
        {
            platformerGoomba.SetVulnerableState();
        }
    }

    protected override IEnumerator TransitionToNightmare (Nightmatrix nightmatrix)
    {
        yield return base.TransitionToNightmare(nightmatrix);

        if(platformerGoomba.state == PlatformerGoomba.State.Vulnerable)
        {
            shooterGoomba.SetVulnerableState();
        }
    }
}
