using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private BouncyProperty bouncyCap;

    private Animator animator;
    private ParticleSystem sporesFX;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sporesFX = GetComponent<ParticleSystem>();
    }

    public void StartTremble()
    {
        animator.SetTrigger("Tremble");
        sporesFX.Play();
    }

    public void StartCharge()
    {
        animator.SetBool("Charge", true);
        StopAllCoroutines();
        StartCoroutine(ChargeTimer());
    }

    private IEnumerator ChargeTimer()
    {
        for (int i = 0; i < 60; i++){
            yield return new WaitForFixedUpdate();
        }
        bouncyCap.super = true;
        animator.SetBool("Release", true);
    }

    public void ResetStates()
    {
        bouncyCap.super = false;
        animator.SetBool("Charge", false);
        animator.SetBool("Release", false);
    }
}
