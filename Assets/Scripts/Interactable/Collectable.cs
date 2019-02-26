using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    private Collider2D m_collider;
    private SpriteRenderer m_renderer;
    private ParticleSystem collectFX;
    private AudioSource collectSFX;

    private void OnEnable()
    {
        m_collider = GetComponent<Collider2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        collectFX = GetComponent<ParticleSystem>();
        collectSFX = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CollectableDisplay.instance.AddScore(1);
            DisableComponents();
        }
    }

    private void DisableComponents()
    {
        if(collectFX) collectFX.Play();
        if (collectSFX) collectSFX.Play();
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }
}