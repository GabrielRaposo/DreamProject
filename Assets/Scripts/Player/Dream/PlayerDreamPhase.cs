using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerGroundMovement))]
public class PlayerDreamPhase : MonoBehaviour
{
    private const float MAX_Y = 15;

    [SerializeField] private float customGravity;
    [SerializeField] private int maxCoyoteTime;

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer damageFX;

    [Header("Audio Effects")]
    [SerializeField] private AudioSource jumpSFX;
    [SerializeField] private AudioSource damageSFX;

    private Vector3 pushForce;

    private const float BASE_GRAVITY = 2f;
    private const float LIGHT_GRAVITY = 1f;
    private float gravityModifier = BASE_GRAVITY;
    [HideInInspector] public bool gravityLock;

    private enum MovementState { Ground, Airborne, Zipping }
    private MovementState movementState;

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

    private PlayerPhaseManager controller; 

    public void Init(PlayerPhaseManager controller)
    {
        this.controller = controller;
    }

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();

        groundMovement = GetComponent<PlayerGroundMovement>();
        airborneMovement = GetComponent<PlayerAirborneMovement>();
        zippingMovement = GetComponent<PlayerZippingMovement>();
    }

    private void Start()
    {
        gravityModifier = BASE_GRAVITY;

        startingPosition = transform.position;
        UpdateFacingDirection(true);
        SetAirborneState();
    }

    private void HardReset()
    {
        StopAllCoroutines();
        m_renderer.enabled = true;
        onGround = invincible = stunned = gravityLock = false;
    }

    public void SwitchIn(Vector3 targetCenter, bool jumpOnExit)
    {
        HardReset();

        SetAirborneState();

        if(jumpOnExit)
        {
            SetJump();
        }
    }

    private void SetGroundState()
    {
        groundMovement.enabled = true;
        airborneMovement.enabled = false;
        zippingMovement.enabled = false;

        superJumping = false;

        movementState = MovementState.Ground;
    }

    private void SetAirborneState()
    {
        groundMovement.enabled = false;
        airborneMovement.enabled = true;
        zippingMovement.enabled = false;

        movementState = MovementState.Airborne;
    }

    private void SetZippingState(Zipline zipline)
    {
        zippingMovement.zipline = zipline;

        groundMovement.enabled = false;
        airborneMovement.enabled = false;
        zippingMovement.enabled = true;

        movementState = MovementState.Zipping;
    }

    void Update()
    {
        if (!stunned)
        {
            if(movementState != MovementState.Zipping)
            {
                if (onGround)
                {
                    if (movementState != MovementState.Ground) SetGroundState();
                }
                else
                {
                    if (movementState != MovementState.Airborne)
                    {
                        SetAirborneState();
                        coyoteTime = maxCoyoteTime;
                    }
                }
            }

            GetInputs();
        }

        gravityModifier = Input.GetButton("Jump") ? LIGHT_GRAVITY : BASE_GRAVITY;
    }

    private void GetInputs()
    {
        switch (movementState)
        {
            case MovementState.Ground:
                groundMovement.horizontalInput = Input.GetAxisRaw("Horizontal");
                groundMovement.verticalInput = Input.GetAxisRaw("Vertical");
                break;

            case MovementState.Airborne:
                airborneMovement.horizontalInput = Input.GetAxisRaw("Horizontal");
                break;

            case MovementState.Zipping:
                zippingMovement.horizontalInput = Input.GetAxisRaw("Horizontal");
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

            pushForce -= pushForce.normalized * .3f;
            if (pushForce.magnitude < .3f) pushForce = Vector3.zero;
        }

        if (Mathf.Abs(velocity.y) > MAX_Y) velocity.y = (velocity.y > 0) ? MAX_Y : -MAX_Y;

        m_rigidbody.velocity = velocity;
    }

    public void SetJumpInput(bool value)
    {
        if (inputLock) return;

        if(movementState != MovementState.Airborne || coyoteTime > 0)
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

    public void SetJump(bool super = false, float customMultiplier = 0)
    {
        jumpSFX.Play();

        SetAirborneState();
        if(customMultiplier == 0)
        {
            airborneMovement.Jump(super);
        }
        else
        {
            airborneMovement.CustomJump(customMultiplier);
        }
        superJumping = super;
    }

    public void SetEnemyJump()
    {
        jumpSFX.Play();

        SetAirborneState();
        airborneMovement.CustomJump(1.3f);
    }

    public void SetDamage(Vector3 contactPoint, int damage)
    {
        if (invincible) return;

        controller.TakeDamage(damage);

        if (transform.position.x < contactPoint.x)
        {
            UpdateFacingDirection(true);
        }
        else
        {
            UpdateFacingDirection(false);
        }

        if(gameObject.activeSelf) StartCoroutine(StunnedState());
    }

    private IEnumerator StunnedState()
    {
        groundMovement.enabled = false;
        airborneMovement.enabled = false;
        stunned = true;
        invincible = true;

        m_animator.SetBool("Stunned", true);
        m_rigidbody.velocity = Vector2.zero;

        damageFX.enabled = true;
        Time.timeScale = 0;
        damageSFX.Play();
        yield return new WaitForSecondsRealtime(.2f);
        Time.timeScale = 1;
        damageFX.enabled = false;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (controller.GetHealth() < 1) controller.Die();

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
        if (value)
        {
            if (m_rigidbody.velocity.y < 1)
            {
                onGround = true;
            }
        }
        else
        {
            if (movementState == MovementState.Ground && transform.position.y > collision.transform.position.y)
            {
                //para lidar com bug de coyote extendido plataformas OneWay com colisão estranha
                coyoteTime = maxCoyoteTime;
            }
            onGround = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //somente o filho "Ghost" consegue entrar em contato com "Enemy"s
        if (collision.transform.CompareTag("Enemy"))
        {
            Yume enemy = collision.transform.GetComponent<Yume>();
            if(enemy != null)
            {
                if ((transform.position - collision.transform.position).y > enemy.mininumTopY)
                {
                    enemy.OnStompEvent(this);
                }
                else
                {
                    enemy.OnTouchEvent(this);
                }
            }
        }
        else if (collision.CompareTag("Zipline"))
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
        else if (collision.transform.CompareTag("Hitbox"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox && hitbox.id != ID.Player)
            {
                SetDamage(collision.transform.position, hitbox.damage);
            }
        }
        else if (collision.CompareTag("Blastzone"))
        {
            controller.Die();
        }
        else if (collision.CompareTag("Exit"))
        {
            PlayerPhaseManager.gameManager.CallNextStage();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Nightmatrix"))
        {
            controller.SetNightmarePhase(collision.gameObject.GetComponent<Nightmatrix>());
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
        if(movementState != MovementState.Zipping)
        {
            pushForce = force;
        }
    }
}
