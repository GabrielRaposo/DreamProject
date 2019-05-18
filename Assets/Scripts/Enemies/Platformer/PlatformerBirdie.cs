using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerBirdie : PlatformerCreature
{
    [Header("Birdy")]
    [SerializeField] private bool diveAttack;
    [SerializeField] private Vector2 raycastSize;
    [SerializeField] private Vector3 raycastOffset;
    [SerializeField] private float diveSpeed = 3;
    [SerializeField] private float rotationSpeed;
    [Header("References")]
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private LinearFlightCicle flightCicle;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ParticleSystem flightFX;
    [SerializeField] private float stunTime;

    public enum State { Idle, WindingUp, Diving, Attacking, Dizzy }
    public State state { get; private set; }

    public bool playerActivated { get; private set; }

    private Coroutine windUpCoroutine;
    private Coroutine angryCoroutine;

    private Vector2 direction;
    private float targetRotation;
    private bool stunned;
    private bool facingLeft;

    private void Start() 
    {
        facingLeft = !m_renderer.flipX;
    }

    private void OnDrawGizmos() 
    {
        if(diveAttack && state == State.Idle)
        {
            float directionModifier = !GetComponent<SpriteRenderer>().flipX ? 1 : -1;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + new Vector3(raycastOffset.x * directionModifier, raycastOffset.y), raycastSize);
        }
    }

    private void FixedUpdate()
    {
        float directionModifier = facingLeft ? 1 : -1;

        switch (state)
        {
            case State.Idle:
                if (!stunned && diveAttack)
                {
                    if (Physics2D.BoxCast(transform.position + new Vector3(raycastOffset.x * directionModifier, raycastOffset.y), 
                                          raycastSize, 
                                          0, Vector2.one, 0, 
                                          1 << LayerMask.NameToLayer("Player")))
                    {
                        windUpCoroutine = StartCoroutine(WindUpAnimation());
                    }
                }
                break;

            case State.Diving:
                m_rigidbody.velocity = direction * diveSpeed;
                float diff = Mathf.Abs (transform.rotation.eulerAngles.z - targetRotation);
                if (diff > Mathf.Abs(rotationSpeed))
                {
                    transform.Rotate(Vector3.forward * rotationSpeed * -directionModifier);
                    direction = RaposUtil.RotateVector(direction, rotationSpeed * -directionModifier);
                }
                else state = State.Attacking;
                break;
            
            case State.Attacking:
                m_rigidbody.velocity = direction * diveSpeed;
                break;

        }
    }

    private void ResetValues()
    {
        if (windUpCoroutine != null) StopCoroutine(windUpCoroutine);
        if (angryCoroutine != null) StopCoroutine(angryCoroutine);
        transform.rotation = Quaternion.Euler(Vector3.zero);
        flightCicle.enabled = true;
        m_renderer.flipY = false;
        m_animator.SetTrigger("Reset");
    }

    private IEnumerator WindUpAnimation()
    {
        state = State.WindingUp;
        m_animator.SetTrigger("WindUp");
        Vector3 windMovement = (Vector2.up * .01f) + (facingLeft ? Vector2.right : Vector2.left) * .02f;
        for(int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
            transform.position += windMovement;

        }

        DiveAttack((m_renderer.flipX ? Vector2.right : Vector2.left), facingLeft ? 1 : -1);
    }

    private void DiveAttack(Vector2 facingDirection, float directionModifier)
    {
        direction = (facingDirection / 3) + Vector2.down;
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

        //targetRotation = 90 + (15 * directionModifier);
        targetRotation = facingLeft ? 180 : 0;

        state = State.Diving;
    }

    private void StopDive()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        m_rigidbody.velocity = Vector2.zero;
        m_animator.SetTrigger("Reset");
        m_renderer.flipY = false;
        ChangeFacingLeft(!facingLeft);
        state = State.Idle;
    }

    private void ChangeFacingLeft(bool value) 
    {
        facingLeft = value;
        m_renderer.flipX = !facingLeft;
    }

    public override void OnStompEvent(PlayerPlatformer player)
    {
        if(stunned) return;

        base.OnStompEvent(player);

        ResetValues();

        if(state == State.Dizzy) StopAllCoroutines();

        player.SetEnemyJump();

        state = State.Idle;
        ResetValues();
        StartCoroutine(StunState (5 * new Vector2((!player.facingRight ? -1 : 1), .6f),
                                 (int)(stunTime * 60), 
                                 player.facingRight));
    }

    public override bool OnHitboxEvent(Hitbox hitbox)
    {
        if(stunned) return false;    

        state = State.Idle;
        ResetValues();

        bool goRight = hitbox.direction.x > 0;
        StartCoroutine(StunState (6 * new Vector2((!goRight ? -1 : 1), 1),
                                 (int)(stunTime * 60), 
                                 goRight));

        return false;
    }

    public override void OnTouchEvent(PlayerPlatformer player)
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

    private IEnumerator StunState(Vector2 launchDirection, int time, bool goRight)
    {
        stunned = true;
        m_animator.SetTrigger("Stunned");
        flightCicle.enabled = false;
        m_collider.enabled = false;

        m_rigidbody.gravityScale = .5f;
        m_rigidbody.velocity = launchDirection;

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

    protected override void OnTwirlEvent(Hitbox hitbox) 
    {
        base.OnTwirlEvent(hitbox);

        switch(state)
        {
            default:
            case State.Diving:
                playerActivated = true;
                if(direction.x != 0) AttackIntoDirection(direction.x > 0 ? Vector2.left : Vector2.right);
                else                 AttackIntoDirection(hitbox.direction);
                break;

            case State.Attacking:
                playerActivated = true;
                AttackIntoDirection(-direction);
                break;

            case State.Dizzy: 
                controller.Die();
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == State.Attacking || state == State.Diving)
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

                StartCoroutine(DeathTimer());
            }
        }
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(6);
        controller.Die();
    }
}
