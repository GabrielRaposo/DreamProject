using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour, ICanTarget
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5;
    [SerializeField] [Range(0f, 1f)] private float speedModifier = .5f;
    [SerializeField] private float dashSpeed = 14;
    [SerializeField] private int dashDuration = 18;
    [SerializeField] private Animator wingLeftAnimator;
    [SerializeField] private Animator wingRightAnimator;
    [SerializeField] private AfterImageTrailEffect afterImageTrailEffect;

    [Header("Shooting")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int shotDelay;
    [SerializeField] private GameObject bulletPoolPrefab;
    [SerializeField] private AudioSource shotSFX;
    [SerializeField] private Transform aimStar;
    [SerializeField] private GameObject autoAimPrefab;

    [Space(10)]
    [SerializeField] private SpriteRenderer damageFX;

    private Vector2 movement;
    private BulletPool bulletPool;
    private Transform target;

    private bool locked;
    private bool shooting;
    private bool invincible;
    public bool facingRight { get; private set; }

    private Coroutine shootCicle;
    private Nightmatrix currentNightmatrix;

    public enum State { Idle, Dashing, Stunned }
    public State PlayerState { get; private set; }

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_renderer;
    
    private SpriteRenderer wingLeftRenderer;
    private SpriteRenderer wingRightRenderer;

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

        wingLeftRenderer = wingLeftAnimator.GetComponent<SpriteRenderer>();
        wingRightRenderer = wingRightAnimator.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        PlayerState = State.Idle;

        bulletPoolPrefab = Instantiate(bulletPoolPrefab);
        bulletPool = bulletPoolPrefab.GetComponent<BulletPool>();
        bulletPool.Init(ID.Player);

        GameObject autoAimObject = Instantiate(autoAimPrefab, transform.position + ((facingRight ? Vector3.right : Vector3.left) * -2), Quaternion.identity); 
        AutoAim autoAim = autoAimObject.GetComponent<AutoAim>();
        if(autoAim)
        {
            autoAim.Init(this);
            autoAim.GetComponent<InheritAnchorMovement>().Set(transform);
        }

        facingRight = true;
    }

    private void HardReset()
    {
        StopAllCoroutines();
        afterImageTrailEffect.StopAllCoroutines();
        m_renderer.enabled = wingLeftRenderer.enabled = wingRightRenderer.enabled = true;
        invincible = shooting = locked = false;
        PlayerState = State.Idle;
    }

    public void SwitchIn(Vector3 targetCenter, Nightmatrix nightmatrix)
    {
        HardReset();

        currentNightmatrix = nightmatrix;
        currentNightmatrix.Activate();
    }

    private void Update()
    {
        if (locked) return;
        
        switch(PlayerState)
        {
            case State.Idle:
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");

                if (Input.GetButtonDown("Attack"))
                {
                    shootCicle = StartCoroutine(ShootAction());
                    shooting = true;
                }
                else if (Input.GetButtonUp("Attack"))
                {
                    if (shootCicle != null) StopCoroutine(shootCicle);
                    shooting = false;
                }

                if (Input.GetButtonDown("Jump") && movement != Vector2.zero)
                {   
                    if (shootCicle != null) StopCoroutine(shootCicle);
                    shooting = false;

                    StartCoroutine(DashAction(movement));
                }
                break;

            default:
                movement = Vector2.zero;
                break;
        }
        
    }

    private void FixedUpdate()
    {
        if (locked) return;

        
        switch (PlayerState)
        {
            case State.Idle:
                m_animator.SetFloat("HorizontalMovement", movement.x);

                float animationSpeed = 1f; 
                if (movement.y > 0) animationSpeed = 1.5f; else 
                if (movement.y < 0) animationSpeed = .7f;

                wingLeftAnimator.speed = wingRightAnimator.speed = animationSpeed;

                m_rigidbody.velocity = movement * movementSpeed * (shooting ? speedModifier : 1);                
                break;
        }
        
    }

    private IEnumerator ShootAction()
    {
        while (true)
        {
            Shoot();
            yield return ShotDelay();
        }
    }

    private IEnumerator ShotDelay()
    {
        for (int i = 0; i < shotDelay; i++) yield return new WaitForFixedUpdate();
    }

    private void Shoot()
    {
        GameObject bulletObject = bulletPool.Get();
        BulletLine bullet = bulletObject.GetComponent<BulletLine>();
        if (bullet)
        {
            bulletObject.SetActive(true);
            Vector2 direction;
            if (target == null)
            {
                direction = (facingRight ? Vector2.right : Vector2.left) * bulletSpeed;
            }
            else 
            {
                direction = (target.position - transform.position).normalized * bulletSpeed;
            }
            bullet.Launch(direction);
            bulletObject.transform.position = aimStar.position;

            shotSFX.Play();
        }
    }

    private IEnumerator DashAction(Vector2 startingMovement)
    {
        PlayerState = State.Dashing;
        invincible = true;
        afterImageTrailEffect.StartCoroutine(afterImageTrailEffect.Call());

        m_rigidbody.velocity = startingMovement.normalized * dashSpeed; 

        for(int i = dashDuration; i > 0; i--)
        {
            yield return new WaitForFixedUpdate();
            m_rigidbody.velocity = startingMovement.normalized * dashSpeed * ((float)i/dashDuration);
        }

        m_rigidbody.velocity = Vector2.zero; 
        afterImageTrailEffect.StopAllCoroutines();
        yield return new WaitForSeconds(.1f);

        invincible = false;
        PlayerState = State.Idle;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerEnterAndStayEvents(collision);
    }

    private void OnTriggerStay2D(Collider2D collision) 
    {
        TriggerEnterAndStayEvents(collision);
    }

    private void TriggerEnterAndStayEvents(Collider2D collision)
    {
        //somente o filho "Ghost" consegue entrar em contato com "Enemy"s
        if (collision.transform.CompareTag("Enemy"))
        {
            Vector3 contactPoint = collision.transform.position;

            IShooterTouch enemy = collision.transform.GetComponent<IShooterTouch>();
            if (enemy != null)
            {
                enemy.OnTouchEvent(this);
            }
        }
        else if (collision.transform.CompareTag("Hitbox"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox && hitbox.id != ID.Player && PlayerState != State.Dashing)
            {
                SetDamage(hitbox.damage);

                Bullet bullet = collision.GetComponent<Bullet>();
                if (bullet)
                {
                    bullet.Vanish();
                }
            }
        }
        else if (!locked && collision.CompareTag("NightmatrixBorder"))
        {
            NightmatrixBorder border = collision.GetComponent<NightmatrixBorder>();
            if (border)
            {
                controller.SetDreamPhase(border.mainMatrix.GetComponent<Nightmatrix>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!locked && collision.CompareTag("Nightmatrix"))
        {
            controller.SetDreamPhase(collision.gameObject.GetComponent<Nightmatrix>());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            SetDamage(1);
        }
    }

    public void SetDamage(int damage)
    {
        if (invincible) return;
        controller.TakeDamage(damage);
        StartCoroutine(StunnedState());
    }

    private IEnumerator StunnedState()
    {
        PlayerState = State.Stunned;
        invincible = true;

        m_animator.SetBool("Stunned", true);
        m_rigidbody.velocity = Vector2.zero;

        damageFX.enabled = true;
        Time.timeScale = 0;
        damageFX.GetComponent<AudioSource>().Play();
        yield return new WaitForSecondsRealtime(.4f);
        Time.timeScale = 1;
        damageFX.enabled = false;
        if (shootCicle != null) StopCoroutine(shootCicle);
                shooting = false;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (controller.GetHealth() < 1) controller.Die();

        PlayerState = State.Idle;
        if (gameObject.activeSelf) StartCoroutine(InvencibilityTime());
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

            m_renderer.enabled = wingLeftRenderer.enabled = wingRightRenderer.enabled = !m_renderer.enabled;
        }
        m_renderer.enabled = wingLeftRenderer.enabled = wingRightRenderer.enabled = true;

        if(PlayerState != State.Dashing) invincible = false;
    }

    private void OnDisable()
    {
        if (currentNightmatrix)
        {
            currentNightmatrix.Deactivate();
            currentNightmatrix = null;
        }
    }

    public void SetTarget(Transform target) 
    {
        if (this.target == null || 
            Vector2.Distance(transform.position, target.position) < Vector2.Distance(transform.position, this.target.position))
        { 
            this.target = target;
        }
    }

    public void RemoveTarget(Transform target) 
    {
        if(this.target == target)
            this.target = null;
    }
}
