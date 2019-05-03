using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerGoomba : PlatformerCreature
{
    [Header("Goomba")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private PlatformerPatroller patroller;
    [SerializeField] private float stunTime;
    [SerializeField] private float jumpForce;

    private bool stunned;
    private bool onGround;
    private Coroutine stunCoroutine;
    private Coroutine attackCoroutine;

    public enum State { Idle, Squished, Vulnerable }
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

    private void Update()
    {
        CheckGround();
    }

    public override void OnStompEvent(PlayerPlatformer player)
    {
        if (stunned) return;    

        base.OnStompEvent(player);

        float knockback = 8;
        if(state == State.Vulnerable)
        {
            ResetValues();
            knockback = 0;
        }

        player.SetEnemyJump();
        //stompFX.Play();

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunState(knockback, (int)(stunTime * 60), player.facingRight));
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

    protected override void OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);
        if (state != State.Squished)
        {
            if (stunCoroutine != null) StopCoroutine(stunCoroutine);
            if(gameObject.activeSelf) stunCoroutine = StartCoroutine(StunState(8, (int)(stunTime * 60), hitbox.direction.x > 0 ? true : false));
        }
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

    private IEnumerator StunState(float pushForce, int time, bool goRight)
    {
        stunned = true;
        m_animator.SetTrigger("Reset");
        m_animator.SetBool("Stunned", true);

        patroller.SetFacingSide(!goRight);
        if (m_rigidbody)
        {
            m_rigidbody.velocity = pushForce * (!goRight ? Vector2.left : Vector2.right);
        }

        patroller.enabled = false;
        Vector2 startingVelocity = m_rigidbody.velocity;
        for (int i = 0; i < time; i++)
        {
            yield return new WaitForFixedUpdate();
            m_rigidbody.velocity -= (startingVelocity / (time + 1));
        }

        controller.Die();
    }

    public void SetVulnerableState()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            m_animator.SetTrigger("Reset");
        }
        StartCoroutine(GhostState());
    }

    private IEnumerator GhostState()
    {
        state = State.Squished;
        patroller.enabled = false;
        if (m_rigidbody)
        {
            m_rigidbody.velocity = Vector2.zero;
        }
        m_animator.SetTrigger("Hammer");

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        state = State.Vulnerable;
        float targetY = transform.position.y + 3;
        m_animator.SetTrigger("Launch");
        if (m_rigidbody)
        {
            m_rigidbody.gravityScale = 0;
            m_rigidbody.velocity = Vector2.up * 1;
        }

        while(transform.position.y < targetY)
        {
            yield return new WaitForFixedUpdate();
        }

        controller.Die();
    }

    public override void OnBouncyTopEvent(Vector2 contactPosition, bool super)
    {
        m_rigidbody.velocity += Vector2.up * jumpForce * (super ? 2 : 1);
    }

    private void CheckGround()
    {
        Vector2 axis = transform.position + (Vector3.down * .5f * transform.localScale.x);
        Vector2 border = new Vector2(.1f, .1f) * transform.localScale.x;

        onGround = Physics2D.OverlapArea(axis - border, axis + border, groundLayer);
    }
}
