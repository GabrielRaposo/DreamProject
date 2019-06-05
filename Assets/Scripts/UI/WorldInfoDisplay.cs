using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldInfoDisplay : MonoBehaviour
{
    [SerializeField] private int worldID;
    [SerializeField] private TextMeshProUGUI moonCountDisplay;
    [SerializeField] private TextMeshProUGUI collectableCountDisplay;

    private void Start() 
    {
        if(worldID == 1)
        {
            moonCountDisplay.text 
                = GameplayData.world1MoonCount.ToString() + "/" + GameplayData.world1MoonMax.ToString();

            collectableCountDisplay.text
                = "x" + GameplayData.world1CollectCount;
        }
        else 
        {
            moonCountDisplay.text 
                = GameplayData.world2MoonCount.ToString() + "/" + GameplayData.world2MoonMax.ToString();

            collectableCountDisplay.text
                = "x" + GameplayData.world2CollectCount;
        }
    }

    public void SetupWorldData()
    {
        BonusCollectableDisplay.savedScore = CollectableDisplay.savedScore = 0;

        if(worldID == 1)
        {
            GameplayData.world1CollectCount = GameplayData.world1MoonCount = 0;
        }
        else 
        {
            GameplayData.world2CollectCount = GameplayData.world2MoonCount = 0;
        }

        GameplayData.currentWorld = worldID;
        PlayerHealth.ResetSavedHealth();
    }
}
