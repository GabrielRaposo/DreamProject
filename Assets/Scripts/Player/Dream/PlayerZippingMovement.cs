using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerZippingMovement : MonoBehaviour
{
    [SerializeField] private ParticleSystem slideFX;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private PlayerDreamPhase controller;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public Zipline zipline;

    private void Awake()
    {
        controller = GetComponent<PlayerDreamPhase>();

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        controller.gravityLock = true;

        slideFX.gameObject.SetActive(true);
        slideFX.startColor = zipline.GetComponent<SpriteRenderer>().color;
        slideFX.Play();

        m_animator.SetBool("Airborne", true);
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(horizontalInput) > 0.9f)
        {
            controller.UpdateFacingDirection(horizontalInput > 0 ? true : false);
        }

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
        controller.gravityLock = false;

        slideFX.Stop();

        zipline.Disabled = true;
        zipline = null;
    }
}
