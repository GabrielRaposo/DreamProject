using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBirdie : ShooterCreature
{
    [Header("Birdy")]
    [SerializeField] private ChaserMovement chaserMovement;

    private bool locked;

    private PlayerPhaseManager player;

    private void Start()
    {
        player = PlayerPhaseManager.instance;
    }

    public override void SwitchIn(Nightmatrix nightmatrix)
    {
        base.SwitchIn(nightmatrix);

        if (nightmatrix.active)
        {
            StartCoroutine( StartleTime() );
        }
    }

    public override void SwitchOut()
    {
        base.SwitchOut();
    }

    public override void OnNotify()
    {
        base.OnNotify();

        if (currentNightmatrix != null)
        {
            if (currentNightmatrix.active)
            {
                StartCoroutine(StartleTime());
            }
        }
    }

    private IEnumerator StartleTime()
    {
        m_animator.SetTrigger("Startle");
        yield return new WaitForSeconds(.5f);
        SetAttack();
    }

    public void SetAttack()
    {
        m_animator.SetTrigger("Attack");

        chaserMovement.enabled = true;
        if (!player) player = PlayerPhaseManager.instance;
        chaserMovement.SetTarget(player.GetTarget());
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
        if (controller.GetHealth() < 1) controller.Die();
    }

    public override void OnTouchEvent(PlayerNightmarePhase player)
    {
        base.OnTouchEvent(player);

        player.SetDamage(1);
    }
}
