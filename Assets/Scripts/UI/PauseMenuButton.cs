using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private AudioSource hoverSFX;

    [Header("Values")]
    [SerializeField] private Sprite outlineSprite;
    [SerializeField] private Sprite filledSprite;

    private void OnEnable() 
    {
        buttonImage.sprite = outlineSprite;
        label.color = Color.white;
    }

    public void OnSelect(BaseEventData eventData) 
    {
        buttonImage.sprite = filledSprite;
        label.color = Color.black;
        hoverSFX.Play();
    }

    public void OnDeselect(BaseEventData eventData) 
    {
        buttonImage.sprite = outlineSprite;
        label.color = Color.white;
    }
}
