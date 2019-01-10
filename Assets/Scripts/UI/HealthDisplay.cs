using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI display;

    public int value { get; private set; }
    private int maxValue;

    public void Init(int value)
    {
        this.value = maxValue = value;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (display)
        {
            display.text = value.ToString();
        }
    }

    public void ChangeValue(int diff)
    {
        value += diff;
        if (value < 0)
        {
            value = 0;
        }
        else if (value > maxValue)
        {
            value = maxValue;
        }
        UpdateDisplay();
    }
}
