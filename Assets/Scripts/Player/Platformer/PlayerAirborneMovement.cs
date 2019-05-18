using System.Collections;
using UnityEngine;

public class PlayerAirborneMovement : MonoBehaviour
{
    [SerializeField] private TwirlHitbox attackHitbox;

    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float attackMultiplier;

    [Header("Jump")]
    [SerializeField] private float jumpInitialSpeed;
    [SerializeField] private float superJumpInitialSpeed;

    [Header("Effects")]
    [SerializeField] private GameObject jumpStartFX;

    private float targetHorizontalSpeed;
    private bool attacking;

    private bool fallingThroughPlatform;
    private LayerMask playerLayer;
    private LayerMask platformLayer;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private PlayerPlatformer controller;

    [HideInInspector] public float horizontalInput;

    private void Awake()
    {
        controller = GetComponent<PlayerPlatformer>();

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();

        playerLayer = LayerMask.NameToLayer("Player");
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void OnEnable()
    {
        m_animator.SetBool("Airborne", true);
    }

    public void Jump(float customMultiplier = 0)
    {
        if(controller.actionState == PlayerPlatformer.ActionState.Attacking)
        {
            attackHitbox.Dettach();
            m_animator.SetTrigger("Reset");
            controller.EndAttack();
        }

        Instantiate(jumpStartFX, transform.position, Quaternion.identity);
        if(customMultiplier == 0)
            m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, jumpInitialSpeed);
        else 
            m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, jumpInitialSpeed * customMultiplier);
    }

    private void Update()
    {
        //lidando com ghost vertices
        //if (Mathf.Abs(lastGroundY - transform.position.y) > .1f)
        //{
        //    m_animator.SetBool("Airborne", true);
        //}

        m_animator.SetFloat("HorizontalSpeed", Mathf.Abs(horizontalInput));
        m_animator.SetInteger("vSpeed", (int) m_rigidbody.velocity.y);
    }

    private void FixedUpdate()
    {
        Vector2 velocity = m_rigidbody.velocity;

        targetHorizontalSpeed = horizontalInput * horizontalSpeed * (attacking ? attackMultiplier : 1);

        float diff = velocity.x - targetHorizontalSpeed;
        if(Mathf.Abs(diff) > acceleration)
        {
            velocity.x += (diff > 0 ? -1 : 1) * acceleration;
        }
        else
        {
            velocity.x = targetHorizontalSpeed;
        }

        if (!attacking && Mathf.Abs(horizontalInput) > 0.9f)
        { 
            controller.UpdateFacingDirection(horizontalInput > 0 ? true : false);
        }

        if (!fallingThroughPlatform)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, velocity.y > 0);
        }

        m_rigidbody.velocity = velocity;
    }

    //Para funcionar, o "Use Collider Mask" da plataforma deve estar desligado
    public IEnumerator FallThroughPlatform()
    {
        fallingThroughPlatform = true;
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, true);
        for (int i = 0; i < 20; i++) yield return new WaitForEndOfFrame();
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, false);
        fallingThroughPlatform = false;
    }

    public void SetAttack(Vector2 direction)
    {
        attackHitbox.SetDirection(controller.facingRight ? Vector2.right : Vector2.left, false);
        StartCoroutine(AttackAction(direction));
    }

    public IEnumerator AttackAction(Vector2 direction)
    {
        attacking = true;
        m_rigidbody.velocity = Vector2.up * 4 + direction * horizontalSpeed;
        //m_rigidbody.velocity = Vector2.up * 3;
        m_animator.SetTrigger("Attack");
        attackHitbox.SetTarget(transform);

        yield return new WaitForSeconds(.5f);
        CancelAttack();
    }

    public void CancelAttack()
    {
        attackHitbox.Dettach();
        m_animator.SetTrigger("Reset");
        controller.EndAttack();
        attacking = false;
    }

    private void OnDisable()
    {
        attacking = false;
        targetHorizontalSpeed = horizontalInput = 0;
        attackHitbox.gameObject.SetActive(false);
        controller.EndAttack();
    }
}
