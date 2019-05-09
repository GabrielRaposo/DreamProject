using System.Collections;
using UnityEngine;

public class PlayerGroundMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float acceleration;

    [Header("Crounching")]
    [SerializeField] private BoxCollider2D highCollider;
    [SerializeField] private float crouchSpeedModifier;

    [Header("Effects")]
    [SerializeField] private GameObject landingSmokeFX;
    [SerializeField] private ParticleSystem smokeTrailFX;

    private float targetHorizontalSpeed;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private PlayerPlatformer controller;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;
    [HideInInspector] public bool crouching;

    private void Awake()
    {
        controller = GetComponent<PlayerPlatformer>();

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        m_animator.SetBool("Airborne", false);
        Instantiate(landingSmokeFX, transform.position, Quaternion.identity);
    }

    private void Update()
    {
        float absHorMove = Mathf.Abs(targetHorizontalSpeed);
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

        targetHorizontalSpeed = horizontalInput * horizontalSpeed;

        float diff = velocity.x - targetHorizontalSpeed;
        if(Mathf.Abs(diff) > acceleration)
        {
            velocity.x += (diff > 0 ? -1 : 1) * acceleration;
        }
        else
        {
            velocity.x = targetHorizontalSpeed;
        }

        if(Mathf.Abs(horizontalInput) > .9f)
        {
            controller.UpdateFacingDirection(horizontalInput > 0 ? true : false);
        }

        //horizontalMovement = horizontalInput * horizontalSpeed * Time.fixedDeltaTime;

        //if (targetHorizontalSpeed == 0)
        //{
        //    if (Mathf.Abs(velocity.x) > breakSpeed)
        //    {
        //        horizontalMovement -= velocity.normalized.x * breakSpeed;
        //    }
        //    else
        //    {
        //        horizontalMovement = 0;
        //    }
        //}

        velocity.x *= (crouching ? crouchSpeedModifier : 1);

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

    public void SetAttack(Vector2 direction)
    {
        StartCoroutine(AttackAction(direction));
    }

    public IEnumerator AttackAction(Vector2 direction)
    {
        m_rigidbody.velocity = direction * horizontalSpeed;
        m_animator.SetTrigger("Attack");
        yield return new WaitForSeconds(.4f);
        m_animator.SetTrigger("Reset");
        controller.EndAttack();
    }

    private void OnDisable()
    {
        targetHorizontalSpeed = 0;
        StopAllCoroutines();
        smokeTrailFX.Stop();
        horizontalInput = verticalInput = 0;
        m_animator.SetBool("Crouching", crouching = false);
        highCollider.enabled = true;
        controller.EndAttack();
    }
}
