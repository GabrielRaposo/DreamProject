using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LinearMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float knockback;
    [SerializeField] private UnityEvent OnReachEvent;

    private Rigidbody2D m_rigidbody;

    private Vector2 direction;
    private Transform target;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start() 
    {
        if (OnReachEvent == null)
        {
            OnReachEvent = new UnityEvent();
        }
    }

        public void SetDirection(Vector2 direction)
    {
        this.direction = direction;

        m_rigidbody.velocity = direction * speed;
    }

    public void SetDirection(Vector2 direction, Transform target)
    {
        this.direction = direction;
        this.target = target;

        m_rigidbody.velocity = direction * speed;
    }

    public void MoveBack()
    {
        transform.position -= (Vector3) direction * knockback;
    }

    private void Update() 
    {
        if(target != null && Vector2.Distance(target.position, transform.position) < .1f)
        {
            OnReachEvent.Invoke();
        }
    }
}
