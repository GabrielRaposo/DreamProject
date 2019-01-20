using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerGroundMovement player;

    void Start()
    {
        PlayerGroundMovement.gameManager = this;
    }

    public void RespawnPlayer()
    {
        Checkpoint checkpoint = CheckpointSystem.currentCheckpoint;
        if (checkpoint)
        {
            player.transform.position = checkpoint.transform.position;
        }
        else
        {
            player.transform.position = player.startingPosition;
        }
    }
}
