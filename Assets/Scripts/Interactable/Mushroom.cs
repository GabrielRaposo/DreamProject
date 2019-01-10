using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    public BouncyProperty bouncyCap;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartTremble()
    {
        animator.SetTrigger("Tremble");
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
