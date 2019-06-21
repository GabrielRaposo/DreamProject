using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusCollectableManager : MonoBehaviour
{
    private static CollectableBonus[] collectableBonuses;
    private static bool[] collectedStates;

    void Start()
    {
        if (transform.childCount > 0)
        {
            InitCollectables(transform.childCount);
        }
    }

    private void InitCollectables(int childCount)
    {
        collectableBonuses = new CollectableBonus[childCount];
        collectedStates = new bool[childCount];

        for(int i = 0; i < childCount; i++)
        {
            collectableBonuses[i] = transform.GetChild(i).GetComponent<CollectableBonus>();
            collectableBonuses[i].Init(this);
        }

        BonusCollectableList.WorldList worldList = BonusCollectableList.GetWorldList(GameplayData.currentWorld);
        BonusCollectableList.StageList stageList = BonusCollectableList.GetCurrentStageList(worldList);
        
        int count;
        if((count = stageList.collectableStates.Count) < 1)
        {
            //cria a lista em que todos os coletáveis locais não estão coletados
            for(int i = 0; i < childCount; i++) 
                stageList.collectableStates.Add(false);
        }
        else
        {
            //fade out nos coletáveis já coletados
            for(int i = 0; i < count; i++)
            {
                if(stageList.collectableStates[i]) collectableBonuses[i].FadeOutSprite(); 
            }
        }
    }

    public void SetCollect(CollectableBonus collectableBonus)
    {
        int id = -1;
        
        for (int i = 0; i < collectableBonuses.Length; i++)
        {
            if(collectableBonuses[i] == collectableBonus) 
                id = i;
        }

        if(id > -1)
        {
            collectedStates[id] = true;

            BonusCollectableList.WorldList worldList = BonusCollectableList.GetWorldList(GameplayData.currentWorld);
            BonusCollectableList.StageList stageList = BonusCollectableList.GetCurrentStageList(worldList);

            if(!stageList.collectableStates[id])
            {
                BonusCollectableDisplay.instance.AddValue(1);          
            }
            else 
            {
                BonusCollectableDisplay.instance.AddValue(0); 
            }
        }
    }

    public static void SaveCollectedStates()
    {
        BonusCollectableList.WorldList worldList = BonusCollectableList.GetWorldList(GameplayData.currentWorld);
        BonusCollectableList.StageList stageList = BonusCollectableList.GetCurrentStageList(worldList);

        for(int i = 0; i < collectedStates.Length; i++)
        {
            if(collectedStates[i])
            {
                stageList.collectableStates[i] = true;
            }
        }
    }
}
