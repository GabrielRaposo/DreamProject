using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour, ICanTarget, IHealable
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
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private BulletPool miniBulletPool;
    [SerializeField] private AudioSource shotSFX;
    [SerializeField] private Transform aimStar;
    [SerializeField] private GameObject autoAimPrefab;

    [Header("Effects")]
    [SerializeField] private ParticleSystem upgradeGetPS;
    [SerializeField] private SpriteRenderer damageFX;
    [SerializeField] private AudioSource dashSFX;

    private bool spreadShotsUpgrade;

    private Vector2 movement;
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

        bulletPool.Init(ID.Player);
        bulletPool.transform.parent = null;

        miniBulletPool.Init(ID.Player);
        miniBulletPool.transform.parent = null;

        //GameObject autoAimObject = Instantiate(autoAimPrefab, transform.position + ((facingRight ? Vector3.right : Vector3.left) * -2), Quaternion.identity); 
        //AutoAim autoAim = autoAimObject.GetComponent<AutoAim>();
        //if(autoAim)
        //{
        //    autoAim.Init(this);
        //    autoAim.GetComponent<InheritAnchorMovement>().Set(transform);
        //}
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

        //necessário por causa da interação com fallThorughPlatforms em PlayerAirborneMovement
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platform"), false);

        currentNightmatrix = nightmatrix;
        currentNightmatrix.Activate(this);
        UpdateFacingRight(!nightmatrix.invertedDirection);
    }

    public void UpdateFacingRight (bool facingRight)
    {
        this.facingRight = facingRight;
        m_renderer.flipX = !facingRight;
    }

    private void Update()
    {
        if (Time.timeScale == 0 || locked) return;
        
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
                else if (shooting && !Input.GetButton("Attack"))
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
                m_animator.SetFloat("HorizontalMovement", movement.x * (facingRight ? 1 : -1));

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
            BulletLine bullet = bulletPool.Get().GetComponent<BulletLine>();
            if (bullet) Shoot(bullet);
            
            if (spreadShotsUpgrade)
            {
                float offsetAngle = 15;
                bullet = miniBulletPool.Get().GetComponent<BulletLine>();
                if (bullet) Shoot(bullet, offsetAngle);
                
                bullet = miniBulletPool.Get().GetComponent<BulletLine>();
                if (bullet) Shoot(bullet, -offsetAngle);
            }

            for (int i = 0; i < shotDelay; i++) yield return new WaitForFixedUpdate();
        }
    }

    private void Shoot(BulletLine bullet, float offsetAngle = 0)
    {
        GameObject bulletObject = bullet.gameObject;
        
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

        direction = RaposUtil.RotateVector(direction, offsetAngle);

        bullet.Launch(direction);
        bulletObject.transform.position = aimStar.position;

        if(offsetAngle == 0) shotSFX.Play();
    }

    private IEnumerator DashAction(Vector2 startingMovement)
    {
        PlayerState = State.Dashing;
        invincible = true;
        afterImageTrailEffect.StartCoroutine(afterImageTrailEffect.Call(facingRight));
        dashSFX.Play();

        m_rigidbody.velocity = startingMovement.normalized * dashSpeed; 

        for(int i = dashDuration; i > 0; i--)
        {
            yield return new WaitForFixedUpdate();
            m_rigidbody.velocity = startingMovement.normalized * dashSpeed * ((float)i/dashDuration);
            if(i == 8) afterImageTrailEffect.StopAllCoroutines();
        }

        m_rigidbody.velocity = Vector2.zero; 
        invincible = false;

        for(int i = 0; i < 4; i++) yield return new WaitForFixedUpdate();

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
        else if (collision.transform.CompareTag("Hitbox") || collision.transform.CompareTag("Explosion"))
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

    public void SetDamage(float damage)
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
        yield return new WaitForSecondsRealtime(.2f);
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

    public void Heal(int value) 
    {
        controller.Heal(value);    
    }

    public void UnlockUpgrade(UpgradeType upgradeType)
    {
        upgradeGetPS.Play();
        upgradeGetPS.GetComponent<AudioSource>().Play();

        switch(upgradeType)
        {
            case UpgradeType.SpreadBullets:
                spreadShotsUpgrade = true;
                break;
        }
    }
}
