using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructAfterTimer : MonoBehaviour
{
    [SerializeField] private float time = 5;

    private void Start()
    {
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    
    public void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
