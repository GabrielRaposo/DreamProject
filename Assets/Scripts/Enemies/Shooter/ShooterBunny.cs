using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBunny : ShooterCreature
{
    [Header("Bunny")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int bulletsPerCicle = 6;

    private bool offsetCicle;
    private BulletPool pool;
    private Coroutine attackCoroutine;

    private List<Bullet> spawnedBullets;

    private void Start()
    {
        pool = BulletPoolIndexer.instance.GetPool("Carrot");
    }

    public override void SwitchIn(Nightmatrix nightmatrix)
    {
        base.SwitchIn(nightmatrix);

        if (nightmatrix.active)
        {
            SetAttackCicle();
        }
    }

    public override void SwitchOut()
    {
        base.SwitchOut();

        if (attackCoroutine != null) 
        {
            StopCoroutine(attackCoroutine);
            ReleaseBullets();
        }
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
        }
    }

    private void SetAttackCicle()
    {
        if (attackCoroutine == null)
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
            ReleaseBullets();
            controller.Die();
        }
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(3);
        controller.Die();
    }

    public override void OnTouchEvent(PlayerNightmarePhase player)
    {
        base.OnTouchEvent(player);

        player.SetDamage(1);
    }

    protected override IEnumerator AttackCicle()
    {
        yield return new WaitForSeconds(.5f);

        offsetCicle = false;

        while (true)
        {
            spawnedBullets = new List<Bullet>();

            m_animator.SetInteger("State", 1);

            for(int i = 0; i < bulletsPerCicle; i++)
            {
                GameObject bulletObject = pool.Get();
                BulletLine bullet = bulletObject.GetComponent<BulletLine>();
                if (bullet)
                {
                    bulletObject.SetActive(true);
                    Vector2 direction = RaposUtil.RotateVector(Vector3.up, i * (360/bulletsPerCicle) + (offsetCicle ? (180/bulletsPerCicle) : 0));

                    bulletObject.transform.rotation = Quaternion.Euler(Vector3.forward * (Vector2.SignedAngle(Vector2.up, direction) + 90));
                    bulletObject.transform.position = transform.position;
                    InheritAnchorMovement inheritAnchorMovement = bullet.GetComponent<InheritAnchorMovement>();
                    if(inheritAnchorMovement != null) 
                    {
                        inheritAnchorMovement.enabled = true;
                        inheritAnchorMovement.Set(transform);
                    }
                    bullet.StartCoroutine(PositionBullet(bulletObject.transform, (Vector3)(direction * 1f)));

                    spawnedBullets.Add(bullet);
                    yield return new WaitForSeconds(.03f);
                }
            }

            yield return new WaitForSeconds(.5f);

            ReleaseBullets();
            m_animator.SetInteger("State", 2);

            yield return new WaitForSeconds(1);
            offsetCicle = !offsetCicle;
        }
    }

    private IEnumerator PositionBullet(Transform t, Vector3 movement)
    {
        int steps = 10;
        Vector3 portion = movement / steps; 
        for(int i = 0; i < steps; i++)
        {
            yield return new WaitForFixedUpdate();
            t.position += portion;
        }
    }

    private void ReleaseBullets()
    {
        for(int i = 0; i < spawnedBullets.Count; i++)
        {
            Bullet bullet = spawnedBullets[i]; 
            if(bullet != null && bullet.isActiveAndEnabled)
            {
                InheritAnchorMovement inheritAnchorMovement = bullet.GetComponent<InheritAnchorMovement>();
                if(inheritAnchorMovement != null) 
                {
                    inheritAnchorMovement.enabled = false;
                }

                Vector2 direction = RaposUtil.RotateVector(Vector3.up, i * (360/bulletsPerCicle) + (offsetCicle ? (180/bulletsPerCicle) : 0));
                bullet.Launch(direction * bulletSpeed);
            }
        }
    }
}
