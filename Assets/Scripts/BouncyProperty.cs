using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BouncyProperty : MonoBehaviour
{
    public UnityEvent OnBounceEvent;
    public UnityEvent OnHammerEvent;

    [HideInInspector] public bool super; 

	void Start ()
    {
        if (OnBounceEvent == null)
        {
            OnBounceEvent = new UnityEvent();
        }

        if (OnHammerEvent == null)
        {
            OnHammerEvent = new UnityEvent();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            PlayerMovement player = collision.transform.GetComponent<PlayerMovement>();
            if (player)
            {
                if (super)
                {
                    player.SetJump(false, true, true);
                }
                else
                {
                    player.SetJump(false, true, false);
                    OnBounceEvent.Invoke();
                }
            }
        } 
        else if (collision.transform.CompareTag("Enemy"))
        {
            Enemy enemy = collision.transform.GetComponent<Enemy>();
            if (enemy)
            {
                if(collision.transform.position.y > transform.position.y - .2f)
                {
                    OnBounceEvent.Invoke();
                    enemy.OnBouncyTopEvent(collision.contacts[0].point, super);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox"))
        {
            OnHammerEvent.Invoke();
        }
    }
}
