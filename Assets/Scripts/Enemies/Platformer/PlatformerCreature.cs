using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerCreature : MonoBehaviour, IStompable, IChildHitboxEvent
{
    [SerializeField] protected float mininumTopY;
    [SerializeField] protected ParticleSystem twirlHitFX;
    [SerializeField] protected AudioSource stompSFX;

    protected bool interactable = true;

    protected Animator m_animator;
    protected SpriteRenderer m_renderer;
    protected Rigidbody2D m_rigidbody;
    protected Collider2D m_collider;
    protected ID id;

    protected IPhaseManager controller;

    public void Init(IPhaseManager controller)
    {
        this.controller = controller;
    }

    private void OnEnable()
    {
        m_animator = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();

        id = ID.Enemy;
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

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        CollisionEvents(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionEvents(collision);
    }

    private void CollisionEvents(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            foreach (ContactPoint2D cp in collision.contacts)
            {
                Vector2 point = cp.point - (Vector2)transform.position;
                if (point.y > -.3f)
                {
                    OnHitWall(collision, point);
                    return;
                }
            }

            OnHitGround(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!interactable) return;

        if (collision.CompareTag("Twirl"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox != null)
            {
                OnTwirlEvent(hitbox);
            }
        } else 
        if (collision.CompareTag("Hitbox"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox != null && hitbox.id != id)
            {
                OnHitboxEvent(hitbox);
            }
        } else
        if (collision.CompareTag("NightmatrixBorder"))
        {
            NightmatrixBorder border = collision.GetComponent<NightmatrixBorder>();
            if (border)
            {
                controller.SetNightmarePhase(border.mainMatrix.GetComponent<Nightmatrix>());
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Nightmatrix"))
        {
            controller.SetNightmarePhase(collision.gameObject.GetComponent<Nightmatrix>());
        }
    }

    protected virtual void OnTwirlEvent(Hitbox hitbox) 
    { 
        if(gameObject.activeSelf) StartCoroutine(InteractionDelay(30));
        
        if (twirlHitFX != null) 
        {
            //twirlHitFX.transform.position = (transform.position + hitbox.transform.position)/2;
            twirlHitFX.Play();
        }
    }  
    public virtual bool OnHitboxEvent(Hitbox hitbox) { if(gameObject.activeSelf) StartCoroutine(InteractionDelay(3)); return false; }

    protected virtual void OnHitWall(Collision2D collision, Vector2 point) { }
    protected virtual void OnHitGround(Collision2D collision) { }


    public float GetYStompRange() { return mininumTopY; }
    public virtual void OnStompEvent(PlayerPlatformer player) { if(gameObject.activeSelf) StartCoroutine(InteractionDelay(3)); }
    public virtual void OnTouchEvent(PlayerPlatformer player) { if(gameObject.activeSelf) StartCoroutine(InteractionDelay(3)); }

    public virtual void OnBouncyTopEvent(Vector2 contactPosition, bool super) { }
    public virtual void OnBouncySideEvent(Vector2 contactPosition) { }

    public virtual void ChildHitboxEnterEvent(Collider2D collision, Hitbox hitbox) { }
    public virtual void ChildHitboxExitEvent(Collider2D collision, Hitbox hitbox) { }

}
