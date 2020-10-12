using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemFlower : MonoBehaviour
{
    [SerializeField] private GameObject gem;

    private Animator m_animator;
    private Collider2D m_collider;
    private ParticleSystem openFX;
    private AudioSource openSFX;

    private bool open;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
        openFX = GetComponent<ParticleSystem>();
        openSFX = GetComponent<AudioSource>();

        if(gem) gem.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            if(!open)
            {
                open = true;
                Open();
            }
        }
    }

    private void Open()
    {
        m_animator.SetTrigger("Open");
        m_collider.enabled = false;
        openFX.Play();
        openSFX.Play();

        if (gem)
        {
            StartCoroutine(SpawnAnimation());
        }
    }

    private IEnumerator SpawnAnimation()
    {
        Collider2D gemCollider = gem.GetComponent<Collider2D>();
        gemCollider.enabled = false;

        gem.transform.position = transform.position;
        gem.SetActive(true);

        while (gem.transform.localPosition.y < 2)
        {
            yield return new WaitForEndOfFrame();
            gem.transform.position += Vector3.up * .5f;
        }

        gemCollider.enabled = true;
    }
}
