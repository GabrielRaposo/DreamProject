using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraPriorityManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera[] virtualCameras;

    public static CameraPriorityManager instance;

    public enum GameState { PlatformGround, PlatformAirborne, Shooter }
    public GameState gameState { get; private set; }

    private void Awake() 
    {
        if(instance == null)
        {
            instance = this;
        }
        else 
        {
            Debug.Log("The instance CameraPriorityManager for already exists.");
            Destroy(gameObject);
        }
    }

    public void SetFocus (GameState gameState)
    {
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if(i == (int)gameState)
            {
                virtualCameras[i].Priority = 1;
            }
            else 
            {
                virtualCameras[i].Priority = 0;
            }
        }
        this.gameState = gameState;
    }

}
