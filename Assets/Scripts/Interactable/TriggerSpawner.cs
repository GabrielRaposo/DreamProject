using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnList;
    [Space(10)]
    [SerializeField] private ParticleSystem spawnEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<Collider2D>().enabled = false;
            if(spawnList.Length > 0) StartCoroutine(SpawnCicle());
        }
    }

    private IEnumerator SpawnCicle()
    {
        foreach(GameObject spawn in spawnList)
        {
            spawn.SetActive(true);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }
    }

}
