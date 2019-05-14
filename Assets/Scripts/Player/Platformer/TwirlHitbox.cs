using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwirlHitbox : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private Collider2D coll;
    
    private Transform target;
    
    void Start()
    {
        hitbox = GetComponent<Hitbox>();
        coll = GetComponent<Collider2D>();
        transform.parent = null;
    }

    public void SetDirection(Vector2 direction)
    {
        hitbox.direction = direction;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        transform.position = target.position;
        gameObject.SetActive(true);
        animator.SetTrigger("Reset");
    }

    public void Dettach()
    {
        target = null;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (target)
        {
            transform.position = target.position;
        }
    }
}
