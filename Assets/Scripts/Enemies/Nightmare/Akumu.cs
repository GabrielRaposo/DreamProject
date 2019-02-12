using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Akumu : MonoBehaviour, IObserver
{
    protected Animator m_animator;
    protected SpriteRenderer m_renderer;
    protected Rigidbody2D m_rigidbody;

    protected bool facingRight;
    protected ID id;
    protected Nightmatrix currentNightmatrix;

    private Shader spriteShader;
    private Shader blinkShader;

    protected IPhaseManager controller;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_rigidbody = GetComponent<Rigidbody2D>();

        spriteShader = Shader.Find("Sprites/Default");
        blinkShader = Shader.Find("GUI / Text Shader");

        id = ID.Enemy;
    }

    public void Init(IPhaseManager controller)
    {
        this.controller = controller;
    }

    public virtual void SwitchIn(Nightmatrix nightmatrix)
    {
        m_renderer.material.shader = spriteShader;

        currentNightmatrix = nightmatrix;
        currentNightmatrix.AddObserver(this);
    }

    public virtual void SwitchOut()
    {
        if (currentNightmatrix != null)
        {
            currentNightmatrix.RemoveObserver(this);
            currentNightmatrix = null;
        }
    }

    private void OnDestroy()
    {
        if (currentNightmatrix != null)
        {
            currentNightmatrix.RemoveObserver(this);
        }
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
        if (collision.CompareTag("Nightmatrix"))
        {
            controller.SetDreamPhase(collision.gameObject);
        }
    }

    public virtual void OnNotify() {}

    protected virtual void OnHitboxEvent(Hitbox hitbox) { }
    public virtual void OnTouchEvent(PlayerNightmarePhase player) { }

    protected virtual IEnumerator AttackCicle() { yield return null; }

    protected IEnumerator BlinkAnimation()
    {
        m_renderer.material.shader = blinkShader;
        for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate();
        m_renderer.material.shader = spriteShader;
    }

}
