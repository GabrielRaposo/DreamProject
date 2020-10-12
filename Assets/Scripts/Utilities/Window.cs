using System.Collections;
using UnityEngine;

public class Window : MonoBehaviour
{
    [SerializeField] private bool final;    

    [Header("References")]
    [SerializeField] private AudioSource openSFX;
    [SerializeField] private AudioSource closeSFX;

    private Animator animator;    
    private ParticleSystem ps;

    void OnEnable()
    {
        animator = GetComponent<Animator>();
        ps = GetComponent<ParticleSystem>();

        ps.Play();
    }

    public void Open()
    {
        if(animator) animator.SetBool("Open", true);
        openSFX.Play();
        ps.Stop();
    }

    public void Close()
    {
        if(animator) animator.SetBool("Open", false);
        closeSFX.Play();
        ps.Play();
    }

    public bool isFinal(){ return final; } 
}
