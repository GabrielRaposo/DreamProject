using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour 
{
    [SerializeField] private PlayerHealthDisplay UIDisplay;

    private int maxHealth;
    public float value { get; private set; }
    [HideInInspector] static public int deathCount;

    public void Init(int maxHealth)
    {
        value = this.maxHealth = maxHealth;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (UIDisplay)
        {
            UIDisplay.ChangeFill(value / maxHealth);
        }
    } 

    public void ChangeHealth(float v)
    {
        value += v;
        if (value > maxHealth) value = maxHealth;
        if (value < 1) value = 0;
        
        UpdateDisplay();
    }
}

