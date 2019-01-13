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
    private bool stunned;
    private Coroutine stunCoroutine;
    private Coroutine attackCoroutine;

    private enum State { Idle, Squished, Vulnerable}
    private State state;

    private void ResetValues()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        if (rb2D) rb2D.gravityScale = 1;
        patroller.enabled = true;
        animator.SetTrigger("Reset");
        state = State.Idle;
    }

    private void Update()
    {
        CheckGround();
    }

    protected override void OnStompEvent(PlayerMovement player, Vector2 contactPosition)
    {
        base.OnStompEvent(player, contactPosition);

        float knockback = 12;
        if(state == State.Vulnerable)
        {
            ResetValues();
            knockback = 0;
        }

        player.SetJump(false);
        TakeDamage(1);
        stompFX.Play();

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunState(knockback, (int)(stunTime * 60), player.facingRight));
    }

    protected override void OnTouchEvent(PlayerMovement player, Vector2 contactPosition)
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
                player.SetExternalForce(knockback);
                ResetValues();
                if (stunCoroutine != null) StopCoroutine(stunCoroutine);
                stunCoroutine = StartCoroutine(StunState(8, (int)(stunTime * 60), transform.position.x > contactPosition.x ? true : false));
                break;
        }
    }

    protected override void OnHammerEvent(Vector2 contactPosition, Vector2 direction)
    {
        base.OnHammerEvent(contactPosition, direction);
        if(state == State.Idle && onGround)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                animator.SetTrigger("Reset");
            }
            TakeDamage(1);
            StartCoroutine(SquishAndLaunch());
        }
        else if (state != State.Squished) {
            ResetValues();
            TakeDamage(1);
            if (stunCoroutine != null) StopCoroutine(stunCoroutine);
            stunCoroutine = StartCoroutine(StunState(12, (int)(stunTime * 60), direction.x > 0 ? true : false));
        }
    }

    private IEnumerator AttackState()
    {
        rb2D.velocity = Vector2.zero;
        animator.SetTrigger("Attack");
        patroller.enabled = false;
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        patroller.enabled = true;
        animator.SetTrigger("Reset");
    }

    private IEnumerator StunState(float pushForce, int time, bool goRight)
    {
        stunned = true;
        animator.SetBool("Stunned", true);

        patroller.SetFacingSide(!goRight);
        if (rb2D)
        {
            rb2D.velocity = pushForce * (!goRight ? Vector2.left : Vector2.right);
        }

        patroller.enabled = false;
        for (int i = 0; i < time; i++)
        {
            yield return new WaitForFixedUpdate();
            rb2D.velocity -= rb2D.velocity * .1f;
        }
        if(health > 0)
        {
            stunned = false;
            animator.SetBool("Stunned", false);
            patroller.enabled = true;
        }
        else
        {
            Destroy(gameObject);
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
        if (rb2D)
        {
            rb2D.velocity = Vector2.zero;
        }
        animator.SetTrigger("Hammer");

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        state = State.Vulnerable;
        //if (coll) coll.enabled = false;
        float targetY = transform.position.y + 3;
        animator.SetTrigger("Launch");
        if (rb2D)
        {
            rb2D.gravityScale = 0;
            rb2D.velocity = Vector2.up * 1;
        }

        while(transform.position.y < targetY)
        {
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }

    public override void OnBouncyTopEvent(Vector2 contactPosition, bool super)
    {
        rb2D.velocity += Vector2.up * jumpForce * (super ? 2 : 1);
    }

    private void CheckGround()
    {
        Vector2 axis = transform.position + (Vector3.down * .5f * transform.localScale.x);
        Vector2 border = new Vector2(.1f, .1f) * transform.localScale.x;

        onGround = Physics2D.OverlapArea(axis - border, axis + border, groundLayer);
    }
}
