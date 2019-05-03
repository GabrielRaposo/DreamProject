using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour, IBreakable
{
    [SerializeField] private int health;    

    private ParticleSystem shakeFX;
    private Vector3 originalPosition;

    private void Awake() 
    {
        shakeFX = GetComponent<ParticleSystem>();
    }

    private void Start() 
    {
        originalPosition = transform.position;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(Shake());
        shakeFX.Play();

        if(health < 1)
        {
            Break();
        }
    }

    private IEnumerator Shake()
    {
        transform.position += RaposUtil.RotateVector(Vector3.up, Random.Range(0, 360)) * .05f;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        transform.position = originalPosition;
    }

    private void Break()
    {
        //if (transform.childCount > 0)
        //{
        //    for(int i = 0; i < transform.childCount; i++)
        //    {
        //        if (transform.GetChild(i).CompareTag("Player"))
        //        {
        //            transform.GetChild(i).parent = null;
        //            break;
        //        }
        //    }
        //}

        Destroy(gameObject);
    }
}
