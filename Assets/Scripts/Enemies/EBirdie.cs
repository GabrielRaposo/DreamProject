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

        ChaserMovement cm = shooterPhase.GetComponent<ChaserMovement>();
        if(cm && cm.enabled)
        {
            platformerBirdie.AttackIntoDirection(movement.normalized);
        }
    }

    protected override IEnumerator TransitionToNightmare(Nightmatrix nightmatrix)
    {
        yield return base.TransitionToNightmare(nightmatrix);

        if (platformerBirdie.state == PlatformerBirdie.State.Attacking)
        {
            ChaserMovement cm = shooterPhase.GetComponent<ChaserMovement>();
            if (cm)
            {
                shooterBirdie.SetAttack();
                shooterBirdie.transform.rotation = Quaternion.Euler(Vector3.forward * (platformerPhase.transform.rotation.eulerAngles.z + 180));
            }
        }
    }

}
