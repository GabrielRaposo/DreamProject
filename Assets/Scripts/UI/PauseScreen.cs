﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] private RawImage fadedImage;
    [SerializeField] private Button firstSelection;

    public bool active { get; private set; }

    public static PauseScreen instance;

    private void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        else 
        {
            //já é destruído no ScreenTransition.cs
        }

        fadedImage.gameObject.SetActive(false);
    }

    public bool ToggleState ()
    {
        if (active = !active) 
            Activate();
        else                  
            Deactivate();
        
        return active;
    }

    public void Activate()
    {
        fadedImage.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        firstSelection.Select();
        Time.timeScale = 0;
        active = true;
    }

    public void Deactivate()
    {
        StartCoroutine(WaitForAFrame());
    }

    private IEnumerator WaitForAFrame()
    {
        yield return new WaitForEndOfFrame();

        fadedImage.gameObject.SetActive(false);
        Time.timeScale = 1;
        active = false;
    }
}
