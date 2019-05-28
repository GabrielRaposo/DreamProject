using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemLock : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private string title;
    [SerializeField] private int price;
    [SerializeField] private Collider2D itemCollider;

    [Header("References")]
    [SerializeField] private TextMesh titleRenderer;
    [SerializeField] private TextMesh priceRenderer;
    
    [Header("Effects")]
    [SerializeField] private AudioSource openSFX;
    [SerializeField] private AudioSource rejectSFX;

    private bool unlocked;
    private Animator m_animator;
    private CollectableDisplay collectableDisplay;

    void Start()
    {
        //m_collider = GetComponent<Collider2D>();
        m_animator = GetComponent<Animator>();

        if (itemCollider) itemCollider.enabled = false;
        titleRenderer.text = title;
        priceRenderer.text = price.ToString();

        collectableDisplay = CollectableDisplay.instance;
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if (unlocked) return;

        if(collision.gameObject.CompareTag("Player") && collectableDisplay)
        {
            if (collectableDisplay.BuyAt(price))
            {
                unlocked = true;
                m_animator.SetTrigger("Open");
                openSFX.Play();
            }
            else 
            {
                m_animator.SetTrigger("Reject");
                rejectSFX.Play();
            }
        }
    }

    public void DisableAll()
    {
        if (itemCollider) itemCollider.enabled = true;
        gameObject.SetActive(false);
    }
}
