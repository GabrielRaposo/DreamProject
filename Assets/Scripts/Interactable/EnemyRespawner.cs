using System.Collections;
using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [SerializeField] private float respawnTime = 1;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject spawnEffect;

    private AudioSource spawnSFX;

    void Start()
    {
        spawnSFX = GetComponent<AudioSource>();
        enemyPrefab.SetActive(false);

        StartCoroutine(TimerToSpawn(1));
    }

    public void Spawn()
    {
        if(gameObject && gameObject.activeSelf) StartCoroutine(TimerToSpawn());
    }

    private IEnumerator TimerToSpawn(int extraTime = 0)
    {
        yield return new WaitForSeconds(respawnTime + extraTime);

        if (spawnSFX) spawnSFX.Play();

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);

        IRespawnable respawnable = enemy.GetComponent<IRespawnable>();
        if(respawnable != null) respawnable.SetRespawner(this);

        GameObject spawnFX = Instantiate(spawnEffect, transform.position, Quaternion.identity); 
            
        EnemySpawnEffect enemySpawnEffect = spawnFX.GetComponent<EnemySpawnEffect>();
        enemySpawnEffect.Set(enemy);
        spawnFX.SetActive(true);
    }
}
