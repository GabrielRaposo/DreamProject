using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] private RawImage fadedImage;
    [SerializeField] private Button firstSelection;

    public bool active { get; private set; }

    private CollectableDisplay collectableDisplay;
    private BonusCollectableDisplay bonusCollectableDisplay;

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

    private void Start() 
    {
        collectableDisplay = CollectableDisplay.instance;
        bonusCollectableDisplay = BonusCollectableDisplay.instance;
    }

    public bool ToggleState ()
    {
        active = !active;

        if (active) 
            Activate();
        else                  
            Deactivate();
        
        return active;
    }

    public void Activate()
    {
        Debug.Log("activate");
        fadedImage.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        firstSelection.Select();
        Time.timeScale = 0;
        active = true;
        BGMPlayer.instance.LowerVolume();

        if(collectableDisplay) 
            collectableDisplay.ToggleVisible(true);
        if(bonusCollectableDisplay) 
            bonusCollectableDisplay.ToggleVisible(true);
    }

    public void Deactivate()
    {
        StartCoroutine(WaitBeforeDeactivate());   
    }

    public IEnumerator WaitBeforeDeactivate()
    {
        yield return new WaitForEndOfFrame();

        Debug.Log("deactivate");
        fadedImage.gameObject.SetActive(false);
        Time.timeScale = 1;
        active = false;
        BGMPlayer.instance.RiseVolume();

        if(collectableDisplay) 
            collectableDisplay.ToggleVisible(false);
        if(bonusCollectableDisplay) 
            bonusCollectableDisplay.ToggleVisible(false);
    }
}
