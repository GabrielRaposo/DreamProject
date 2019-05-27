using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerGroundMovement))]
[RequireComponent(typeof(PlayerAirborneMovement))]
[RequireComponent(typeof(PlayerZippingMovement))]
public class PlayerPlatformer : MonoBehaviour, IHealable
{
    private const float MAX_Y = 15;

    [SerializeField] private float customGravity;
    [SerializeField] private int maxCoyoteTime;

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer damageFX;
    [SerializeField] private GameObject stompFX;

    [Header("Audio Effects")]
    [SerializeField] private AudioSource jumpSFX;
    [SerializeField] private AudioSource stompSFX;
    [SerializeField] private AudioSource damageSFX;

    private Vector3 pushForce;

    private const float BASE_GRAVITY = 2.5f;
    private const float LIGHT_GRAVITY = 1.5f;
    private const float ATTACK_GRAVITY = 0.75f;
    private float gravityModifier = BASE_GRAVITY;

    private enum MovementState { Ground, Airborne, Zipping }
    private MovementState movementState;

    public enum ActionState { Idle, Attacking, Stunned }
    public ActionState actionState;

    private bool holdingJump;
    private bool maxJumpLock;
    private int coyoteTime;
    private int firstAirborneFrames;
    private bool invincible;
    private bool inputLock;
    private bool onAttackCooldown;

    private bool onGround;
    public bool facingRight { get; private set; }
    public Vector2 startingPosition { get; private set; }

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_renderer;

    private PlayerGroundMovement groundMovement;
    private PlayerAirborneMovement airborneMovement;
    private PlayerZippingMovement zippingMovement;

    private PlayerPhaseManager controller; 
    private CameraPriorityManager cameraPriorityManager;

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
        cameraPriorityManager = CameraPriorityManager.instance;

        actionState = ActionState.Idle;

        gravityModifier = BASE_GRAVITY;

