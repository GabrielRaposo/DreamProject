using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BouncyProperty : MonoBehaviour
{
    [SerializeField] private UnityEvent OnBounceEvent;
    [SerializeField] private UnityEvent OnHammerEvent;

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
                player.SetJump(super);
                if (!super)
                {
                    OnBounceEvent.Invoke();
                }
            }
        } 
        else if (collision.transform.CompareTag("Enemy"))
        {
            Enemy enemy = collision.transform.GetComponent<Enemy>();
            if (enemy)
            {
                if(collision.transform.position.y > transform.position.y + .1f)
                {
                    OnBounceEvent.Invoke();
                    enemy.OnBouncyTopEvent(collision.contacts[0].point, super);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            OnHammerEvent.Invoke();
        }
    }
}
