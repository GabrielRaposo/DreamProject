﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectableDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI display;

    private static int savedScore = 0;

    private int score;
    public int Score
    {
        get
        {
            return score;
        }

        private set
        {
            score = value;
            UpdateDisplay();
        }
    }

    public static CollectableDisplay instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        Score = savedScore;
    }

    private void UpdateDisplay()
    {
        if (display)
        {
            display.text = Score.ToString();
        }
    }

    public void AddScore(int quant)
    {
        Score += quant;
    }

    public void SaveScore()
    {
        savedScore = score;
    }
}
