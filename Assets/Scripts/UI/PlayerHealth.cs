using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour 
{
    [SerializeField] private PlayerHealthDisplay UIDisplay;

    private static int maxHealth = 3;
    public static float value { get; private set; }
    public static float savedHealth;
    
    public static int deathCount;

    public void Init(int mHealth)
    {
        value = savedHealth;
        maxHealth = mHealth;

        if (UIDisplay)
        {
            UIDisplay.ChangeFill(value / maxHealth, true);
        }
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

    public static void SaveHealth()
    {
        savedHealth = value;
    }

    public static void ResetSavedHealth()
    {
        savedHealth = maxHealth;
    }
}

