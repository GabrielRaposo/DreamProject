using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Collectable : MonoBehaviour
{
    private Collider2D m_collider;
    private SpriteRenderer m_renderer;
    private ParticleSystem collectFX;
    private FollowTransform followTransform;

    private bool collected;

    private void OnEnable()
    {
        m_collider = GetComponent<Collider2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        collectFX = GetComponent<ParticleSystem>();
        followTransform = GetComponent<FollowTransform>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collected) return;

        if (collision.CompareTag("Player"))
        {
            collected = true;
            CollectableDisplay.instance.AddScore(1);
            followTransform.enabled = true;
            followTransform.Follow(collision.transform);
        }
    }

    //acessado pelo script de eventos
    public void DisableComponents()
    {
        if (collectFX) collectFX.Play();

        if (m_collider) m_collider.enabled = false;
        if (m_renderer) m_renderer.enabled = false;
        if (followTransform) followTransform.enabled = false;

        Destroy(gameObject, 2f);
    }
}