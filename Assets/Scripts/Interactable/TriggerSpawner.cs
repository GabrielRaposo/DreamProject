using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject challengeBarrier;
    [SerializeField] private GameObject[] spawnList;
    [Space(10)]
    [SerializeField] private GameObject spawnPrefab;

    private List<GameObject> spawnFXs;

    private void Start() 
    {
        spawnFXs = new List<GameObject>();

        foreach(GameObject spawn in spawnList)
        {
            GameObject spawnFX = Instantiate(spawnPrefab, spawn.transform.position, Quaternion.identity); 
            spawnFX.SetActive(false);
            
            EnemySpawnEffect enemySpawnEffect = spawnFX.GetComponent<EnemySpawnEffect>();
            enemySpawnEffect.Set(spawn);

            spawnFXs.Add(spawnFX);
        }
    }

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
        if(challengeBarrier != null)
        {
            challengeBarrier.SetActive(true);
        }

        foreach(GameObject spawnFX in spawnFXs)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            spawnFX.SetActive(true);
        }
    }

}
