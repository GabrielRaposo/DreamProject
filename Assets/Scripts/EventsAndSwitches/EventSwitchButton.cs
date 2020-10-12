using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSwitchButton : EventSwitch
{
    [SerializeField] private bool singleUse;

    private Animator m_animator;
    private ParticleSystem clickFX;
    private AudioSource clickSFX;

    private bool pressed;

    private void Awake() 
    {
        m_animator = GetComponent<Animator>();
        clickFX = GetComponent<ParticleSystem>();
        clickSFX = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            if(lockCount < 1 && !pressed)
            {
                if(eventsOnTrigger != null && eventsOnTrigger.Length > 0)
                {
                    foreach(CallEventOnTrigger e in eventsOnTrigger)
                    {
                        e.CallEvent(this);
                    }
                    m_animator.SetTrigger("Press");
                }
                clickFX.Play();
                clickSFX.Play();
            }
            pressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(!singleUse && collision.CompareTag("Player"))
        {
            if(eventsOnTrigger != null && eventsOnTrigger.Length > 0)
            {
                pressed = false;
                if(lockCount < 1)
                {
                    m_animator.SetTrigger("Reset");
                }
            }
        }
    }

    public override void Lock()
    {
        lockCount++;
    }

    public override void Unlock()
    {
        lockCount--;
        if(lockCount < 1 && !pressed && !singleUse)
        {
            m_animator.SetTrigger("Reset");
        }
    }
}
