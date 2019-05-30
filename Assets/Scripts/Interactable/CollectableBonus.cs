﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CollectableBonus : MonoBehaviour
{
    [SerializeField] private ParticleSystem backPS;

    private Rigidbody2D m_rigidbody2D;
    private SpriteRenderer m_renderer;
    private Animator m_animator;
    private AudioSource collectSFX;

    private bool collected;

    private void OnEnable()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();

        collectSFX = GetComponent<AudioSource>();

        backPS.transform.parent = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collected) return;

        if (collision.CompareTag("Player"))
        {
            collected = true;
            BonusCollectableDisplay.instance.AddScore(1);
            backPS.Stop();
            if (m_rigidbody2D) 
            {
                m_rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                m_rigidbody2D.velocity = Vector2.up * 6;
            }
            if (m_animator) m_animator.SetTrigger("Collect");
        }
    }

    //acessado pelo script de eventos
    public void DisableComponents()
    {
        if (collectSFX) collectSFX.Play();
        if (m_renderer) m_renderer.enabled = false;

        Destroy(gameObject, 2f);
    }
}