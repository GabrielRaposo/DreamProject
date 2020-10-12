using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSwitchCrystal : EventSwitch
{
    [SerializeField] private AudioSource openSFX;
    [SerializeField] private AudioSource closeSFX;

    private bool open;

    private Animator m_animator;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        StartCoroutine(ShineAnimation());
    }

    private IEnumerator ShineAnimation()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3.0f));
            m_animator.SetTrigger("Shine");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Twirl") || collision.CompareTag("Hitbox"))
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            if(bullet)
            {
                bullet.Vanish();
            }

            if(eventsOnTrigger != null && eventsOnTrigger.Length > 0)
            {
                foreach(CallEventOnTrigger e in eventsOnTrigger)
                {
                    e.CallEvent(this);
                }
            }
            m_animator.SetTrigger("Open");
            open = true;
            openSFX.Play();
        }
    }

    public override void Lock()
    {
        lockCount++;
    }

    public override void Unlock()
    {
        lockCount--;
        if(lockCount < 1 && open)
        {
            m_animator.SetTrigger("Close");
            closeSFX.Play();
        }
    }
}
