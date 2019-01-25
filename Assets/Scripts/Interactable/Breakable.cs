using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            Break();
        }
    }

    private void Break()
    {
        if (transform.childCount > 0)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("Player"))
                {
                    transform.GetChild(i).parent = null;
                    break;
                }
            }
        }

        Destroy(gameObject);
    }
}
