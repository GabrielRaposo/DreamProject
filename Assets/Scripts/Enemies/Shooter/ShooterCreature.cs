using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterCreature : MonoBehaviour, IObserver, IShooterTouch, IChildHitboxEvent
{
    [SerializeField] protected AudioSource hitSFX;

    protected Animator m_animator;
    protected SpriteRenderer m_renderer;
    protected Rigidbody2D m_rigidbody;

    protected bool facingRight;
    protected ID id;
    protected Nightmatrix currentNightmatrix;

    protected IPhaseManager controller;
    protected ShooterMovement shooterMovement;

    public void Init(IPhaseManager controller)
    {
        this.controller = controller;
    }

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        shooterMovement = GetComponent<ShooterMovement>();

        id = ID.Enemy;
    }

    public virtual void SwitchIn(Nightmatrix nightmatrix)
    {
        m_renderer.color = Color.white;

        currentNightmatrix = nightmatrix;
        currentNightmatrix.AddObserver(this);

        if (shooterMovement != null)
        {
            shooterMovement.enabled = true;
            shooterMovement.Call(nightmatrix);
        }
    }

    public virtual void SwitchOut()
    {
        if (currentNightmatrix != null)
        {
            currentNightmatrix.RemoveObserver(this);
            currentNightmatrix = null;
        }

        if (shooterMovement != null)
        {
            shooterMovement.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (currentNightmatrix != null)
        {
            currentNightmatrix.RemoveObserver(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            OnWallEvent(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentNightmatrix == null && collision.CompareTag("Nightmatrix"))
        {
            Nightmatrix nightmatrix = collision.GetComponent<Nightmatrix>();
            if(nightmatrix) SwitchIn(nightmatrix);
        }

        if (collision.CompareTag("Hitbox"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox != null && hitbox.id != id)
            {
                OnHitboxEvent(hitbox, hitbox.damage);
            }
        } else
        if (collision.CompareTag("Explosion"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox != null && hitbox.id != id)
            {
                OnHitboxEvent(hitbox, 100);
            }
        } else
        if (collision.CompareTag("NightmatrixBorder"))
        {
            NightmatrixBorder border = collision.GetComponent<NightmatrixBorder>();
            if (border)
            {
                controller.SetDreamPhase(border.mainMatrix.GetComponent<Nightmatrix>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Nightmatrix"))
        {
            controller.SetDreamPhase(collision.gameObject.GetComponent<Nightmatrix>());
        }
    }

    public virtual void OnNotify() {}

    public virtual void OnHitboxEvent(Hitbox hitbox, int damage) { }
    public virtual void OnTouchEvent(PlayerShooter player) { }
    protected virtual void OnWallEvent(Collision2D collision) { }

    public virtual void ChildHitboxEnterEvent(Collider2D collision, Hitbox hitbox) { }
    public virtual void ChildHitboxExitEvent(Collider2D collision, Hitbox hitbox) { }

    protected virtual IEnumerator AttackCicle() { yield return null; }

    protected IEnumerator BlinkAnimation()
    {
        m_renderer.color = Color.red;
        for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate();
        m_renderer.color = Color.white;
    }

}
