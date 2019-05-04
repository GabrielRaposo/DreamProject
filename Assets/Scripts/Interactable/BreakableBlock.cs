using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour, IBreakable
{
    [SerializeField] private int health;    

    private SpriteRenderer m_renderer;
    private Animator m_animator;
    private ParticleSystem shakeFX;
    private Vector3 originalPosition;

    private void Awake() 
    {
        m_renderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        shakeFX = GetComponent<ParticleSystem>();
    }

    private void Start() 
    {
        originalPosition = transform.position;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(Shake());
        shakeFX.Play();

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
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }
}
