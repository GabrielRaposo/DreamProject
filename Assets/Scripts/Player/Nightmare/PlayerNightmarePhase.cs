using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNightmarePhase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] [Range(0f, 1f)] private float speedModifier;

    [Header("Shooting")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int shotDelay;
    [SerializeField] private GameObject bulletPoolPrefab;
    [SerializeField] private AudioSource shotSFX;

    [Space(10)]
    [SerializeField] private SpriteRenderer damageFX;

    private Vector2 movement;
    private BulletPool bulletPool;

    private bool locked;
    private bool shooting;
    private bool stunned;
    private bool invincible;
    public bool facingRight { get; private set; }

    private Coroutine shootCicle;
    private Nightmatrix currentNightmatrix;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_renderer;

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
    }

    private void Start()
    {
        bulletPoolPrefab = Instantiate(bulletPoolPrefab);
        bulletPool = bulletPoolPrefab.GetComponent<BulletPool>();
        bulletPool.Init(ID.Player);

        facingRight = true;
    }

    private void HardReset()
    {
        StopAllCoroutines();
        m_renderer.enabled = true;
        invincible = stunned = shooting = false;
    }

    public void SwitchIn(Vector3 targetCenter, Nightmatrix nightmatrix)
    {
        HardReset();

        currentNightmatrix = nightmatrix;
        currentNightmatrix.Activate();
    }

    private void Update()
    {
        if (!locked)
        {
            if (!stunned)
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");

                if (Input.GetButtonDown("Attack"))
                {
                    shootCicle = StartCoroutine(ShootCicle());
                    shooting = true;
                }
                else if (Input.GetButtonUp("Attack"))
                {
                    if (shootCicle != null) StopCoroutine(shootCicle);
                    shooting = false;
                }
            }
            else
            {
                movement = Vector2.zero;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!locked)
        {
            m_animator.SetFloat("VerticalMovement", movement.y);
            m_rigidbody.velocity = movement * movementSpeed * (shooting ? speedModifier : 1);
        }
    }

    private IEnumerator ShootCicle()
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
            bullet.Launch((facingRight ? Vector2.right : Vector2.left) * bulletSpeed);
            bulletObject.transform.position = transform.position;

            shotSFX.Play();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //somente o filho "Ghost" consegue entrar em contato com "Enemy"s
        if (collision.transform.CompareTag("Enemy"))
        {
            Vector3 contactPoint = collision.transform.position;

            Akumu enemy = collision.transform.GetComponent<Akumu>();
            if (enemy != null)
            {
                enemy.OnTouchEvent(this);
            }
        }
        else if (collision.transform.CompareTag("Hitbox"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox && hitbox.id != ID.Player)
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
                controller.SetDreamPhase(border.mainMatrix);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!locked && collision.CompareTag("Nightmatrix"))
        {
            controller.SetDreamPhase(collision.gameObject);
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
        stunned = true;
        invincible = true;

        m_animator.SetBool("Stunned", true);
        m_rigidbody.velocity = Vector2.zero;

        damageFX.enabled = true;
        Time.timeScale = 0;
        damageFX.GetComponent<AudioSource>().Play();
        yield return new WaitForSecondsRealtime(.2f);
        Time.timeScale = 1;
        damageFX.enabled = false;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (controller.GetHealth() < 1) controller.Die();

        stunned = false;
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

            m_renderer.enabled = !m_renderer.enabled;
        }
        m_renderer.enabled = true;

        invincible = false;
    }

    private void OnDisable()
    {
        if (currentNightmatrix)
        {
            currentNightmatrix.Deactivate();
            currentNightmatrix = null;
        }
    }
}
