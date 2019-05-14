using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerBunny : PlatformerCreature
{
    [Header("Bunny")]
    [SerializeField] private PlatformerLeaper leaper;
    [SerializeField] private float stunTime;
    [SerializeField] private float jumpForce;
    [SerializeField] private BouncerMovement bouncerMovement;
    [SerializeField] private ParticleSystem spinFX;

    public enum State { Idle, Stunned, Rolling }
    public State state;

    private Coroutine stunCoroutine;

    private void ResetValues()
    {
        state = State.Idle;
        leaper.enabled = true;
    }

    public override void OnStompEvent(PlayerPlatformer player)
    {
        if (state == State.Stunned) return;    

        base.OnStompEvent(player);

        float knockback = 8;

        player.SetEnemyJump();

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunState(knockback, (int)(stunTime * 60), player.facingRight));
    }

    public override void OnTouchEvent(PlayerPlatformer player)
    {
        base.OnTouchEvent(player);
        player.SetDamage(transform.position, 1);
    }

    public override bool OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        if (gameObject.activeSelf) stunCoroutine = StartCoroutine(StunState(8, (int)(stunTime * 60), hitbox.direction.x > 0 ? true : false));
        return false;
    }

    private IEnumerator StunState(float pushForce, int time, bool goRight)
    {
        state = State.Stunned;
        m_animator.SetTrigger("Reset");
        m_animator.SetBool("Stunned", true);

        leaper.SetFacingSide(!goRight);
        if (m_rigidbody)
        {
            m_rigidbody.velocity = pushForce * (!goRight ? Vector2.left : Vector2.right);
        }

        leaper.enabled = false;
        Vector2 startingVelocity = m_rigidbody.velocity;
        for (int i = 0; i < time; i++)
        {
            yield return new WaitForFixedUpdate();
            m_rigidbody.velocity -= (startingVelocity / (time + 1));
        }

        controller.Die();
    }

    public override void OnBouncyTopEvent(Vector2 contactPosition, bool super)
    {
        m_rigidbody.velocity += Vector2.up * jumpForce * (super ? 2 : 1);
    }

    protected override void OnTwirlEvent(Hitbox hitbox) 
    {
        base.OnTwirlEvent(hitbox);

        SetRollingState((hitbox.transform.position.x < transform.position.x ? Vector2.right : Vector2.left) + (Vector2.up *2));
    }

    public void SetRollingState(Vector2 movement)
    {      
        m_animator.SetTrigger("Spin");
        leaper.enabled = false;
        m_rigidbody.velocity = Vector2.zero;
        bouncerMovement.enabled = true;
        bouncerMovement.Launch(this, movement * 5); 
        m_rigidbody.gravityScale = 1.5f;
        spinFX.Play();
        state = State.Rolling;
    }

    protected override void OnHitWall(Collision2D collision, Vector2 point) 
    {
        switch(state)
        {
            case State.Idle:
                leaper.SetFacingSide(point.x < 0);
                break;

            case State.Rolling:
                BreakableBlock breakableBlock = collision.gameObject.GetComponent<BreakableBlock>();
                if(breakableBlock)
                {
                    breakableBlock.TakeDamage(999);
                    controller.Die();
                }
                else if (!bouncerMovement.CountBounce()) controller.Die();
                break;
        }
        if (state == State.Rolling)
        {
            
        }
    }

    protected override void OnHitGround(Collision2D collision) 
    {
        if (state == State.Rolling)
        { 
            BreakableBlock breakableBlock = collision.gameObject.GetComponent<BreakableBlock>();
            if(breakableBlock)
            {
                breakableBlock.TakeDamage(999);
                controller.Die();
            }
            else if (!bouncerMovement.CountBounce()) controller.Die();
        }
    }
}
