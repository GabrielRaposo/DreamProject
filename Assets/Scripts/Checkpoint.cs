using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public SpriteRenderer flag;
    private CircleCollider2D coll;

    private void OnEnable()
    {
        coll = GetComponent<CircleCollider2D>();
    }

    public void Activate()
    {
        flag.color = Color.red;
        coll.enabled = false;
    }

    public void Deactivate()
    {
        flag.color = Color.white;
        coll.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CheckpointSystem.SetCheckpoint(this);
            Activate();
        }
    }
}
