using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkumuGoomba : Akumu
{
    [Header("Goomba")]
    [SerializeField] private float bulletSpeed;

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

        if (nightmatrix.active)
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(AttackCicle());
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
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(AttackCicle());
            }
            else
            {
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            }
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

        StartCoroutine(BlinkAnimation());

        controller.TakeDamage(hitbox.damage);
        controller.CheckHealth();
    }

    public override void OnTouchEvent(PlayerNightmarePhase player)
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
                    direction = (player.GetTargetPosition() - transform.position).normalized;
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
