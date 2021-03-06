﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BouncyProperty : MonoBehaviour
{
    [SerializeField] private UnityEvent OnBounceEvent;
    [SerializeField] private UnityEvent OnHammerEvent;

    [HideInInspector] public bool super; 

    private AudioSource bounceSFX;

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

        bounceSFX = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            PlayerPlatformer player = collision.transform.GetComponent<PlayerPlatformer>();
            if (player)
            {
                bounceSFX.Play();
                player.SetBounceJump(1.6f);
                if (!super)
                {
                    OnBounceEvent.Invoke();
                }
            }
        } 
        else if (collision.transform.CompareTag("Enemy"))
        {
            PlatformerCreature enemy = collision.transform.GetComponent<PlatformerCreature>();
            if (enemy)
            {
                bounceSFX.Play();
                if(collision.transform.position.y > transform.position.y + .1f)
                {
                    OnBounceEvent.Invoke();
                    enemy.OnBouncyTopEvent(transform.position, super);
                }
                else
                {
                    OnBounceEvent.Invoke();
                    enemy.OnBouncySideEvent(transform.position);
                } 
            }
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Twirl"))
    //    {
    //        OnHammerEvent.Invoke();
    //    }
    //}
}
