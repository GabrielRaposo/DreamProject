using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LinearFlightCicle : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;

    private float speed;
    private Vector2 originalPosition;
    private Vector2 targetPosition;

    private Vector2 moveDirection;
    private bool goingToTarget;

    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_renderer;

    private void OnEnable()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();

        originalPosition = transform.position;
        targetPosition = target.position;
        moveDirection = (targetPosition - originalPosition).normalized;

        goingToTarget = true;
    }

    private void FixedUpdate()
    {
        if (goingToTarget)
        {
            if (speed < maxSpeed)
            {
                speed += acceleration;
            }
            else speed = maxSpeed;

            if (Vector2.Distance(transform.position, targetPosition) < .1f)
            {
                goingToTarget = false;
                if (Mathf.Abs(moveDirection.x) > 0) m_renderer.flipX = speed > 0; 
            }
        }
        else
        {
            if (speed > -maxSpeed)
            {
                speed -= acceleration;
            }
            else speed = -maxSpeed;

            if (Vector2.Distance(transform.position, originalPosition) < .1f)
            {
                goingToTarget = true;
                if (Mathf.Abs(moveDirection.x) > 0) m_renderer.flipX = speed > 0;
            }
        }
        m_rigidbody.velocity = moveDirection * speed;
    }

    public void Stop()
    {
        moveDirection = Vector2.zero;
    }

    public void Restart()
    {
        moveDirection = (targetPosition - originalPosition).normalized;
    }
}
