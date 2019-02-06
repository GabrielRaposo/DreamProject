using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultsDisplay : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI display = GetComponent<TextMeshProUGUI>();
        if (display)
        {
            display.text =
                "Tempo: " + PlaytimeData.GetPlayTime() + "\n" +
                "Estrelas: " + PlaytimeData.starsCount + "\n" +
                "Mortes: " + PlaytimeData.numberOfDeaths;
        }
    }

}
