using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        PlayerPhaseManager.gameManager = this;
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            ReturnToStageSelect();
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            CallNextStage();
        }
    }

    public void KillPlayer()
    {
        PlayerPhaseManager.instance.TakeDamage(99);
        PlayerPhaseManager.instance.Die();
    }

    public void RestartScene()
    {
        PlaytimeData.numberOfDeaths++;
        PlayerHealth.ResetSavedHealth();

        ScreenTransition screenTransition = ScreenTransition.instance;
        string sceneName = SceneManager.GetActiveScene().path;
        if (screenTransition) screenTransition.Call(sceneName);
        else                  ScreenTransition.LoadScene(sceneName);
    }

    private void UpdateData()
    {
        PlayerHealth.SaveHealth();
        PlaytimeData.finishedStages++;
        CollectableDisplay.instance.SaveScore();
        BonusCollectableDisplay.instance.SaveScore();
           
        if(GameplayData.currentWorld == 1)
        {
            GameplayData.world1MoonCount = BonusCollectableDisplay.instance.Score;
            GameplayData.world1CollectCount = CollectableDisplay.instance.Score;
        }
        else 
        {
            GameplayData.world2MoonCount = BonusCollectableDisplay.instance.Score;
            GameplayData.world2CollectCount = CollectableDisplay.instance.Score;
        }
    }

    public void CallNextStage()
    {
        UpdateData();    

        ScreenTransition screenTransition = ScreenTransition.instance;
        string scenePath = SceneManager.GetActiveScene().path;
        int currentIndex = int.Parse(scenePath.Substring(scenePath.Length - 7, 1));
        scenePath = scenePath.Substring(0, scenePath.Length - 7) + (currentIndex + 1) + ".unity";

        if (screenTransition) screenTransition.Call(scenePath);
        else                  ScreenTransition.LoadScene(scenePath);
    }

    public void ReturnToStageSelect()
    {
        UpdateData();  

        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        yield return new WaitForSeconds(.5f);
        WinScreen winScreen = WinScreen.instance;
        if(winScreen) winScreen.Show();
        yield return new WaitForSeconds(2);

        ScreenTransition screenTransition = ScreenTransition.instance;
        string scenePath = scenePath = "Assets/Scenes/WorldsScene.unity";
        
        if (screenTransition) screenTransition.Call(scenePath);
        else                  ScreenTransition.LoadScene(scenePath);
    }
}
