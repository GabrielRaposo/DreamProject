using System.Collections;
using UnityEngine;

public class PlayerGroundMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float breakSpeed;

    [Header("Crounching")]
    [SerializeField] private BoxCollider2D highCollider;
    [SerializeField] private float crouchSpeedModifier;

    [Header("Effects")]
    [SerializeField] private GameObject landingSmokeFX;
    [SerializeField] private ParticleSystem smokeTrailFX;

    private float horizontalMovement;
    private bool breaking;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private PlayerDreamPhase controller;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;
    [HideInInspector] public bool crouching;

    private void Awake()
    {
        controller = GetComponent<PlayerDreamPhase>();

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        breaking = false;
        m_animator.SetBool("Airborne", false);
        Instantiate(landingSmokeFX, transform.position, Quaternion.identity);
    }

    private void Update()
    {
        float absHorMove = Mathf.Abs(horizontalMovement);
        m_animator.SetFloat("HorizontalSpeed", absHorMove);
        
        if(absHorMove > 0) {
            if(!smokeTrailFX.isPlaying) smokeTrailFX.Play();
        }
        else smokeTrailFX.Stop();

        CheckCrouch();
        m_animator.SetBool("Crouching", crouching);
    }

    private void FixedUpdate()
    {
        Vector2 velocity = m_rigidbody.velocity;

        if (Mathf.Abs(horizontalInput) > 0.9f)
        {
            horizontalMovement = horizontalInput * horizontalSpeed * Time.fixedDeltaTime;
            breaking = false;
            controller.UpdateFacingDirection(horizontalMovement > 0 ? true : false);
        }
        else if (Mathf.Abs(horizontalMovement) > 0)
        {
            breaking = true;
        }

        if (breaking)
        {
            if (Mathf.Abs(velocity.x) > breakSpeed)
            {
                horizontalMovement -= velocity.normalized.x * breakSpeed;
            }
            else
            {
                horizontalMovement = 0;
                breaking = false;
            }
        }

        velocity.x = horizontalMovement * (crouching ? crouchSpeedModifier : 1);

        //gambiarra temporária
        if (velocity.y > 0 && velocity.y < 1)
        {
            velocity.y = 0;
        }

        m_rigidbody.velocity = velocity;
    }

    private void CheckCrouch()
    {
        if (verticalInput < 0)
        {
            crouching = true;
            highCollider.enabled = false;
        }
        else if (!Physics2D.OverlapCircle(transform.position, .25f, 1 << LayerMask.NameToLayer("Ground")))
        {
            crouching = false;
            highCollider.enabled = true;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        smokeTrailFX.Stop();
        horizontalMovement = horizontalInput = verticalInput = 0;
        m_animator.SetBool("Crouching", crouching = false);
        highCollider.enabled = true;
    }
}
