using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerZippingMovement : MonoBehaviour
{
    [SerializeField] private ParticleSystem slideFX;
    [SerializeField] private AudioSource slideSFX;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private PlayerPlatformer controller;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public Zipline zipline;

    private void Awake()
    {
        controller = GetComponent<PlayerPlatformer>();

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        slideFX.gameObject.SetActive(true);
        ParticleSystem.MainModule mainModule = slideFX.main;
        mainModule.startColor = zipline.GetComponent<SpriteRenderer>().color;
        slideFX.Play();
        slideSFX.Play();

        if(zipline) controller.UpdateFacingDirection(zipline.Movement().x > 0);
        m_animator.SetTrigger("Reset");
        m_animator.SetBool("Airborne", true);
    }

    private void FixedUpdate()
    {
        //if (Mathf.Abs(horizontalInput) > 0.9f)
        //{
        //    controller.UpdateFacingDirection(horizontalInput > 0 ? true : false);
        //}

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

        slideFX.Stop();
        slideSFX.Stop();

        zipline.Disabled = true;
        zipline = null;
    }
}
