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

        player.SetEnemyJump();
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine 
            = StartCoroutine(StunState (5 * new Vector2((!player.facingRight ? -1 : 1), .6f),
                                       (int)(stunTime * 60), 
                                       player.facingRight));
    }

    public override void OnTouchEvent(PlayerPlatformer player)
    {
        base.OnTouchEvent(player);
        player.SetDamage(transform.position, 1);
    }

    public override bool OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);

        if(state == State.Stunned) return false;

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        if (gameObject.activeSelf && state != State.Stunned)
        {
            bool goRight = hitbox.direction.x > 0;

            stunCoroutine 
                = StartCoroutine(StunState (6 * new Vector2((!goRight ? -1 : 1), 1),
                                           (int)(stunTime * 60), 
                                           goRight));
        }
        return true;
    }

    private IEnumerator StunState(Vector2 launchDirection, int time, bool goRight)
    {
        state = State.Stunned;
        m_animator.SetTrigger("Reset");
        m_animator.SetBool("Stunned", true);
        m_collider.enabled = false;
        stompSFX.Play();

        m_rigidbody.gravityScale = .5f;
        leaper.SetFacingSide(!goRight);
        if (m_rigidbody)
        {
            m_rigidbody.velocity = launchDirection;
        }

        leaper.enabled = false;
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

    protected override void OnTwirlEvent(Hitbox hitbox) 
    {
        base.OnTwirlEvent(hitbox);

        SetRollingState((hitbox.transform.position.x < transform.position.x ? Vector2.right : Vector2.left) + (Vector2.up *2));
    }

    public void SetRollingState(Vector2 movement)
    {      
        stompSFX.Play();   
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
                IBreakable breakableBlock = collision.gameObject.GetComponent<IBreakable>();
                if(breakableBlock != null)
                {
                    breakableBlock.TakeDamage(999);
                    controller.Die();
                }
                else if (!bouncerMovement.CountBounce()) controller.Die();
                break;
        }
    }

    protected override void OnHitGround(Collision2D collision) 
    {
        if (state == State.Rolling)
        { 
            IBreakable breakableBlock = collision.gameObject.GetComponent<IBreakable>();
            if(breakableBlock != null)
            {
                breakableBlock.TakeDamage(999);
                controller.Die();
            }
            else if (!bouncerMovement.CountBounce()) controller.Die();
        }
    }
}
