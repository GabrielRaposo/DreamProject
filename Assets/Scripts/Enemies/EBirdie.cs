using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBirdie : EnemyController
{
    private PlatformerBirdie platformerBirdie;
    private ShooterBirdie shooterBirdie;

    private void Awake() 
    {
        platformerBirdie = platformerPhase.GetComponent<PlatformerBirdie>();
        shooterBirdie = shooterPhase.GetComponent<ShooterBirdie>();

    }

    protected override IEnumerator TransitionToDream(Nightmatrix nightmatrix)
    {
        yield return base.TransitionToDream(nightmatrix);

        LinearMovement lm = shooterPhase.GetComponent<LinearMovement>();
        if(lm && lm.enabled)
        {
            platformerBirdie.AttackIntoDirection(movement.normalized);
        }
    }

    protected override IEnumerator TransitionToNightmare(Nightmatrix nightmatrix)
    {
        bool attacking = (platformerBirdie.state == PlatformerBirdie.State.Attacking || platformerBirdie.state == PlatformerBirdie.State.Diving);
        shooterBirdie.attacking = attacking;

        yield return base.TransitionToNightmare(nightmatrix);

        if (attacking) shooterBirdie.SetDirectionalAttack(-movement.normalized);
    }

}
