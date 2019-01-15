using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    [SerializeField] protected float mininumTopY;

    protected bool interactable = true;

    protected Animator animator;
    protected Rigidbody2D rb2D;
    protected Collider2D coll;
    protected ID id;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        id = ID.Enemy;
    }

    protected virtual void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 1)
        {
            Destroy(gameObject);
        }
    }

    protected IEnumerator InteractionDelay(int frames)
    {
        interactable = false;
        while(frames > 0)
        {
            yield return new WaitForFixedUpdate();
            frames--;
        }
        interactable = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            Vector2 direction = Vector2.zero;
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox != null)
            {
                direction = hitbox.direction;
            }
            OnHammerEvent(collision.transform.position, direction);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!interactable) return;

        if (collision.transform.CompareTag("Player"))
        {
            Vector3 contactPoint = collision.transform.position;
            if (collision.contactCount > 0) contactPoint = collision.contacts[0].point;

            PlayerMovement player = collision.transform.GetComponent<PlayerMovement>();
            if (player == null)
            {
                player = new PlayerMovement();
            }

            if ((collision.transform.position - transform.position).y > mininumTopY)
            {
                OnStompEvent(player, contactPoint);
            }
            else
            {
                OnTouchEvent(player, contactPoint);
            }
        }  
    }

    protected virtual void OnHammerEvent(Vector2 contactPosition, Vector2 direction)    { StartCoroutine(InteractionDelay(3)); }
    protected virtual void OnStompEvent(PlayerMovement player, Vector2 contactPosition) { StartCoroutine(InteractionDelay(3)); }
    protected virtual void OnTouchEvent(PlayerMovement player, Vector2 contactPosition) { StartCoroutine(InteractionDelay(3)); }

    public virtual void OnBouncyTopEvent(Vector2 contactPosition, bool super) { }
    public virtual void OnBouncySideEvent(Vector2 contactPosition) { }
}
