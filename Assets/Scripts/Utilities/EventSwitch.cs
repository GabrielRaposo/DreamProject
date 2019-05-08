using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSwitch : MonoBehaviour
{
    [SerializeField] private SwitchPositionEvent[] eventsOnSwitch;
    [SerializeField] private bool singleUse;

    private Animator m_animator;
    private ParticleSystem clickFX;

    private bool pressed;
    private int pressLock;

    private void Awake() 
    {
        m_animator = GetComponent<Animator>();
        clickFX = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            if(pressLock < 1 && !pressed)
            {
                if(eventsOnSwitch != null && eventsOnSwitch.Length > 0)
                {
                    foreach(ICallEventOnSwitch e in eventsOnSwitch)
                    {
                        e.CallEvent(this);
                    }
                    m_animator.SetTrigger("Press");
                }
                clickFX.Play();
            }
            pressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(!singleUse && collision.CompareTag("Player"))
        {
            if(eventsOnSwitch != null && eventsOnSwitch.Length > 0)
            {
                pressed = false;
                if(pressLock < 1)
                {
                    m_animator.SetTrigger("Reset");
                }
            }
        }
    }

    public void Lock()
    {
        pressLock++;
    }

    public void Unlock()
    {
        pressLock--;
        if(pressLock < 1 && !pressed && !singleUse)
        {
            m_animator.SetTrigger("Reset");
        }
    }
}
