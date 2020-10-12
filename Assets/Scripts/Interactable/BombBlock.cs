using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBlock : MonoBehaviour, IBreakable
{
    [SerializeField] private float health;    
    [SerializeField] private GameObject explosionHitbox;

    private SpriteRenderer m_renderer;
    private Animator m_animator;
    private ParticleSystem shakeFX;
    private AudioSource shakeSFX;

    private Vector3 originalPosition;
    private Coroutine blinkCoroutine;

    private void Awake() 
    {
        m_renderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        shakeFX = GetComponent<ParticleSystem>();
        shakeSFX = GetComponent<AudioSource>();
    }

    private void Start() 
    {
        originalPosition = transform.position;
        blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    private IEnumerator BlinkLoop()
    {
        while(true)
        {
            float time = Random.Range(1.5f, 3.0f);
            yield return new WaitForSeconds(time);
            m_animator.SetTrigger("Blink");
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if(blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        StartCoroutine(Shake());
        shakeFX.Play();
        shakeSFX.Play();
        m_animator.SetTrigger("TakeDamage");

        if(health < 1)
        {
            Break();
        }
    }

    private IEnumerator Shake()
    {
        transform.position += RaposUtil.RotateVector(Vector3.up, Random.Range(0, 360)) * .05f;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        transform.position = originalPosition;
    }

    private void Break()
    {
        explosionHitbox.transform.parent = null;
        explosionHitbox.SetActive(true);

        Destroy(gameObject);
    }
    
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Explosion"))
        {
            TakeDamage(100);
        }
    }
}
