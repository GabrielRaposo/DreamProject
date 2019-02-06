using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoomba : Enemy
{
    [Header("Goomba")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Patroller patroller;
    [SerializeField] private float stunTime;
    [SerializeField] private float jumpForce;
    [SerializeField] private ParticleSystem stompFX;

    private bool onGround;
    private Coroutine stunCoroutine;
    private Coroutine attackCoroutine;

    private enum State { Idle, Squished, Vulnerable }
    private State state;

    private void ResetValues()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        if (m_rigidbody) m_rigidbody.gravityScale = 1;
        patroller.enabled = true;
        m_animator.SetBool("Attack", false);
        m_animator.SetTrigger("Reset");
        state = State.Idle;
        Debug.Log("reset");
    }

    private void Update()
    {
        CheckGround();
    }

    public override void OnStompEvent(PlayerController player, Vector2 contactPosition)
    {
        base.OnStompEvent(player, contactPosition);

        float knockback = 8;
        if(state == State.Vulnerable)
        {
            ResetValues();
            knockback = 0;
        }

        player.SetJump(false);
        TakeDamage(1);
        stompFX.Play();

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunState(knockback, (int)(stunTime * 60), player.facingRight));
    }

    public override void OnTouchEvent(PlayerController player, Vector2 contactPosition)
    {
        base.OnTouchEvent(player, contactPosition);
        switch (state)
        {
            case State.Idle:
                player.SetDamage(contactPosition, 1);
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(AttackState());
                break;

            case State.Vulnerable:
                Vector2 knockback = (transform.position.x < contactPosition.x ? Vector3.right : Vector3.left) * 20;
                //player.SetPush(knockback);
                //ResetValues();
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                if (stunCoroutine != null) StopCoroutine(stunCoroutine);
                stunCoroutine = StartCoroutine(StunState(8, (int)(stunTime * 60), transform.position.x > contactPosition.x ? true : false));
                break;
        }
    }

    protected override void OnHammerEvent(Vector2 contactPosition, Hitbox hitbox)
    {
        base.OnHammerEvent(contactPosition, hitbox);
        if(state == State.Idle && onGround)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                m_animator.SetTrigger("Reset");
            }
            TakeDamage(hitbox.damage);
            StartCoroutine(SquishAndLaunch());
        }
        else if (state != State.Squished) {
            ResetValues();
            TakeDamage(hitbox.damage);
            if (stunCoroutine != null) StopCoroutine(stunCoroutine);
            stunCoroutine = StartCoroutine(StunState(8, (int)(stunTime * 60), hitbox.direction.x > 0 ? true : false));
        }
    }

    protected override void OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);
        if (state != State.Squished)
        {
            TakeDamage(hitbox.damage);
            if (stunCoroutine != null) StopCoroutine(stunCoroutine);
            stunCoroutine = StartCoroutine(StunState(8, (int)(stunTime * 60), hitbox.direction.x > 0 ? true : false));
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
        Debug.Log("out");
    }

    private IEnumerator StunState(float pushForce, int time, bool goRight)
    {
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
        if(health > 0)
        {
            m_animator.SetBool("Stunned", false);
            patroller.enabled = true;
        }
        else
        {
            Die();
        }
    }

    protected override void TakeDamage(int damage)
    {
        health -= damage;
    }

    private IEnumerator SquishAndLaunch()
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
        //if (coll) coll.enabled = false;
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

        Die();
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
