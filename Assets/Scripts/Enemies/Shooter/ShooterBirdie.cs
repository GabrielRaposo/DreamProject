using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBirdie : ShooterCreature
{
    [Header("Birdy")]
    [SerializeField] private LinearMovement linearMovement;
    [SerializeField] private GameObject targetAim;
    [SerializeField] private GameObject explosion;

    private bool locked;
    [HideInInspector] public bool attacking;

    private PlayerPhaseManager player;

    private void Start()
    {
        player = PlayerPhaseManager.instance;
    }

    public override void SwitchIn(Nightmatrix nightmatrix)
    {
        base.SwitchIn(nightmatrix);

        if(attacking)
        {
            if (shooterMovement != null) 
            {
                shooterMovement.enabled = false;
            }
        } else 
        if (nightmatrix.active)
        {   
            StartCoroutine(StartleTime());
        }
    }

    public override void SwitchOut()
    {
        base.SwitchOut();

        if (targetAim && targetAim.activeSelf)
        {
            targetAim.GetComponent<Animator>().SetTrigger("Fade");
            Destroy(targetAim, .7f);
        }
    }

    public override void OnNotify()
    {
        base.OnNotify();

        if (currentNightmatrix != null)
        {
            if (currentNightmatrix.active && !targetAim.activeSelf && !attacking)
            {
                if (shooterMovement != null) 
                {
                    shooterMovement.enabled = false;
                }

                StartCoroutine(StartleTime());
            }
        }
    }

    private IEnumerator StartleTime()
    {
        m_animator.SetTrigger("Startle");

        m_rigidbody.velocity = Vector2.zero;

        Transform t = PlayerPhaseManager.instance.GetTarget();
        targetAim.transform.parent = null;
        targetAim.transform.position = t.position;
        targetAim.SetActive(true);
        FollowTransform followTransform = targetAim.GetComponent<FollowTransform>();
        followTransform.Follow(t);

        yield return new WaitForSeconds(1f);

        followTransform.enabled = false;

        SetAttack();
    }

    public void SetAttack()
    {
        attacking = true;
        m_animator.SetTrigger("Attack");

        linearMovement.enabled = true;
        Vector2 direction = Vector2.left;
        direction = (targetAim.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.Euler(Vector2.SignedAngle(Vector2.left, direction) * Vector3.forward);
        
        linearMovement.SetDirection(direction, targetAim.transform);
    }

    public void SetDirectionalAttack(Vector2 direction)
    {
        attacking = true;
        m_animator.SetTrigger("Attack");

        linearMovement.enabled = true;
        transform.rotation = Quaternion.Euler(Vector2.SignedAngle(Vector2.left, direction) * Vector3.forward);
        
        linearMovement.SetDirection(direction);
    }

    public void Explode()
    {
        if (targetAim) 
        {
            if(targetAim.activeSelf) targetAim.GetComponent<Animator>().SetTrigger("Fade");
            Destroy(targetAim, .7f);
        }
        explosion.transform.parent = null;
        explosion.SetActive(true);

        controller.Die();
    }

    protected override void OnHitboxEvent(Hitbox hitbox, int damage)
    {
        base.OnHitboxEvent(hitbox, damage);

        Bullet bullet = hitbox.GetComponent<Bullet>();
        if (bullet)
        {
            bullet.Vanish();
            linearMovement.MoveBack();
        }

        StartCoroutine(BlinkAnimation());

        controller.TakeDamage(damage);
        if (controller.GetHealth() < 1) Explode();
    }

    public override void OnTouchEvent(PlayerShooter player)
    {
        base.OnTouchEvent(player);

        player.SetDamage(1);
    }

    //generalizar depois
    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }
}
