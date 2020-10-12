using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : MonoBehaviour
{   
    [SerializeField] private AudioSource winSFX;
    [SerializeField] private GameObject boxDisplay;
    
    public static WinScreen instance;    

    private void Awake() 
    {
        instance = this;
    
        boxDisplay.SetActive(false);
    }

    public void Show()
    {
        boxDisplay.SetActive(true);
        winSFX.Play();
    }
}
