using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private UnityEvent OnReachEvent;

    private Transform target;

    void Start()
    {
        if (OnReachEvent == null)
        {
            OnReachEvent = new UnityEvent();
        }

        if (moveSpeed <= 0) moveSpeed = .1f;
    }

    public void Follow (Transform target)
    {
        this.target = target;
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            transform.position += (target.position - transform.position) * moveSpeed;
            if ((transform.position - target.position).magnitude < (moveSpeed + .1f))  
            {
                transform.position = target.position;
                OnReachEvent.Invoke();
            }
        }
    }
}
