using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSelectManager : MonoBehaviour
{
    [SerializeField] private WorldInfoDisplay world1InfoDisplay;
    [SerializeField] private WorldInfoDisplay world2InfoDisplay;

    void Start()
    {
        GameplayData.Setup();

        world1InfoDisplay.Setup();
        world2InfoDisplay.Setup();
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            string sceneName = "DevScene";
            ScreenTransition screenTransition = ScreenTransition.instance;
            if(screenTransition) screenTransition.Call(sceneName);
            else                 ScreenTransition.LoadScene(sceneName);
        }
    }
}
