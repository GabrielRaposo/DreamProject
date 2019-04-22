﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerCreature : MonoBehaviour
{
    [SerializeField] public float mininumTopY;

    protected bool interactable = true;

    protected Animator m_animator;
    protected SpriteRenderer m_renderer;
    protected Rigidbody2D m_rigidbody;
    protected Collider2D m_collider;
    protected ID id;

    protected IPhaseManager controller;

    public void Init(IPhaseManager controller)
    {
        this.controller = controller;
    }

    private void OnEnable()
    {
        m_animator = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();

        id = ID.Enemy;
    }

    protected IEnumerator InteractionDelay(int frames)
    {
        interactable = false;
        while(frames > 0)
        {
            yield return new WaitForFixedUpdate();
            frames--;
        }
        interactable = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox"))
        {
            Hitbox hitbox = collision.GetComponent<Hitbox>();
            if (hitbox != null && hitbox.id != id)
            {
                OnHitboxEvent(hitbox);
            }
        } else
        if (collision.CompareTag("NightmatrixBorder"))
        {
            NightmatrixBorder border = collision.GetComponent<NightmatrixBorder>();
            if (border)
            {
                controller.SetNightmarePhase(border.mainMatrix.GetComponent<Nightmatrix>());
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Nightmatrix"))
        {
            controller.SetNightmarePhase(collision.gameObject.GetComponent<Nightmatrix>());
        }
    }

    protected virtual void OnHitboxEvent(Hitbox hitbox) { if(gameObject.activeSelf) StartCoroutine(InteractionDelay(3)); }

    public virtual void OnStompEvent(PlayerDreamPhase player) { if(gameObject.activeSelf) StartCoroutine(InteractionDelay(3)); }
    public virtual void OnTouchEvent(PlayerDreamPhase player) { if(gameObject.activeSelf) StartCoroutine(InteractionDelay(3)); }

    public virtual void OnBouncyTopEvent(Vector2 contactPosition, bool super) { }
    public virtual void OnBouncySideEvent(Vector2 contactPosition) { }

    public virtual void ChildHitboxEnterEvent(Collider2D collision) { }
    public virtual void ChildHitboxExitEvent(Collider2D collision) { }
}