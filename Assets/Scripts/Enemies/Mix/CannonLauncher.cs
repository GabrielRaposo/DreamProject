using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonLauncher : MonoBehaviour
{
    [SerializeField] private float startingDelay;
    [SerializeField] private float bulletSpeed = 2f;
    [SerializeField] private float shotDelay = 2f;
    [SerializeField] private ParticleSystem shotFX;

    private Animator m_animator;
    private Rigidbody2D m_rigidbody;
    private Coroutine fallCoroutine;

    private BulletPool pool;

    private void Awake() 
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        pool = BulletPoolIndexer.instance.GetPool("Cannon");
        m_animator = GetComponent<Animator>();

        if(pool != null)
        {
            StartCoroutine(AttackLoop());
        }
    }

    private IEnumerator AttackLoop()
    {
        if(startingDelay > 0) yield return new WaitForSeconds(startingDelay);

        while (true) 
        {
            yield return new WaitForSeconds(shotDelay);
            Shoot();
        }
    }

    private void Shoot()
    {
        m_animator.SetTrigger("Shoot");
        shotFX.Play();

        GameObject bulletObject = pool.Get();
        bulletObject.transform.position = transform.position;
        bulletObject.SetActive(true);

        CannonBall bulletScript = bulletObject.GetComponent<CannonBall>();
        bulletScript.Launch(RaposUtil.RotateVector(Vector2.up, transform.rotation.eulerAngles.z) * bulletSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Nightmatrix"))
        {
            if(fallCoroutine != null) StopCoroutine(fallCoroutine);
            fallCoroutine = StartCoroutine(BrakeFall());
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(collision.CompareTag("Nightmatrix"))
        {
            m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
            m_rigidbody.velocity = Vector2.zero;
            m_rigidbody.gravityScale = 1;
        }
    }

    private IEnumerator BrakeFall()
    {
        m_rigidbody.gravityScale = 0;
        while (m_rigidbody.velocity.y < 0)
        {
            yield return new WaitForFixedUpdate();
            m_rigidbody.velocity += Vector2.up * .7f;
        }
        m_rigidbody.velocity = Vector2.zero;
        m_rigidbody.bodyType = RigidbodyType2D.Static;
    }
}
