using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterGoomba : ShooterCreature
{
    [Header("Goomba")]
    [SerializeField] private bool bloatOnDeath;
    [SerializeField] private float startingDelay;
    [SerializeField] private float shotsDelay = 1;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int bloatedHealth;
    [SerializeField] private LaunchMovement launchMovement;
    [SerializeField] private AudioSource shootSFX;

    public enum State { Idle, Bloated, Launched }
    public State state; 

    private bool locked;
    private PlayerPhaseManager player;
    private BulletPool pool;
    private Coroutine attackCoroutine;

    private void Start()
    {
        pool = BulletPoolIndexer.instance.GetPool("Line");
        player = PlayerPhaseManager.instance;
    }

    public override void SwitchIn(Nightmatrix nightmatrix)
    {
        base.SwitchIn(nightmatrix);

        facingRight = m_renderer.flipX = (transform.position.x < nightmatrix.transform.position.x);

        if (state == State.Bloated) SetBloatedState(); 

        if (nightmatrix.active)
        {
            SetAttackCicle();
        }
    }

    public override void SwitchOut()
    {
        base.SwitchOut();

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
    }

    public override void OnNotify()
    {
        base.OnNotify();

        if (currentNightmatrix != null)
        {
            if (currentNightmatrix.active)
            {
                SetAttackCicle();
            }
            else
            {
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            }
        }
    }

    private void SetAttackCicle()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        if (state == State.Idle)
        {
            attackCoroutine = StartCoroutine(AttackCicle());
        }
    }

    public override void OnHitboxEvent(Hitbox hitbox, float damage)
    {
        base.OnHitboxEvent(hitbox, damage);

        Bullet bullet = hitbox.GetComponent<Bullet>();
        if (bullet)
        {
            bullet.Vanish();
            hitSFX.Play();
        }

        if (gameObject.activeSelf) StartCoroutine(BlinkAnimation());

        controller.TakeDamage(damage);

        if (controller.GetHealth() < 1)
        {
            if (bloatOnDeath && state != State.Bloated)
            {
                SetBloatedState();
            }
            else
            {
                if (bloatOnDeath) SuicideShots();
                controller.Die();
            }
        }
    }

    private void SuicideShots()
    {
        int max = 8;
        for(int i = 0; i < max; i++)
        {
            GameObject bulletObject = pool.Get();
            BulletLine bullet = bulletObject.GetComponent<BulletLine>();
            if (bullet)
            {
                bulletObject.SetActive(true);
                Vector2 direction = RaposUtil.RotateVector(Vector2.up, (360/max) * i);

                bullet.Launch(direction * bulletSpeed);
                bulletObject.transform.position = transform.position;
            }
        }
    }

    public void SetBloatedState()
    {
        if(shooterMovement) shooterMovement.enabled = false;
        m_rigidbody.velocity = Vector2.zero;
        m_animator.SetTrigger("Bloat");
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        controller.SetHealth(bloatedHealth);
        StartCoroutine(DeathTimer());

        state = State.Bloated;
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(3);
        controller.Die();
    }

    public void SetLaunchedState(Vector2 movement)
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        if(shooterMovement) shooterMovement.enabled = false;
        m_animator.SetTrigger("Launch");

        if (movement.y == 0)
        { 
            m_renderer.flipX = (movement.x > 0);
        }
        else 
        {
            if (movement.x == 0) m_renderer.flipX = (movement.x > 0);
            else if (movement.x > 0) m_renderer.flipY = true;
            transform.rotation = Quaternion.Euler( Vector3.forward * (Vector2.SignedAngle(Vector2.up, movement) - 90) );
        }

        launchMovement.enabled = true;
        launchMovement.Launch(this, movement * 6);

        state = State.Launched;
    }

    public override void OnTouchEvent(PlayerShooter player)
    {
        base.OnTouchEvent(player);

        player.SetDamage(1);
    }

    protected override IEnumerator AttackCicle()
    {
        yield return new WaitForSeconds(startingDelay + .8f);

        while (true)
        {
            GameObject bulletObject = pool.Get();
            BulletLine bullet = bulletObject.GetComponent<BulletLine>();
            if (bullet)
            {
                bulletObject.SetActive(true);
                Vector2 direction;
                if (player)
                {
                    direction = (player.GetTarget().position - transform.position).normalized;
                }
                else
                {
                    direction = Vector2.left;
                }

                bullet.Launch(direction * bulletSpeed);
                bulletObject.transform.position = transform.position;

                m_animator.SetTrigger("Attack");
                shootSFX.Play();
            }

            yield return new WaitForSeconds(shotsDelay);
        }
    }

    public override void ChildHitboxEnterEvent(Collider2D collision, Hitbox hitbox) 
    {
        if(collision.CompareTag("Enemy"))
        {
            ShooterCreature shooterCreature = collision.GetComponent<ShooterCreature>();
            if(shooterCreature != null)
            {
                shooterCreature.OnHitboxEvent(hitbox, 20);
                //controller.Die();
            }
        }
    }

    protected override void OnWallEvent(Collision2D collision) 
    {
        base.OnWallEvent(collision);

        switch(state)
        {
            case State.Idle:
                shooterMovement.NotifyGround();
                break;

            case State.Launched:
                IBreakable breakableBlock = collision.gameObject.GetComponent<IBreakable>();
                if(breakableBlock != null)
                {
                    breakableBlock.TakeDamage(999);
                }
                controller.Die();
                break;
        }
    }
}
