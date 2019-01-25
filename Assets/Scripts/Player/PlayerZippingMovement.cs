using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerZippingMovement : MonoBehaviour
{
    [SerializeField] private ParticleSystem slideFX;
    [SerializeField] private Hitbox hammerHitbox;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private PlayerController controller;

    [HideInInspector] public Zipline zipline;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        controller.gravityLock = true;
        controller.UpdateFacingDirection(zipline.Movement().x > 0? true : false);

        slideFX.gameObject.SetActive(true);
        slideFX.startColor = zipline.GetComponent<SpriteRenderer>().color;
        slideFX.Play();

        m_animator.SetBool("Airborne", true);
    }

    private void FixedUpdate()
    {
        if (zipline)
        {
            m_rigidbody.velocity = zipline.Movement();
        }

        Vector3 diff = (zipline.targetPosition - transform.position).normalized;
        if ((zipline.Movement() + diff).magnitude < zipline.Movement().magnitude)
        {
            controller.SetJump();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (controller.attacking) m_animator.SetTrigger("Reset");
        hammerHitbox.gameObject.SetActive(false);
        controller.attacking = false;
        controller.gravityLock = false;

        slideFX.Stop();

        zipline.Disabled = true;
        zipline = null;
    }

    public void SetAttackInput()
    {
        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        controller.gravityLock = true;
        controller.attacking = true;

        hammerHitbox.direction = controller.facingRight ? Vector2.right : Vector2.left;
        Vector3 spawnOffset = new Vector3(1.1f * ((controller.facingRight) ? .7f : -.7f), -.5f);
        hammerHitbox.transform.localPosition = spawnOffset;
        m_animator.SetTrigger("Attack");

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        m_rigidbody.velocity = Vector3.zero;
        controller.gravityLock = false;

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        m_animator.SetTrigger("Reset");

        controller.attacking = false;
    }
}
