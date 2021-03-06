﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBunny : ShooterCreature
{
    [Header("Bunny")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int bulletsPerCicle = 6;
    [SerializeField] private LaunchMovement launchMovement;

    public enum State { Idle, Launched }
    public State state;

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

        facingRight = m_renderer.flipX = (transform.position.x < nightmatrix.transform.position.x);

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
        if (state == State.Idle && attackCoroutine == null)
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
            ReleaseBullets();
            controller.Die();
        }
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
                    Vector2 direction = RaposUtil.RotateVector (Vector3.up, i * (360/bulletsPerCicle) + (offsetCicle ? (180/bulletsPerCicle) : 0));

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

    public void SetLaunchedState(Vector2 movement)
    {
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
}
