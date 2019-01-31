using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerGroundMovement))]
public class PlayerController : MonoBehaviour
{
    private const float MAX_Y = 15;

    [Header("Health")]
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private int maxHealth;

    [Header("Physics")]
    [SerializeField] private float customGravity;
    [SerializeField] private int maxCoyoteTime;

    [Space(10)]

    [SerializeField] private SpriteRenderer damageFX;

    [HideInInspector] public bool attacking;
    private Vector3 pushForce;

    private const float BASE_GRAVITY = 2f;
    private const float LIGHT_GRAVITY = 1f;
    private float gravityModifier = BASE_GRAVITY;
    [HideInInspector] public bool gravityLock;

    private enum State { Ground, Airborne, Zipping }
    private State state;
    private bool stunned;
    private bool superJumping;
    private int coyoteTime;
    private bool invincible;
    private bool inputLock;
    public bool onGround { get; private set; }
    public bool facingRight { get; private set; }
    public Vector2 startingPosition { get; private set; }

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_renderer;

    private PlayerGroundMovement groundMovement;
    private PlayerAirborneMovement airborneMovement;
    private PlayerZippingMovement zippingMovement;

    public static GameManager gameManager;
    public static PlayerController instance;

    void OnEnable()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();

        groundMovement = GetComponent<PlayerGroundMovement>();
        airborneMovement = GetComponent<PlayerAirborneMovement>();
        zippingMovement = GetComponent<PlayerZippingMovement>();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        if (healthDisplay)
        {
            healthDisplay.Init(maxHealth);
        }

        gravityModifier = BASE_GRAVITY;

        startingPosition = transform.position;
        UpdateFacingDirection(true);
        SetAirborneState();
    }

    private void SetGroundState()
    {
        groundMovement.enabled = true;
        airborneMovement.enabled = false;
        zippingMovement.enabled = false;

        superJumping = false;

        state = State.Ground;
    }

    private void SetAirborneState()
    {
        groundMovement.enabled = false;
        airborneMovement.enabled = true;
        zippingMovement.enabled = false;

        state = State.Airborne;
    }

    private void SetZippingState(Zipline zipline)
    {
        zippingMovement.zipline = zipline;

        groundMovement.enabled = false;
        airborneMovement.enabled = false;
        zippingMovement.enabled = true;

        state = State.Zipping;
    }

    void Update()
    {
        if (!stunned && !attacking)
        {
            if(state != State.Zipping)
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
            }

            GetInputs();
        }

        if (Input.GetButtonDown("Jump"))
        {
            gravityModifier = LIGHT_GRAVITY;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            gravityModifier = BASE_GRAVITY;
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
                zippingMovement.horizontalInput = Input.GetAxisRaw("Horizontal");

                if (Input.GetButtonDown("Attack"))
                {
                    zippingMovement.SetAttackInput();
                }
                break;
        }

        if (Input.GetButtonDown("Jump"))
        {
            gravityModifier = LIGHT_GRAVITY;
            SetJumpInput(true);
        }
    }

    private void FixedUpdate()
    {
        if (coyoteTime > 0)
        {
            coyoteTime--;
        }

        Vector3 velocity = m_rigidbody.velocity;

        if (!gravityLock)
        {
            float y;
            if (!superJumping) y = velocity.y + (customGravity * gravityModifier * Physics2D.gravity.y * Time.fixedDeltaTime);
            else               y = velocity.y + (customGravity * LIGHT_GRAVITY * Physics2D.gravity.y * Time.fixedDeltaTime);
            velocity.y = y;
        }

        if(pushForce != Vector3.zero)
        {
            velocity.y += pushForce.y;
            transform.position += Vector3.right * pushForce.x * .01f;

            pushForce -= pushForce.normalized * .1f;
            if (pushForce.magnitude < .1f) pushForce = Vector3.zero;
        }

        if (Mathf.Abs(velocity.y) > MAX_Y) velocity.y = (velocity.y > 0) ? MAX_Y : -MAX_Y;

        m_rigidbody.velocity = velocity;
    }

    public void SetJumpInput(bool value)
    {
        if (inputLock) return;

        if(state != State.Airborne || coyoteTime > 0)
        {
            if (!groundMovement.crouching)
            {
                SetJump();
            }
            else if (transform.parent != null)
            {
                if (transform.parent.CompareTag("OneWay"))
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
        SetAirborneState();
        airborneMovement.Jump(super);
        superJumping = super;
    }

    public void SetAttackInput()
    {
        //if (inputLock) return;
        //StartCoroutine(LockInputs());
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
        invincible = true;

        m_animator.SetBool("Stunned", true);
        m_rigidbody.velocity = knockback;

        damageFX.enabled = true;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(.2f);
        Time.timeScale = 1;
        damageFX.enabled = false;

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (healthDisplay.value < 1)
        {
            Die();
        }

        stunned = false;
        StartCoroutine(InvencibilityTime());
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

    //Gambiarra temporária
    private IEnumerator LockInputs()
    {
        inputLock = true;
        for (int i = 0; i < 3; i++) yield return new WaitForEndOfFrame();
        inputLock = false;
    }

    public void OnGround(bool value, Collider2D collision)
    {
        //onGround = value;
        if (value)
        {
            if (m_rigidbody.velocity.y < 1)
            {
                onGround = true;
            }
        }
        else
        {
            if (state == State.Ground && transform.position.y > collision.transform.position.y)
            {
                //para lidar com bug de coyote extendido plataformas OneWay com colisão estranha
                coyoteTime = maxCoyoteTime;
            }
            onGround = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Zipline"))
        {
            Zipline zipline = collision.GetComponent<Zipline>();
            if (zipline && !zipline.Disabled)
            {
                Vector3 snap = zipline.SnapedPosition(transform.position);
                snap -= transform.position;
                if (snap.magnitude < 1f)
                {
                    transform.position += snap;
                    SetZippingState(zipline);
                }
            }
        }
        else if (collision.CompareTag("Blastzone"))
        {
            Die();
        }
        else if (collision.CompareTag("Exit"))
        {
            gameManager.CallNextStage();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Windbox"))
        {
            Windbox windbox = collision.GetComponent<Windbox>();
            if (windbox)
            {
                SetPushForce(windbox.force);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            SetDamage(collision.contacts[0].point, 1);
        }
    }

    public void SetPushForce(Vector3 force)
    {
        if(state != State.Zipping)
        {
            pushForce = force;
        }
    }

    private void Die()
    {
        if (gameManager) gameManager.RestartScene();
        Destroy (gameObject);
    }
}
