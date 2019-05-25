﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        PlayerPhaseManager.gameManager = this;
    }

    public void KillPlayer()
    {
        PlayerPhaseManager.instance.TakeDamage(99);
        PlayerPhaseManager.instance.Die();
    }

    public void RestartScene()
    {
        PlaytimeData.numberOfDeaths++;

        ScreenTransition screenTransition = ScreenTransition.instance;
        string sceneName = SceneManager.GetActiveScene().path;
        if (screenTransition) screenTransition.Call(sceneName);
        else                  ScreenTransition.LoadScene(sceneName);
    }

    public void CallNextStage()
    {
        PlaytimeData.finishedStages++;
        CollectableDisplay.instance.SaveScore();

        ScreenTransition screenTransition = ScreenTransition.instance;
        string scenePath = SceneManager.GetActiveScene().path;
        int currentIndex = int.Parse(scenePath.Substring(scenePath.Length - 7, 1));
        if(currentIndex + 1 < 5)
        {
            scenePath = scenePath.Substring(0, scenePath.Length - 7) + (currentIndex + 1) + ".unity";
        }
        else
        {
            scenePath = "Assets/Scenes/OutroScene.unity";
        }
        if (screenTransition) screenTransition.Call(scenePath);
        else                  ScreenTransition.LoadScene(scenePath);
    }
}
