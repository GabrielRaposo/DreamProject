using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour, IBreakable
{
    [SerializeField] private float health;    
    [SerializeField] private GameObject hiddenTreasure;

    private SpriteRenderer m_renderer;
    private Animator m_animator;
    private ParticleSystem shakeFX;
    private AudioSource shakeSFX;
    private Vector3 originalPosition;

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
        if (hiddenTreasure != null) hiddenTreasure.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        StartCoroutine(Shake());
        shakeFX.Play();
        shakeSFX.Play();

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
        m_renderer.sortingOrder++;
        StartCoroutine(WaitToDestroy());
    }

    private IEnumerator WaitToDestroy()
    {
        m_animator.SetTrigger("Break");
        if(hiddenTreasure != null)  
        { 
            hiddenTreasure.transform.parent = null;
            hiddenTreasure.SetActive(true);
        }
        yield return new WaitForSeconds(.5f);
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
