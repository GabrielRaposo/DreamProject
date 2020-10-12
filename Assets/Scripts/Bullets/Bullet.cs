using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour, IPoolObject
{
    protected Animator m_animator;
    protected Collider2D m_collider;
    protected Rigidbody2D m_rigidbody;
    protected Hitbox m_hitbox;

    [SerializeField] private UnityEvent launchEvent;

    protected Vector2 velocity;
    private BulletPool pool;

    private void Awake() 
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_hitbox = GetComponent<Hitbox>();
    }

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

        StartCoroutine(TimerToVanish(3));
    }

    private IEnumerator TimerToVanish(float time)
    {
        yield return new WaitForSeconds(time);
        Vanish();
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NightmatrixBorder"))
        {
            Vanish();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IBreakable breakable = collision.GetComponent<IBreakable>();
            if (breakable != null)
            {
                breakable.TakeDamage(m_hitbox.damage);
            }
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
        StopAllCoroutines();
        StartCoroutine(DisableComponents());
    }

    private IEnumerator DisableComponents()
    {
        ToggleComponents(false);

        m_animator.SetTrigger("Vanish");
        yield return new WaitForSeconds(.5f);

        ReturnToPool();
    }

    protected virtual void ReturnToPool()
    {
        if (pool) pool.Return(gameObject);
        else gameObject.SetActive(false);
    }

    public void SetPool(BulletPool pool) 
    {
        this.pool = pool;
    }
}
