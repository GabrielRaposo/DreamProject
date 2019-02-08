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
    //shot delay
    [SerializeField] private GameObject bulletPoolPrefab;

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
        invincible = stunned = false;
    }

    public void SwitchIn(Vector3 targetCenter)
    {
        HardReset();

        StartCoroutine(MoveTowardsCenter(targetCenter));
    }

    private IEnumerator MoveTowardsCenter(Vector3 targetCenter)
    {
        locked = true;

        m_rigidbody.velocity = (targetCenter - transform.position).normalized * movementSpeed;
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        locked = false;
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
        for (int i = 0; i < 5; i++) yield return new WaitForFixedUpdate();
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
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //somente o filho "Ghost" consegue entrar em contato com "Enemy"s
        if (collision.transform.CompareTag("Enemy"))
        {
            Vector3 contactPoint = collision.transform.position;

            Nenemy enemy = collision.transform.GetComponent<Nenemy>();
            if (enemy != null)
            {
                enemy.OnTouchEvent(this);
            }
        }
    }

    public void SetDamage(int damage)
    {
        if (invincible) return;
        controller.ChangeHealth(damage);
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
        yield return new WaitForSecondsRealtime(.2f);
        Time.timeScale = 1;
        damageFX.enabled = false;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        controller.CheckHealth();

        stunned = false;
        StartCoroutine(InvencibilityTime());
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Nightmatrix"))
        {
            controller.SetDreamPhase(collision.gameObject);
        }
    }
}
