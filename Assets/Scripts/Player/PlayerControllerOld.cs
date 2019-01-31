using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerGroundMovement))]
public class PlayerControllerOld : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private int maxHealth;

    [Header("Gravity")]
    [SerializeField] private float customGravity;
    [SerializeField] private float maxFallSpeed;

    [Space(30)]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int maxCoyoteTime;

    [HideInInspector] public bool attacking;
    private float pushForce;

    private const float BASE_GRAVITY = 2f;
    private const float LIGHT_GRAVITY = 1f;
    private float gravityModifier = BASE_GRAVITY;
    [HideInInspector] public bool gravityLock;

    private enum State { Ground, Airborne, Zipping }
    private State state;
    private bool stunned;
    private int coyoteTime;
    private bool invincible;
    private bool inputLock;
    public bool onGround { get; private set; }
    public bool facingRight { get; private set; }
    public Vector2 startingPosition { get; private set; }

    private LayerMask playerLayer;
    private LayerMask platformLayer;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_renderer;

    private PlayerGroundMovement groundMovement;
    private PlayerAirborneMovement airborneMovement;

    private Vector3 lastPlatformPosition;
    private Transform currentPlatform;

    public static GameManager gameManager;
    public static PlayerController instance;

    private void Awake()
    {
        //if (instance == null)
        //{
        //    instance = this;
        //}
    }

    private void Start()
    {
        if (healthDisplay)
        {
            healthDisplay.Init(maxHealth);
        }

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();

        gravityModifier = BASE_GRAVITY;

        startingPosition = transform.position;
        UpdateFacingDirection(true);
        SetAirborneState();
    }

    void OnEnable()
    {
        groundMovement = GetComponent<PlayerGroundMovement>();
        airborneMovement = GetComponent<PlayerAirborneMovement>();

        playerLayer = LayerMask.NameToLayer("Player");
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void SetGroundState()
    {
        groundMovement.horizontalInput = Input.GetAxisRaw("Horizontal");

        groundMovement.enabled = true;
        airborneMovement.enabled = false;

        state = State.Ground;
    }

    private void SetAirborneState()
    {
        airborneMovement.horizontalInput = Input.GetAxisRaw("Horizontal");

        groundMovement.enabled = false;
        airborneMovement.enabled = true;

        state = State.Airborne;
    }

    void Update()
    {
        if (!stunned && !attacking)
        {
            if (onGround)
            {
                if (state != State.Ground) SetGroundState();
            }
            else
            {
                if (state != State.Airborne)
                {
                    SetAirborneState();
                    coyoteTime = maxCoyoteTime;
                }
            }

            GetInputs();
        }
    }

    private void GetInputs()
    {
        switch (state)
        {
            case State.Ground:
                groundMovement.horizontalInput = Input.GetAxisRaw("Horizontal");
                groundMovement.verticalInput = Input.GetAxisRaw("Vertical");

                if (Input.GetButtonDown("Attack"))
                {
                    groundMovement.SetAttackInput();
                }
                break;

            case State.Airborne:
                airborneMovement.horizontalInput = Input.GetAxisRaw("Horizontal");

                if (Input.GetButtonDown("Attack"))
                {
                    airborneMovement.SetAttackInput();
                }
                break;

            case State.Zipping:
                if (Input.GetButtonDown("Attack"))
                {
                    //zippingMovement.SetAttackInput();
                }
                break;
        }

        if (Input.GetButtonDown("Jump"))
        {
            gravityModifier = LIGHT_GRAVITY;
            SetJumpInput(true);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            gravityModifier = BASE_GRAVITY;
        }
    }

    private void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            MoveWithPlatform();
        }

        if (coyoteTime > 0)
        {
            coyoteTime--;
        }

        Vector2 velocity = m_rigidbody.velocity;

        if (!gravityLock)
        {
            float y = velocity.y + (customGravity * gravityModifier * Physics2D.gravity.y * Time.fixedDeltaTime);
            if (velocity.y < maxFallSpeed) velocity.y = maxFallSpeed;
            velocity.y = y;
        }

        m_rigidbody.velocity = velocity;
    }

    private void MoveWithPlatform()
    {
        if (lastPlatformPosition != currentPlatform.position)
        {
            Vector3 diff = currentPlatform.position - lastPlatformPosition;
            transform.position += diff;

            lastPlatformPosition = currentPlatform.position;
        }
    }

    public void SetJumpInput(bool value)
    {
        if (inputLock) return;

        if (state == State.Ground || coyoteTime > 0)
        {
            if (!groundMovement.crouching)
            {
                SetJump();
            }
            else if (currentPlatform != null)
            {
                if (currentPlatform.CompareTag("OneWay"))
                {
                    SetAirborneState();
                    airborneMovement.StartCoroutine(airborneMovement.FallThroughPlatform());
                    StartCoroutine(LockInputs());
                }
            }

        }
    }

    public void SetJump(bool super = false)
    {
        //if (attacking)
        //{
        //    m_animator.SetTrigger("Reset");
        //    ResetValues();
        //}

        SetAirborneState();
        airborneMovement.Jump(super);
    }

    public void SetAttackInput()
    {
        //if (inputLock) return;
        //StartCoroutine(LockInputs());

        //hammerHitbox.direction = facingRight ? Vector2.right : Vector2.left;
    }

    public void SetDamage(Vector3 contactPoint, int damage)
    {
        if (invincible) return;

        if (healthDisplay)
        {
            healthDisplay.ChangeValue(-damage);
        }

        Vector3 knockback;
        if (transform.position.x < contactPoint.x)
        {
            knockback = Vector3.left;
            UpdateFacingDirection(true);
        }
        else
        {
            knockback = Vector3.right;
            UpdateFacingDirection(false);
        }
        knockback *= 5;
        StartCoroutine(StunnedState(knockback));
    }

    private IEnumerator StunnedState(Vector3 knockback)
    {
        groundMovement.enabled = false;
        airborneMovement.enabled = false;
        stunned = true;
        m_rigidbody.velocity = knockback;
        m_animator.SetBool("Stunned", true);
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(InvencibilityTime());
        stunned = false;
        if (onGround)
        {
            SetGroundState();
        }
        else
        {
            SetAirborneState();
        }
        m_animator.SetBool("Stunned", false);
    }

    private IEnumerator InvencibilityTime()
    {
        invincible = true;

        int blinkCount = 18;
        for (int i = 0; i < blinkCount; i++)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            m_renderer.enabled = !m_renderer.enabled;
        }
        m_renderer.enabled = true;

        invincible = false;
    }

    public void UpdateFacingDirection(bool facingRight)
    {
        this.facingRight = facingRight;
        m_renderer.flipX = !facingRight;
    }

    public void SetPush(Vector2 pushForce)
    {
        this.pushForce = pushForce.x;
    }

    //Gambiarra temporária
    private IEnumerator LockInputs()
    {
        inputLock = true;
        for (int i = 0; i < 3; i++) yield return new WaitForEndOfFrame();
        inputLock = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionCheck(collision, true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionCheck(collision, false);
    }


    private void CollisionCheck(Collision2D collision, bool firstFrame)
    {
        int layer = collision.gameObject.layer;

        if (groundLayer == (groundLayer | (1 << layer)))
        {
            foreach (ContactPoint2D cp in collision.contacts)
            {
                Vector2 contactPoint = cp.point - (Vector2)transform.position;
                Vector2 limit = new Vector2(.1f, -.45f);

                if (contactPoint.y < limit.y && Mathf.Abs(contactPoint.x) < limit.x)
                {
                    if (m_rigidbody.velocity.y < 1)
                        onGround = true;

                    if (layer == platformLayer)
                    {
                        //transform.parent = collision.transform;
                        currentPlatform = collision.transform;
                        lastPlatformPosition = currentPlatform.position;

                        Collider2D coll = collision.transform.GetComponent<Collider2D>();
                        if (firstFrame && coll)
                        {
                            Vector2 position = transform.position;
                            position.y = collision.transform.position.y + coll.bounds.extents.y + .5f;
                            if (Mathf.Abs(transform.position.y - position.y) < .2f)
                            {
                                transform.position = position;
                            }
                        }
                    }

                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (groundLayer == (groundLayer | (1 << collision.gameObject.layer)))
        {
            onGround = false;
            if (transform.position.y > collision.transform.position.y)
            {
                //para lidar com bug de coyote extendido plataformas OneWay com colisão estranha
                coyoteTime = maxCoyoteTime;
            }
        }

        if (collision.transform == currentPlatform)
        {
            currentPlatform = null;
            //transform.parent = null;
        }
        int layer = collision.gameObject.layer;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Blastzone"))
        {
            Die();
        }
        else if (collision.CompareTag("Zipline"))
        {
            Zipline zipline = collision.GetComponent<Zipline>();
            if (zipline)
            {

            }
        }
    }

    private void Die()
    {
        if (gameManager) gameManager.RestartScene();
    }
}
