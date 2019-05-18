using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraPriorityManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera[] virtualCameras;

    public static CameraPriorityManager instance;

    public enum GameState { PlatformGround, PlatformAirborne, Shooter, Special }
    public GameState gameState { get; private set; }
    private GameState previousGameState;

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
        if(this.gameState == gameState) return;

        int j = (int)gameState;
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if(i != j)
            {
                virtualCameras[i].Priority = 0;
            }
        }
        virtualCameras[j].transform.position = virtualCameras[(int)this.gameState].transform.position;
        virtualCameras[j].Priority = 1;

        previousGameState = this.gameState;
        this.gameState = gameState;
    }

    public void SetSpecialFocus (Transform t)
    {
        int j = (int)GameState.Special;
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if(i != j)
            {
                virtualCameras[i].Priority = 0;
            }
        }
        virtualCameras[j].transform.position = virtualCameras[(int)this.gameState].transform.position;
        virtualCameras[j].Follow = t;
        virtualCameras[j].Priority = 1;

        previousGameState = gameState;
        gameState = GameState.Special;
    }

    public void ReturnToPreviousFocus()
    {
        SetFocus(previousGameState);
    }
}
