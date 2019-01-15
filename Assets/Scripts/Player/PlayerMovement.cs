using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Health")]
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private int maxHealth;

    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float breakSpeed;

    [Header("Crounching")]
    [SerializeField] private BoxCollider2D highCollider;
    [SerializeField] private float crouchSpeedModifier;

    [Header("Jump")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpInitialSpeed;
    [SerializeField] private float superJumpInitialSpeed;
    [SerializeField] private float customGravity;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private int maxCoyoteTime;

    [Header("Attack")]
    [SerializeField] private Hitbox hammerHitbox;
    [SerializeField] private GameObject gustPrefab;
    [SerializeField] private float gustSpeed;
    [SerializeField] private float airdashSpeed;

    public Vector2 startingPosition { get; private set; }
    public bool facingRight { get; private set; }
    public bool stunned { get; private set; }
    public bool onGround { get; private set; }

    private float horizontalMovement;
    private float horizontalInput;
    private bool breaking;
    private float verticalInput;
    private bool crouching;
    private bool fallingThroughPlatform;
    private float externalForce;

    private const float BASE_GRAVITY = 2f;
    private const float LIGHT_GRAVITY = 1f;
    private float gravityModifier = BASE_GRAVITY;
    private float gravity;
    private bool gravityLock;
    private Vector2 currentGroundNormal;
    private int coyoteTime;

    private bool inputLock;
    private bool invincible;
    private Vector3 lastPlatformPosition;
    private Transform currentPlatform;
    private Coroutine attackCoroutine;
    private float lastGroundY;

    private LayerMask playerLayer;
    private LayerMask platformLayer;

    //private ID id;
    private Animator animator;
    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;
    private PlayerController controller;

    public static GameManager gameManager;
    public static PlayerMovement instance; 
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();

        playerLayer = LayerMask.NameToLayer("Player");
        platformLayer = LayerMask.NameToLayer("Platform");

        //id = ID.Player;
        startingPosition = transform.position;
    }

    private void Start ()
    {
        if (healthDisplay)
        {
            healthDisplay.Init(maxHealth);
        }

        UpdateFacingDirection(true);
        ResetValues();
    }

    private void ResetValues()
    {
        gravityLock = false;
        controller.enabled = true;
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        breaking = false;
        hammerHitbox.gameObject.SetActive(false);
    }

    private void Update ()
    {
        if (Mathf.Abs(horizontalInput) > 0.9f)
        {
            horizontalMovement = horizontalInput * horizontalSpeed * Time.fixedDeltaTime;
            breaking = false;
            UpdateFacingDirection(horizontalMovement > 0 ? true : false);
        }
        else if (Mathf.Abs(horizontalMovement) > 0)
        {
            breaking = true;
        }

        animator.SetFloat("HorizontalSpeed", Mathf.Abs(horizontalMovement));
        if (onGround)
        {
            animator.SetBool("Airborne", false);
            lastGroundY = transform.position.y;
        }
        else
        {
            //lidando com ghost vertices
            if (Mathf.Abs(lastGroundY - transform.position.y) > .1f)
            {
                animator.SetBool("Airborne", true);
            }

            animator.SetFloat("VerticalSpeed", rigidbody.velocity.y);
        }

        CheckCrouch();
        animator.SetBool("Crouching", crouching);
    }

    private void FixedUpdate()
    {
        if(coyoteTime > 0)
        {
            coyoteTime--;
        }

        if (currentPlatform != null)
        {
            MoveWithPlatform();
        }

        Vector2 velocity = rigidbody.velocity;

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
        if (Mathf.Abs(externalForce) > 0)
        {
            externalForce -= (externalForce * .2f);
            if(Mathf.Abs(externalForce) < .01f)
            {
                externalForce = 0;
            }
        }

        velocity.x = (horizontalMovement * (crouching ? crouchSpeedModifier : 1)) + externalForce;

        if (!onGround)
        {
            if (!gravityLock)
            {
                gravity = gravityModifier;
            }

            float y = velocity.y + (customGravity * gravity * Physics2D.gravity.y * Time.fixedDeltaTime);
            if (y < maxFallSpeed) y = maxFallSpeed;
            velocity.y = y;
        }
        else
        {
            //gambiarra temporária
            if (velocity.y > 0 && velocity.y < 1)
            {
                velocity.y = 0;
            }
        }

        rigidbody.velocity = velocity;
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

    private void CheckCrouch()
    {
        if(onGround)
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
        else
        {
            crouching = false;
            highCollider.enabled = true;
        }
    }

    private IEnumerator WaitForFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    private void Die()
    {
        if(gameManager) gameManager.RespawnPlayer();
    }

    public void SetHorizontalInput(float horizontalInput)
    {
        //if (crouching) return;
        this.horizontalInput = horizontalInput;
    }

    private void UpdateFacingDirection(bool facingRight)
    {
        this.facingRight = facingRight;
        renderer.flipX = !facingRight;
    }

    public void SetVerticalInput(float verticalInput)
    {
        this.verticalInput = verticalInput;
    }

    public void SetJumpInput(bool value)
    {
        if (value)
        {
            gravityModifier = LIGHT_GRAVITY;

            if (inputLock) return;

            if (!crouching)
            {
                if (onGround || coyoteTime > 0)
                {
                    SetJump();
                    StartCoroutine(LockInputs());
                }
            }
            else if (currentPlatform != null)
            {
                if (currentPlatform.CompareTag("OneWay"))
                {
                    StartCoroutine(FallThroughPlatform());
                    StartCoroutine(LockInputs());
                }
            }
        } else
        {
            gravityModifier = BASE_GRAVITY;
        }
    }

    public void SetJump(bool super = false)
    {
        if (attackCoroutine != null)
        {
            animator.SetTrigger("Reset");
            ResetValues();
        }

        rigidbody.velocity = new Vector2(rigidbody.velocity.x, super ? superJumpInitialSpeed : jumpInitialSpeed);
    }

    //Para funcionar, o Use Collider Mask da plataforma deve estar desligado
    private IEnumerator FallThroughPlatform()
    {
        fallingThroughPlatform = true;
        onGround = false;
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, true);
        for (int i = 0; i < 20; i++) yield return new WaitForEndOfFrame();
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, false);
        fallingThroughPlatform = false;
    }

    public void SetAttackInput()
    {
        if (inputLock || crouching) return;
        StartCoroutine(LockInputs());

        hammerHitbox.direction = facingRight ? Vector2.right : Vector2.left;

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        if (onGround)
        {
            attackCoroutine = StartCoroutine(LandAttackSequence());
        }
        else
        {
            attackCoroutine = StartCoroutine(AerialAttackSequence());
        }
    }

    private IEnumerator LandAttackSequence()
    {
        controller.enabled = false;
        horizontalMovement = 0;
        horizontalInput = 0;

        Vector3 spawnOffset = new Vector3(1.1f * ((facingRight) ? 1 : -1), -.2f);
        hammerHitbox.transform.localPosition = spawnOffset;
        animator.SetTrigger("Attack");

        yield return WaitForFrames(20);

        animator.SetTrigger("Reset");
        controller.enabled = true;
    }

    private void LaunchGust(Vector3 spawnOffset, Gust gust, float speed)
    {
        gust.transform.localPosition = transform.position + spawnOffset + (Vector3.down * .2f);
        //gust.transform.position += ((speed > 0) ? Vector3.right : Vector3.left) * .5f;
        gust.gameObject.SetActive(true);
        gust.Launch(speed);
    }

    private IEnumerator AerialAttackSequence()
    {
        controller.enabled = false;
        gravity = 0;
        gravityLock = true;
        rigidbody.velocity = Vector3.zero;
        horizontalMovement = (facingRight ? 1 : -1) * airdashSpeed;
        horizontalInput = 0;
        breaking = false;

        hammerHitbox.transform.localPosition = 1.1f * ((facingRight) ? Vector3.right : Vector3.left);
        animator.SetTrigger("Attack");

        yield return WaitForFrames(10);
        rigidbody.velocity = Vector3.zero;
        gravityLock = false;
        horizontalMovement = 0;
        yield return WaitForFrames(10);

        animator.SetTrigger("Reset");
        controller.enabled = true;
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

        if(groundLayer == (groundLayer | (1 << layer)))
        {
            foreach(ContactPoint2D cp in collision.contacts)
            {
                Vector2 contactPoint = cp.point - (Vector2) transform.position;
                Vector2 limit = new Vector2(.1f, -.45f);

                if (contactPoint.y < limit.y && Mathf.Abs(contactPoint.x) < limit.x)
                {
                    onGround = true;

                    if (layer == platformLayer)
                    {
                        currentPlatform = collision.transform;
                        //transform.parent = collision.transform;
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
            coyoteTime = maxCoyoteTime;
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
        if(collision.CompareTag("Blastzone"))
        {
            Die();
        }
    }

    public void SetDamage(Vector3 contactPoint, int damage)
    {
        if (invincible) return;

        if (healthDisplay)
        {
            healthDisplay.ChangeValue(-damage);
        }

        Vector3 knockback;
        if(transform.position.x < contactPoint.x)
        {
            knockback = Vector3.left;
            UpdateFacingDirection(true);
        }
        else
        {
            knockback = Vector3.right;
            UpdateFacingDirection(false);
        }
        knockback *= 15;
        knockback += Vector3.up * 8;
        ResetValues();
        StartCoroutine(StunnedState(knockback));
    }

    private IEnumerator StunnedState(Vector3 knockback)
    {
        rigidbody.velocity = Vector2.zero;
        rigidbody.velocity += Vector2.up * knockback.y;
        externalForce = knockback.x;
        horizontalInput = 0;
        breaking = true;

        stunned = true;
        animator.SetBool("Stunned", true);
        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(InvencibilityTime());
        stunned = false;
        animator.SetBool("Stunned", false);
    }

    public void SetExternalForce(Vector2 externalForce)
    {
        this.externalForce = externalForce.x;
    }

    private IEnumerator InvencibilityTime()
    {
        //Physics2D.IgnoreLayerCollision(playerLayer, enemyPlayer, true);
        invincible = true;

        int blinkCount = 18;
        for(int i = 0; i < blinkCount; i++)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            renderer.enabled = !renderer.enabled;
        }
        renderer.enabled = true;

        //Physics2D.IgnoreLayerCollision(playerLayer, enemyPlayer, false);
        invincible = false;
    }
}
