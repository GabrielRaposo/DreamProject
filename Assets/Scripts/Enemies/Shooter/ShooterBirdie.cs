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
    [SerializeField] private ParticleSystem trailFX;
    [SerializeField] private AudioSource chargeSFX;

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

        facingRight = m_renderer.flipX = (transform.position.x < nightmatrix.transform.position.x);

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

        yield return new WaitForSeconds(.8f);

        targetAim.GetComponent<AudioSource>().Stop();
        followTransform.enabled = false;

        m_animator.SetTrigger("PreAttack");
        m_rigidbody.velocity = (facingRight ? Vector2.right : Vector2.left) * .2f;
        yield return new WaitForSeconds(.4f);

        chargeSFX.Play();
        SetAttack();
    }

    public void SetAttack()
    {
        attacking = true;
        m_animator.SetTrigger("Attack");
        trailFX.Play();

        linearMovement.enabled = true;
        Vector2 direction = Vector2.left;
        direction = (targetAim.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.Euler((Vector2.SignedAngle(Vector2.left, direction)  + (facingRight ? 180 : 0)) * Vector3.forward);
        
        linearMovement.SetDirection(direction, targetAim.transform);
    }

    public void SetDirectionalAttack(Vector2 direction, bool playerActivated)
    {
        attacking = true;
        m_animator.SetTrigger("Attack");
        trailFX.Play();

        linearMovement.enabled = true;
        transform.rotation = Quaternion.Euler(Vector2.SignedAngle(Vector2.left, direction) * Vector3.forward);
        
        linearMovement.SetDirection(direction);

        launchHitbox.GetComponent<Collider2D>().enabled = playerActivated;
    }

    public void Explode()
    {
        if (targetAim) 
        {
            targetAim.GetComponent<AudioSource>().Stop();
            if(targetAim.activeSelf) targetAim.GetComponent<Animator>().SetTrigger("Fade");
            Destroy(targetAim, .7f);
        }
        explosion.transform.parent = null;
        explosion.SetActive(true);

        controller.Die();
    }

    public override void OnHitboxEvent(Hitbox hitbox, float damage)
    {
        base.OnHitboxEvent(hitbox, damage);

        Bullet bullet = hitbox.GetComponent<Bullet>();
        if (bullet)
        {
            bullet.Vanish();
            linearMovement.MoveBack();
            hitSFX.Play();
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
