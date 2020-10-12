using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour, IPoolObject, IStompable, IShooterTouch
{
    [SerializeField] private float mininumTopY;

    [Header("Components")]
    [SerializeField] private Rigidbody2D m_rigidbody;
    [SerializeField] private Collider2D m_collider;
    [SerializeField] private Animator m_animator;

    private BulletPool pool;

    private void ToggleComponents(bool value)
    {
        m_collider.enabled = value;
        if(!value)
        {
            m_rigidbody.velocity = Vector2.zero;
        }
    }

    public void Launch(Vector2 velocity)
    {
        ToggleComponents(true);

        m_animator.SetTrigger("Reset");
        m_rigidbody.velocity = velocity;
    }

    public void SetPool(BulletPool pool) 
    {
        this.pool = pool;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
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

    protected virtual void ReturnToPool()
    {
        if (pool) pool.Return(gameObject);
        else gameObject.SetActive(false);
    }

    public float GetYStompRange() { return mininumTopY; }

    public virtual void OnStompEvent(PlayerPlatformer player) 
    {  
        player.SetEnemyJump();
        Vanish();
    }

    public virtual void OnTouchEvent(PlayerPlatformer player) 
    {  
        player.SetDamage(transform.position, 1);
    }

    public void OnTouchEvent(PlayerShooter player) 
    {
        player.SetDamage(1);
    }
}
