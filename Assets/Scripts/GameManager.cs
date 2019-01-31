using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerController player;

    void Start()
    {
        PlayerController.gameManager = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
        else
        if (Input.GetKeyDown(KeyCode.P))
        {
            CallNextStage();
        }
    }

    public void RestartScene()
    {
        ScreenTransition screenTransition = ScreenTransition.instance;
        string sceneName = SceneManager.GetActiveScene().path;
        if (screenTransition) screenTransition.Call(sceneName);
        else                  ScreenTransition.LoadScene(sceneName);
    }

    public void CallNextStage()
    {
        CollectableDisplay.instance.SaveScore();

        ScreenTransition screenTransition = ScreenTransition.instance;
        string sceneName = SceneManager.GetActiveScene().path;
        int currentIndex = int.Parse(sceneName.Substring(sceneName.Length - 7, 1));
        sceneName = sceneName.Substring(0, sceneName.Length - 7) + (currentIndex + 1) + ".unity";
        if (screenTransition) screenTransition.Call(sceneName);
        else                  ScreenTransition.LoadScene(sceneName);
    }
}
