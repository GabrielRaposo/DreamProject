using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchNightmatrixDirectionEvent : CallEventOnTrigger
{
    private Nightmatrix nightmatrix;    

    private void Start() 
    {
        nightmatrix = GetComponent<Nightmatrix>();
    }

    public override void CallEvent(EventSwitch eventSwitch)
    {
        if (nightmatrix && eventSwitch) 
        {
            nightmatrix.InvertDirection();
            StartCoroutine(LockAndUnlockLoop(eventSwitch));
        }
    }

    private IEnumerator LockAndUnlockLoop(EventSwitch eventSwitch)
    {
        eventSwitch.Lock();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        eventSwitch.Unlock();
    }
}
