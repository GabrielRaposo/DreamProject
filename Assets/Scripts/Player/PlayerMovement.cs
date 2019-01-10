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
    [SerializeField] private float jumpForce;
    [SerializeField] private float superJumpForce;
    [SerializeField] private float customGravity;
    [SerializeField] private float maxFallSpeed;

    [Header("Attack")]
    [SerializeField] private Hitbox hammerHitbox;
    [SerializeField] private GameObject gustPrefab;
    [SerializeField] private float gustSpeed;
    [SerializeField] private float airdashSpeed;

    private float horizontalMovement;
    private float horizontalInput;
    private bool breaking;
    private float verticalInput;
    private bool crouching;
    private float externalForce;
    private float gravityModifier;
    private bool inputLock;
    private Coroutine attackCoroutine;

    public bool facingRight { get; private set; }
    public bool stunned { get; private set; }
    public bool onGround { get; private set; }

    private Gust leftGust;
    private Gust rightGust;

    private ID id;
    private Animator animator;
    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;
    private PlayerController controller;

    private const float BASE_GRAVITY = 2f;
    private const float LIGHT_GRAVITY = 1f;

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

        id = ID.Player;
    }

    private void Start ()
    {
        gravityModifier = BASE_GRAVITY;

        if (healthDisplay)
        {
            healthDisplay.Init(maxHealth);
        }

        CreateGusts();
        UpdateFacingDirection(true);
        ResetValues();
    }
	
    private void CreateGusts()
    {
        GameObject lGust = Instantiate(gustPrefab, transform.position, Quaternion.identity);
        leftGust = lGust.GetComponent<Gust>();
        lGust.SetActive(false);

        GameObject rGust = Instantiate(gustPrefab, transform.position, Quaternion.identity);
        rightGust = rGust.GetComponent<Gust>();
        rGust.SetActive(false);
    }

    private void ResetValues()
    {
        controller.enabled = true;
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        breaking = false;
        hammerHitbox.gameObject.SetActive(false);
    }

    private void Update ()
    {
        CheckGround();

        animator.SetFloat("HorizontalSpeed", Mathf.Abs(horizontalMovement));
        if (!onGround)
        {
            animator.SetFloat("VerticalSpeed", rigidbody.velocity.y);
        }

        CheckCrouch();
        animator.SetBool("Crouching", crouching);
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rigidbody.velocity;
        if (breaking)
        {
            if (Mathf.Abs(velocity.x) > breakSpeed)
            {
                horizontalMovement -= velocity.normalized.x * breakSpeed * (!crouching ? 1f : .1f);
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
        velocity.x = horizontalMovement + externalForce;

        if (!onGround)
        {
            float y = velocity.y + (customGravity * gravityModifier * Physics2D.gravity.y * Time.fixedDeltaTime);
            if (y < maxFallSpeed) y = maxFallSpeed;
            velocity.y = y;
        }
        else
        {
            //gambiarra temporária
            if(velocity.y > 0 && velocity.y < 1)
            {
                velocity.y = 0;
            }
        }

        rigidbody.velocity = velocity;
    }

    public void CheckGround()
    {
        Vector2 axis = transform.position + (Vector3.down * .5f * transform.localScale.x);
        Vector2 border = new Vector2(.1f, .1f) * transform.localScale.x;

        if(rigidbody.velocity.y < .01f)
        {
            onGround = Physics2D.OverlapArea(axis - border, axis + border, groundLayer);
        }
        else onGround = false;            

        if (onGround)
        {
            gravityModifier = BASE_GRAVITY;
            animator.SetBool("Airborne", false);
        }
        else
        {
            animator.SetBool("Airborne", true);
        }
    }

    private void CheckCrouch()
    {
        if(verticalInput < 0 && onGround)
        {
            crouching = true;
            highCollider.enabled = false;
        }
        else
        {
            //check ceilling
            breaking = true;
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

    public void SetHorizontalMovement(float horizontalInput)
    {
        //if (crouching) return;
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

    public void SetJump(bool resetGravity, bool externalForce = false, bool super = false)
    {
        if (attackCoroutine != null)
        {
            animator.SetTrigger("Reset");
            ResetValues();
        }

        if (resetGravity)
        {
            gravityModifier = LIGHT_GRAVITY;
        }

        if (externalForce || onGround)
        {
            if (inputLock) return;
            StartCoroutine(LockInputs());

            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            rigidbody.AddForce(Vector2.up * jumpForce * (super ? 2 : 1));
        }
    }

    public void ReleaseJump()
    {
        gravityModifier = BASE_GRAVITY;
    }

    public void SetAttack()
    {
        if (inputLock) return;
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

        Vector3 spawnOffset = 1.1f * ((facingRight) ? Vector3.right : Vector3.left);
        hammerHitbox.transform.localPosition = spawnOffset;
        animator.SetTrigger("Attack");

        yield return WaitForFrames(20);

        animator.SetTrigger("Reset");
        controller.enabled = true;
    }

    //Chamado durante a animação "heroHammerGround"
    private void GenerateWindBoxes()
    {
        Vector3 spawnOffset = 1.1f * ((facingRight) ? Vector3.right : Vector3.left);
        LaunchGust(spawnOffset, rightGust, gustSpeed);
        LaunchGust(spawnOffset, leftGust, -gustSpeed);
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
        gravityModifier = 0;
        rigidbody.velocity = Vector3.zero;
        horizontalMovement = (facingRight ? 1 : -1) * airdashSpeed;
        breaking = false;

        Vector3 spawnOffset = 1.1f * ((facingRight) ? Vector3.right : Vector3.left);
        hammerHitbox.transform.localPosition = 1.1f * ((facingRight) ? Vector3.right : Vector3.left);
        animator.SetTrigger("Attack");

        yield return WaitForFrames(10);
        rigidbody.velocity = Vector3.zero;
        gravityModifier = BASE_GRAVITY;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Blastzone"))
        {
            Die();
        }
    }

    public void SetDamage(Vector3 contactPoint, int damage)
    {
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
        horizontalMovement = 0;
        breaking = true;

        stunned = true;
        animator.SetBool("Stunned", true);
        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        stunned = false;
        animator.SetBool("Stunned", false);
    }

    public void SetKnockback(Vector2 knockback)
    {
        externalForce = knockback.x;
    }
}
