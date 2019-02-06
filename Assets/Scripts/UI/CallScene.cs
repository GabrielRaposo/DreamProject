using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallScene : MonoBehaviour
{
    [SerializeField] string scenePath; 
    
    public void Call()
    {
        PlaytimeData.Reset();

        ScreenTransition screenTransition = ScreenTransition.instance;
        if (screenTransition) screenTransition.Call(scenePath);
        else ScreenTransition.LoadScene(scenePath);
    }
}
