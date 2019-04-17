using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YumeBirdie : Yume
{
    [Header("Birdy")]
    [SerializeField] private bool aimBelow;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private LinearFlightCicle flightCicle;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ParticleSystem flightFX;
    [SerializeField] private float stunTime;
    [SerializeField] private ParticleSystem stompFX;

    public enum State { Idle, Attacking, Dizzy }
    public State state { get; private set; }

    private Coroutine angryCoroutine;
    private Vector2 direction;
    private bool stunned;

    private void Start()
    {
        if (aimBelow) m_renderer.color = new Color(236f / 255, 162f / 255, 227f / 255);
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Idle:
                if (aimBelow && !stunned)
                {
                    if (Physics2D.Linecast(transform.position, transform.position + (Vector3.down * 10), 1 << LayerMask.NameToLayer("Player")))
                    {
                        AttackIntoDirection(Vector2.down);
                    }
                }
                break;

            case State.Attacking:
                m_rigidbody.velocity = direction * 6;
                break;
        }
    }

    private void ResetValues()
    {
        if (angryCoroutine != null) StopCoroutine(angryCoroutine);
        transform.rotation = Quaternion.Euler(Vector3.zero);
        flightCicle.enabled = true;
        m_renderer.flipY = false;
        m_animator.SetTrigger("Reset");
    }

    public override void OnStompEvent(PlayerDreamPhase player)
    {
        base.OnStompEvent(player);

        ResetValues();
        switch (state)
        {
            case State.Idle:
            case State.Dizzy:
                player.SetJump(false);
                stompFX.Play();
                break;

            case State.Attacking:
                player.SetDamage(transform.position, 1);
                break;
        }

        TakeDamage();
    }

    private void TakeDamage()
    {
        state = State.Idle;
        ResetValues();
        m_collider.enabled = false;
        m_rigidbody.velocity = (Vector2.up * .5f);
        flightCicle.enabled = false;
        StartCoroutine(StunState());
    }

    public override void OnTouchEvent(PlayerDreamPhase player)
    { 
        base.OnTouchEvent(player);

        player.SetDamage(transform.position, 1);

        switch (state)
        {
            case State.Idle:
                if (angryCoroutine != null) StopCoroutine(angryCoroutine);
                angryCoroutine = StartCoroutine(AngryState());
                break;
        }
    }

    private IEnumerator StunState()
    {
        m_animator.SetTrigger("Stunned");
        stunned = true;
        m_collider.enabled = false;
        yield return new WaitForSeconds(.3f);
        controller.Die();
    }

    private IEnumerator AngryState()
    {
        m_rigidbody.velocity = Vector2.zero;
        m_animator.SetTrigger("Angry");
        flightCicle.Stop();
        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        flightCicle.Restart();
        m_animator.SetTrigger("Reset");
    }

    protected override void OnHitboxEvent(Hitbox hitbox)
    {
        ResetValues();

        m_collider.enabled = false;
        m_rigidbody.velocity = (Vector2.up * .5f);
        flightCicle.enabled = false;
        StartCoroutine(StunState());
    }

    public void AttackIntoDirection(Vector2 direction)
    {
        this.direction = direction;
        flightCicle.enabled = false;

        hitbox.direction = direction;
        hitbox.transform.localPosition = Vector2.right * .3f;
        hitbox.GetComponent<Collider2D>().enabled = true;

        float angle = Vector2.SignedAngle(Vector2.up, direction) + 90;
        transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        m_renderer.flipX = true;
        m_renderer.flipY = (angle > 0) ? true : false;

        m_animator.SetTrigger("Attack");
        flightFX.Play();

        state = State.Attacking;
    }

    private IEnumerator BounceOnHit()
    {
        Vector2 movement = (direction * 2);
        m_rigidbody.velocity = -movement;
        int iterations = 20;
        for (int i = 0; i < iterations; i++)
        {
            yield return new WaitForFixedUpdate();
            m_rigidbody.velocity += (movement / iterations);
        }
        m_rigidbody.velocity = Vector2.zero;
    }

    public override void OnBouncyTopEvent(Vector2 contactPosition, bool super)
    {
        AttackIntoDirection(Vector2.up);
    }

    public override void OnBouncySideEvent(Vector2 contactPosition)
    {
        AttackIntoDirection(Vector2.up);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == State.Attacking)
        {
            int layer = collision.gameObject.layer;
            if (groundLayer == (groundLayer | 1 << layer))
            {
                hitbox.GetComponent<Collider2D>().enabled = false;
                if(gameObject.activeSelf) StartCoroutine(BounceOnHit());
                m_animator.SetTrigger("Dizzy");
                flightFX.Stop();
                transform.rotation = Quaternion.Euler(Vector3.zero);
                m_renderer.flipY = m_renderer.flipX = false;

                state = State.Dizzy;
            }
        }
    }
}
