using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerBunny : PlatformerCreature
{
    [Header("Bunny")]
    [SerializeField] private PlatformerLeaper leaper;
    [SerializeField] private float stunTime;
    [SerializeField] private float jumpForce;

    private bool stunned;
    private Coroutine stunCoroutine;

    private void ResetValues()
    {
        stunned = false;
        leaper.enabled = true;
    }

    public override void OnStompEvent(PlayerPlatformer player)
    {
        if (stunned) return;    

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

    protected override void OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        if (gameObject.activeSelf) stunCoroutine = StartCoroutine(StunState(8, (int)(stunTime * 60), hitbox.direction.x > 0 ? true : false));
    }

    private IEnumerator StunState(float pushForce, int time, bool goRight)
    {
        stunned = true;
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
}
