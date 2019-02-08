using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nenemy : MonoBehaviour
{
    public int health;
    [SerializeField] protected GameObject destructionFX;

    protected Animator m_animator;
    protected SpriteRenderer m_renderer;
    protected Rigidbody2D m_rigidbody;

    protected ID id;

    private void OnEnable()
    {
        m_animator = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_rigidbody = GetComponent<Rigidbody2D>();

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox != null && hitbox.id != id)
            {
                OnHitboxEvent(hitbox);
            }
        }
    }

    protected void Die()
    {
        if (destructionFX != null)
        {
            Instantiate(destructionFX, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    protected virtual void OnHitboxEvent(Hitbox hitbox) { }
    public virtual void OnTouchEvent(PlayerNightmarePhase player) { }
}
