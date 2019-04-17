using System.Collections;
using UnityEngine;

public class PlayerAirborneMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float breakSpeed;

    [Header("Jump")]
    [SerializeField] private float jumpInitialSpeed;
    [SerializeField] private float superJumpInitialSpeed;

    [Header("Effects")]
    [SerializeField] private GameObject jumpStartFX;

    private float horizontalMovement;
    private bool breaking;

    private bool fallingThroughPlatform;
    private LayerMask playerLayer;
    private LayerMask platformLayer;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private PlayerDreamPhase controller;

    [HideInInspector] public float horizontalInput;

    private void Awake()
    {
        controller = GetComponent<PlayerDreamPhase>();

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();

        playerLayer = LayerMask.NameToLayer("Player");
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void OnEnable()
    {
        m_animator.SetBool("Airborne", true);
    }

    public void Jump(bool super = false)
    {
        Instantiate(jumpStartFX, transform.position, Quaternion.identity);
        m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, super ? superJumpInitialSpeed : jumpInitialSpeed);
    }

    private void Update()
    {
        //lidando com ghost vertices
        //if (Mathf.Abs(lastGroundY - transform.position.y) > .1f)
        //{
        //    m_animator.SetBool("Airborne", true);
        //}

        m_animator.SetFloat("HorizontalSpeed", Mathf.Abs(horizontalMovement));
        m_animator.SetInteger("vSpeed", (int) m_rigidbody.velocity.y);
    }

    private void FixedUpdate()
    {
        Vector2 velocity = m_rigidbody.velocity;

        //if (Mathf.Abs(horizontalInput) > 0.9f)
        //{
        //    horizontalMovement = horizontalInput * horizontalSpeed * Time.fixedDeltaTime;
        //    breaking = false;
        //    controller.UpdateFacingDirection(horizontalMovement > 0 ? true : false);
        //}
        //else if (Mathf.Abs(horizontalMovement) > 0)
        //{
        //    breaking = true;
        //}

        horizontalMovement = horizontalInput * horizontalSpeed * Time.fixedDeltaTime;
        if (Mathf.Abs(horizontalInput) > 0.9f) controller.UpdateFacingDirection(horizontalMovement > 0 ? true : false);

        if (!fallingThroughPlatform)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, velocity.y > 0);
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

        velocity.x = horizontalMovement;

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

    private void OnDisable()
    {
        horizontalMovement = horizontalInput = 0;
    }
}
