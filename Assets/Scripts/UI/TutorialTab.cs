using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialTab : MonoBehaviour
{
    [SerializeField] private RectTransform anchorTab; 
    [SerializeField] private TextMeshProUGUI label; 
    [SerializeField] private float speed;

    private const int hiddenY = 200;

    private int targetY;

    void Start()
    {
        anchorTab.anchoredPosition = Vector2.up * hiddenY;
        targetY = hiddenY;
    }

    public void Show (string text)
    {
        label.text = text;
        targetY = 0;
        enabled = true;
    }

    public void Hide()
    {
        targetY = hiddenY;
    }

    private void Update() 
    {
        float diff = targetY - anchorTab.anchoredPosition.y;
        if (Mathf.Abs(diff) > speed * 2)
        {
            anchorTab.anchoredPosition += Vector2.up * (diff > 0 ? 1 : -1) * speed;
        }
        else 
        {
            anchorTab.anchoredPosition = Vector2.up * targetY;
            if(targetY == hiddenY) enabled = false;
        }
    }
}
