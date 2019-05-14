using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBirdie : ShooterCreature
{
    [Header("Birdy")]
    [SerializeField] private LinearMovement linearMovement;
    [SerializeField] private GameObject targetAim;
    [SerializeField] private GameObject explosion;
    [SerializeField] private Hitbox launchHitbox;

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
            if (currentNightmatrix.active && targetAim && !targetAim.activeSelf && !attacking)
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

    public void SetDirectionalAttack(Vector2 direction, bool playerActivated)
    {
        attacking = true;
        m_animator.SetTrigger("Attack");

        linearMovement.enabled = true;
        transform.rotation = Quaternion.Euler(Vector2.SignedAngle(Vector2.left, direction) * Vector3.forward);
        
        linearMovement.SetDirection(direction);

        launchHitbox.GetComponent<Collider2D>().enabled = playerActivated;
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

    public override void OnHitboxEvent(Hitbox hitbox, int damage)
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

    protected override void OnWallEvent(Collision2D collision) 
    {
        base.OnWallEvent(collision);

        if(attacking) 
            Explode();
        else 
            shooterMovement.NotifyGround();
    }

    public override void ChildHitboxEnterEvent(Collider2D collision, Hitbox hitbox) 
    {
        base.ChildHitboxEnterEvent(collision, hitbox);
       
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
}
