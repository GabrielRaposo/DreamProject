using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterGoomba : ShooterCreature
{
    [Header("Goomba")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int bloatedHealth;

    public bool vulnerable { get; private set; }
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

        if (vulnerable) SetVulnerableState(); 

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

        if (!vulnerable)
        {
            attackCoroutine = StartCoroutine(AttackCicle());
        }
    }

    protected override void OnHitboxEvent(Hitbox hitbox)
    {
        base.OnHitboxEvent(hitbox);

        Bullet bullet = hitbox.GetComponent<Bullet>();
        if (bullet)
        {
            bullet.Vanish();
        }

        if (gameObject.activeSelf) StartCoroutine(BlinkAnimation());

        controller.TakeDamage(hitbox.damage);

        if (controller.GetHealth() < 1)
        {
            if (!vulnerable)
            {
                SetVulnerableState();
            }
            else
            {
                SuicideShots();
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

    public void SetVulnerableState()
    {
        m_animator.SetTrigger("Bloat");
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        controller.SetHealth(bloatedHealth);
        StartCoroutine(DeathTimer());

        vulnerable = true;
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(3);
        controller.Die();
    }

    public override void OnTouchEvent(PlayerShooter player)
    {
        base.OnTouchEvent(player);

        player.SetDamage(1);
    }

    protected override IEnumerator AttackCicle()
    {
        yield return new WaitForSeconds(.5f);

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
            }

            yield return new WaitForSeconds(1);
        }
    }
}
