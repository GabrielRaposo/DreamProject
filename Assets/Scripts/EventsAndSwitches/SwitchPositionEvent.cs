using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPositionEvent : CallEventOnTrigger
{   
    [SerializeField] private Vector3 positionOffset; 
    [SerializeField] private float speed = 1;

    private Vector3 startingPosition;   
    private Vector3 targetPosition;
    private bool locked; 
    private EventSwitch eventSwitch;

    void Start()
    {
        startingPosition = transform.position;
        targetPosition = startingPosition + positionOffset;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + positionOffset, .2f);
        Gizmos.DrawLine(transform.position, transform.position + positionOffset);
    }

    public override void CallEvent(EventSwitch eventSwitch)
    {
        if(locked) return;
        locked = true;

        this.eventSwitch = eventSwitch;

        StopAllCoroutines();
        StartCoroutine(MoveTo(targetPosition));
        targetPosition = (targetPosition == startingPosition) ? (startingPosition + positionOffset) : startingPosition;
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        if(eventSwitch != null) eventSwitch.Lock();
        Vector3 direction = (target - transform.position).normalized;
        float localSpeed = speed / 25;

        while(Vector3.Distance(transform.position, target) > localSpeed)
        {
            yield return new WaitForFixedUpdate();
            transform.position += direction * localSpeed;
        }

        transform.position = target;
        locked = false;
        if(eventSwitch != null) eventSwitch.Unlock();
    }
}
