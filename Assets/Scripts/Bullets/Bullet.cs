using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected Animator m_animator;
    [SerializeField] protected Collider2D m_collider;
    [SerializeField] protected Rigidbody2D m_rigidbody;

    [SerializeField] private UnityEvent launchEvent;

    protected Vector2 velocity;
    [HideInInspector] public BulletPool pool;

    private void ToggleComponents(bool value)
    {
        m_collider.enabled = value;
        if(!value)
        {
            m_rigidbody.velocity = Vector2.zero;
        }
    }

    public virtual void Launch(Vector2 velocity)
    {
        this.velocity = velocity;

        ToggleComponents(true);

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
        StartCoroutine(DisableComponents());
    }

    private IEnumerator DisableComponents()
    {
        ToggleComponents(false);

        m_animator.SetTrigger("Vanish");
        yield return new WaitForSeconds(.5f);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pool) pool.Return(gameObject);
        else gameObject.SetActive(false);
    }
}
