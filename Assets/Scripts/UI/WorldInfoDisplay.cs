using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldInfoDisplay : MonoBehaviour
{
    [SerializeField] private int worldID;
    [SerializeField] private TextMeshProUGUI moonCountDisplay;
    [SerializeField] private TextMeshProUGUI collectableCountDisplay;

    public void Setup() 
    {
        WorldData worldData = GameplayData.GetWorldData(worldID);

        int bonusCount = BonusCollectableList.GetWorldCollectedCount(worldID);
        moonCountDisplay.text = bonusCount.ToString() + "/" + worldData.moonMax.ToString();

        collectableCountDisplay.text = "x" + worldData.collectCount;
    }

    public void SetupWorldData()
    {
        CheckpointSystem.SetSpawnPosition(Vector2.zero);
        CollectableDisplay.savedScore = 0;
        GameplayData.currentWorld = worldID;
        PlayerHealth.ResetSavedHealth();
    }
}
