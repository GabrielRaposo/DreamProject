using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Vector3 spawnOffset;

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + spawnOffset, .2f);
    }

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer) spriteRenderer.enabled = false;

        Collider2D collider2D = GetComponent<Collider2D>();
        if(collider2D)
        {
            collider2D.enabled = !(CheckpointSystem.spawnPosition == (Vector2)(transform.position + spawnOffset));           
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            Debug.Log("checkpoint caught at " + collision.transform.position);
            
            Collider2D collider2D = GetComponent<Collider2D>();
            if(collider2D) collider2D.enabled = false;

            CheckpointSystem.SetSpawnPosition(transform.position + spawnOffset);

            CollectableDisplay.instance.SaveScore();
            BonusCollectableManager.SaveCollectedStates();
        }
    }
}
