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

    [Header("Attack")]
    [SerializeField] private Hitbox hammerHitbox;
    [SerializeField] private AudioSource attackSFX; 

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
    }

    private void Update()
    {
        m_animator.SetFloat("HorizontalSpeed", Mathf.Abs(horizontalMovement));

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
        if(controller.attacking) m_animator.SetTrigger("Reset");
        hammerHitbox.gameObject.SetActive(false);
        controller.attacking = false;
        horizontalMovement = horizontalInput = verticalInput = 0;
        m_animator.SetBool("Crouching", crouching = false);
        highCollider.enabled = true;
    }

    public void SetAttackInput()
    {
        if (!crouching)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        controller.attacking = true;
        horizontalMovement = 0;
        horizontalInput = 0;

        hammerHitbox.direction = controller.facingRight ? Vector2.right : Vector2.left;
        Vector3 spawnOffset = new Vector3(1.1f * ((controller.facingRight) ? .8f : -.8f), -.2f);
        hammerHitbox.transform.localPosition = spawnOffset;
        hammerHitbox.transform.rotation = Quaternion.Euler(Vector3.up * (controller.facingRight ? 0 : 180));
        m_animator.SetTrigger("Attack");
        attackSFX.Play();

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        m_animator.SetTrigger("Reset");
        controller.attacking = false;
    }
}
