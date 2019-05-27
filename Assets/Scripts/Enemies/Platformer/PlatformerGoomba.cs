using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerGoomba : PlatformerCreature
{
    [Header("Goomba")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private PlatformerPatroller patroller;
    [SerializeField] private RollingMovement rollingMovement;
    [SerializeField] private float stunTime;
    [SerializeField] private float jumpForce;
    [SerializeField] private ParticleSystem spinFX;

    private bool stunned;
    //private bool onGround;
    private Coroutine stunCoroutine;
    private Coroutine attackCoroutine;

    public enum State { Idle, Rolling }
    public State state { get; private set; }

    private void ResetValues()
    {
        stunned = false;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        if (m_rigidbody) m_rigidbody.gravityScale = 1;
        patroller.enabled = true;
        m_animator.SetBool("Attack", false);
        m_animator.SetTrigger("Reset");
        state = State.Idle;
    }

    public override void OnStompEvent(PlayerPlatformer player)
    {
        if (stunned) return;    

        base.OnStompEvent(player);

        player.SetEnemyJump();
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine 
            = StartCoroutine(StunState (5 * new Vector2((!player.facingRight ? -1 : 1), .6f),
                                       (int)(stunTime * 60), 
                                       player.facingRight));
    }

    public override void OnTouchEvent(PlayerPlatformer player)
    {
        base.OnTouchEvent(player);
        switch (state)
        {
            case State.Idle:
                player.SetDamage(transform.position, 1);
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(AttackState());
                break;
        }
    }

    public override bool OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);
        if (stunned) return false;
        
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        if(gameObject.activeSelf) {

            bool goRight = hitbox.direction.x > 0;

            stunCoroutine 
                = StartCoroutine(StunState (6 * new Vector2((!goRight ? -1 : 1), 1),
                                           (int)(stunTime * 60), 
                                           goRight));
        }
        return true;
    }

    private IEnumerator AttackState()
    {
        m_rigidbody.velocity = Vector2.zero;
        m_animator.SetBool("Attack", true);
        patroller.enabled = false;
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        patroller.enabled = true;
        m_animator.SetBool("Attack", false);
    }

    private IEnumerator StunState(Vector2 launchDirection, int time, bool goRight)
    {
        stunned = true;
        m_animator.SetTrigger("Reset");
        m_animator.SetBool("Stunned", true);
        m_collider.enabled = false;
        stompSFX.Play();

        m_rigidbody.gravityScale = .5f;
        patroller.SetFacingRight(!goRight);
        if (m_rigidbody)
        {
            m_rigidbody.velocity = launchDirection;
        }

        patroller.enabled = false;
        rollingMovement.enabled = false;

        Vector2 startingVelocity = m_rigidbody.velocity;
        Color blinkColor = new Color(255f/255, 150f/255, 150f/255);
        int j = 0;
        for (int i = 0; i < time; i++)
        {
            yield return new WaitForFixedUpdate();
            m_rigidbody.velocity -= (startingVelocity / (time + 1));
            if (i % 3 == 0) j++;
            m_renderer.color = (j % 2 == 0) ? blinkColor : Color.white;
        }

        controller.Die();
    }


    public override void OnBouncyTopEvent(Vector2 contactPosition, bool super)
    {
        m_rigidbody.velocity += Vector2.up * jumpForce * (super ? 2 : 1);
    }

    public override void OnBouncySideEvent(Vector2 contactPosition) 
    {
        base.OnBouncySideEvent(contactPosition);

        switch(state)
        {
            case State.Rolling:

                SetRollingState((contactPosition.x < transform.position.x ? Vector3.right : Vector3.left));
                
                break;
        }
    }

    protected override void OnTwirlEvent(Hitbox hitbox) 
    {
        base.OnTwirlEvent(hitbox);
    
        SetRollingState((hitbox.transform.position.x < transform.position.x ? Vector3.right : Vector3.left));
    }

    public void SetRollingState(Vector2 movement)
    {
        stompSFX.Play();    

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            m_animator.SetTrigger("Reset");
        }
        
        m_animator.SetTrigger("Spin");
        patroller.enabled = false;
        rollingMovement.enabled = true;
        rollingMovement.Launch(this, movement * 6); 
        spinFX.Play();
        state = State.Rolling;
    }

    protected override void OnHitWall(Collision2D collision, Vector2 point) 
    {
        //base.OnHitWall();

        switch(state)
        {
            default:
                if(patroller.enabled) patroller.SetFacingRight(point.x < 0 ? true : false);
                break;

            case State.Rolling:
                if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    BreakableBlock breakableBlock = collision.transform.GetComponent<BreakableBlock>();
                    if(breakableBlock != null)
                    {
                        breakableBlock.TakeDamage(999);
                    }
                    controller.Die();
                }
                break;
        }
    }

    public override void ChildHitboxEnterEvent(Collider2D collision, Hitbox hitbox) 
    {
        base.ChildHitboxEnterEvent(collision, hitbox);

        if(collision.CompareTag("Enemy"))
        {
            PlatformerCreature platformerCreature = collision.GetComponent<PlatformerCreature>();
            if(platformerCreature != null)
            {
                if(platformerCreature.OnHitboxEvent(hitbox)) 
                    StartCoroutine(HitStop());
            }
        }
    }

    private IEnumerator HitStop()
    {
        m_animator.speed = 0;
        rollingMovement.enabled = false;
        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(.05f);

        m_animator.speed = 1;
        rollingMovement.enabled = true;
        m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }
}