        startingPosition = transform.position;
        UpdateFacingDirection(true);
        SetAirborneState();
    }

    private void HardReset()
    {
        StopAllCoroutines();
        m_renderer.enabled = true;
        onGround = invincible = maxJumpLock = onAttackCooldown = false;
        actionState = ActionState.Idle;    
    }

    public void SwitchIn(Vector3 targetCenter, Vector3 exitMovement, bool dashing, Vector2 shooterMovement)
    {
        HardReset();

        SetAirborneState();

        if (exitMovement.y > 0) // saindo por cima
        {
            maxJumpLock = true;
            Vector2 movement = Vector2.up * 7f;
            if (dashing) 
            {
                movement.y = 12f;
                if(shooterMovement.x != 0) movement.x = (shooterMovement.x > 0 ? 1 : -1) * 9f;
            }
            m_rigidbody.velocity = movement;
        }
        else if (exitMovement.y == 0) // saindo pelos lados
        {
            maxJumpLock = true;
            Vector2 movement = new Vector2((exitMovement.x > 0 ? 1 : -1) * 6f, 4f);
            if (dashing) 
            {
                movement.x = (exitMovement.x > 0 ? 1 : -1) * 10f;
                if(shooterMovement.y != 0) movement.y += (shooterMovement.y > 0 ? 1 : -1) * 5f;
            }
            m_rigidbody.velocity = movement;
        }
    }

    private void SetGroundState()
    {
        groundMovement.enabled = true;
        airborneMovement.enabled = false;
        zippingMovement.enabled = false;

        maxJumpLock = false;

        //cameraPriorityManager.SetFocus(CameraPriorityManager.GameState.PlatformGround);

        movementState = MovementState.Ground;
    }

    private void SetAirborneState()
    {
        if(actionState == ActionState.Attacking) 
        {
            if(groundMovement.enabled)   groundMovement.CancelAttack();
            if(airborneMovement.enabled) airborneMovement.CancelAttack();
            
            actionState = ActionState.Idle;
        }

        groundMovement.enabled = false;
        airborneMovement.enabled = true;
        zippingMovement.enabled = false;

        firstAirborneFrames = 3;

        //cameraPriorityManager.SetFocus(CameraPriorityManager.GameState.PlatformAirborne);

        movementState = MovementState.Airborne;
    }

    private void SetZippingState(Zipline zipline)
    {
        zippingMovement.zipline = zipline;

        groundMovement.enabled = false;
        airborneMovement.enabled = false;
        zippingMovement.enabled = true;

        EndAttack();

        //cameraPriorityManager.SetFocus(CameraPriorityManager.GameState.PlatformAirborne);

        movementState = MovementState.Zipping;
    }

    void Update()
    {
        //test -----------------------------
        //m_renderer.color = (onAttackCooldown ? Color.gray : Color.white);
        
        if (Time.timeScale == 0) return;

        if (actionState != ActionState.Stunned)
        {
            if(actionState == ActionState.Idle && movementState != MovementState.Zipping)
            {
                if (onGround)
                {
                    if (movementState != MovementState.Ground && firstAirborneFrames < 1) 
                    { 
                        SetGroundState();
                    }
                }
                else if (movementState != MovementState.Airborne)
                {
                    SetAirborneState();
                    coyoteTime = maxCoyoteTime;
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

        if (actionState == ActionState.Idle)
        {
            if (Input.GetButtonDown("Jump"))
            {
                gravityModifier = LIGHT_GRAVITY;
                SetJumpInput(true);
                holdingJump = true;
            }
            else if(Input.GetButtonUp("Jump")) StartCoroutine(WaitAndReleaseJump());
        
            if (Input.GetButtonDown("Attack"))
            {
                SetAttackInput();
            }
        }
    }

    private IEnumerator WaitAndReleaseJump()
    {
        for(int i = 0; i < 4; i++) yield return new WaitForFixedUpdate();
        holdingJump = false;
    }

    private void FixedUpdate()
    {
        if (coyoteTime > 0) coyoteTime--;
        if (firstAirborneFrames > 0) firstAirborneFrames--; //Para corrigir BUG: player reativa estado de ground assim que sai do chão
        
        Vector3 velocity = m_rigidbody.velocity;

        if (movementState == MovementState.Airborne)
        {
            float y;
            if(actionState != ActionState.Attacking)
            {
                if (maxJumpLock)
                {
                    y = velocity.y + (customGravity * LIGHT_GRAVITY * Physics2D.gravity.y * Time.fixedDeltaTime);
                }
                else 
                {
                    y = velocity.y + (customGravity * gravityModifier * Physics2D.gravity.y * Time.fixedDeltaTime);
                }
            }
            else
            {
                y = velocity.y + (customGravity * ATTACK_GRAVITY * Physics2D.gravity.y * Time.fixedDeltaTime);
            }
            velocity.y = y;
        }

        //if (pushForce != Vector3.zero)
        //{
        //    velocity.y += pushForce.y;
        //    transform.position += Vector3.right * pushForce.x * .01f;

        //    pushForce -= pushForce.normalized * .3f;
        //    if (pushForce.magnitude < .3f) pushForce = Vector3.zero;
        //}

        if (Mathf.Abs(velocity.y) > MAX_Y) velocity.y = (velocity.y > 0) ? MAX_Y : - MAX_Y;

        m_rigidbody.velocity = velocity;
    }

    public void SetJumpInput(bool value)
    {
        if (inputLock) return;

        //if(movementState != MovementState.Airborne || coyoteTime > 0)
        if(onGround || coyoteTime > 0 || movementState == MovementState.Zipping)
        {
            if (!groundMovement.crouching)
            {
                jumpSFX.Play();
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

    public void SetJump(float customMultiplier = 0, bool maxJumpLock = false)
    {
        this.maxJumpLock = maxJumpLock;

        SetAirborneState();
        airborneMovement.Jump(customMultiplier);
    }

    public void SetBounceJump(float customMultiplier = 0)
    {
        maxJumpLock = holdingJump;

        //jumpSFX.Play();

        SetAirborneState();
        airborneMovement.Jump(customMultiplier);
    }

    public void SetEnemyJump()
    {
        maxJumpLock = holdingJump;

        stompSFX.Play();
        Instantiate(stompFX, transform.position + Vector3.down, Quaternion.identity);

        SetAirborneState();
        airborneMovement.Jump(maxJumpLock ? 1.2f : 1f);
    }

    private void SetAttackInput()
    {
        if (onAttackCooldown) return;

        if (groundMovement.enabled)   
        {
            actionState = ActionState.Attacking;
            groundMovement.SetAttack(Input.GetAxisRaw("Horizontal") * Vector2.right); 
        } 
        else if (airborneMovement.enabled) 
        {
            actionState = ActionState.Attacking;
            airborneMovement.SetAttack(Input.GetAxisRaw("Horizontal") * Vector2.right);
        }
    }

    public void EndAttack()
    {
        if (actionState == ActionState.Attacking)
        { 
            if (gameObject.activeSelf) StartCoroutine(AttackCooldownTimer());
            actionState = ActionState.Idle;
        }

        //para corrigir bug em que é possível pular no frame que sai do ataque
        if (movementState == MovementState.Ground && !onGround) SetAirborneState();
    }

    private IEnumerator AttackCooldownTimer()
    {
        onAttackCooldown = true;
        yield return new WaitForSeconds(.2f);
        onAttackCooldown = false;
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
        onAttackCooldown = false;
        invincible = true;
        actionState = ActionState.Stunned;

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

        if (controller.GetHealth() > 0) 
        { 
            actionState = ActionState.Idle;
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
            m_animator.SetTrigger("Reset");
        } else controller.Die();

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
            IStompable enemy = collision.transform.GetComponent<IStompable>();
            if(enemy != null)
            {
                if ((transform.position - collision.transform.position).y > enemy.GetYStompRange())
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
        else if (collision.transform.CompareTag("Hitbox") || collision.transform.CompareTag("Explosion"))
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
            Window windowScript = collision.GetComponent<Window>();
            if (windowScript)
            {
                groundMovement.enabled = airborneMovement.enabled = zippingMovement.enabled = false;
                GetComponent<Collider2D>().enabled = false;

                m_animator.SetBool("Airborne", true);
                m_animator.SetFloat("HorizontalSpeed", 0f);
                m_animator.SetTrigger("Reset");

                controller.TravelThroughExit(collision.transform.position, windowScript);
            }
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

    public void Heal(int value) 
    {
        controller.Heal(value);
    }
}
