using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D m_rigidbody;
    [SerializeField] private UnityEvent launchEvent;

    protected Vector2 velocity;
    [HideInInspector] public BulletPool pool;

    public virtual void Launch(Vector2 velocity)
    {
        this.velocity = velocity;

        if (launchEvent != null)
        {
            launchEvent.Invoke();
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NightmatrixBorder"))
        {
            Vanish();
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Nightmatrix"))
        {
            Vanish();
        }
    }

    public void Vanish()
    {
        if (pool) pool.Return(gameObject);
        else gameObject.SetActive(false);
    }
}
