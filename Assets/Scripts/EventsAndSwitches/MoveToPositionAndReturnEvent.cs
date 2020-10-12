using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPositionAndReturnEvent : CallEventOnTrigger
{   
    [SerializeField] private Vector3 positionOffset; 
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float holdTime = 1f;
    [SerializeField] private float returnSpeed = .5f;

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
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        if(eventSwitch != null) eventSwitch.Lock();
        Vector3 direction = (target - transform.position).normalized;
        float localSpeed = moveSpeed / 25;

        while(Vector3.Distance(transform.position, target) > localSpeed)
        {
            yield return new WaitForFixedUpdate();
            transform.position += direction * localSpeed;
        }

        transform.position = target;
        locked = false;
        if(eventSwitch != null) eventSwitch.Unlock();

        yield return Hold();
    }

    private IEnumerator Hold()
    {
        yield return new WaitForSeconds(holdTime);
        yield return Return();
    }

    private IEnumerator Return()
    {
        yield return new WaitForSeconds(holdTime);

        Vector3 direction = (startingPosition - transform.position).normalized;
        float localSpeed = returnSpeed / 25;

        while(Vector3.Distance(transform.position, startingPosition) > localSpeed)
        {
            yield return new WaitForFixedUpdate();
            transform.position += direction * localSpeed;
        }

        transform.position = startingPosition;
    }
}

