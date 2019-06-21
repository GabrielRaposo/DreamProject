using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BonusCollectableList
{
    public struct WorldList
    {
        public WorldList(int id)
        {
            this.id = id;
            stageLists = new List<StageList>();
        }

        public int id;
        public List<StageList> stageLists;
    }

    public struct StageList
    {
        public StageList(string path)
        {
            this.path = path;
            collectableStates = new List<bool>(); 
        }

        public string path;    
        public List<bool> collectableStates;
    }

    private static List<WorldList> worldLists = new List<WorldList>();

    public static WorldList GetWorldList(int id)
    {
        foreach(WorldList list in worldLists)
        {   
            if (list.id == id) return list;
        }
            
        Debug.Log("World " + id + " not found. Creating a new one.");
        WorldList worldList = new WorldList(id);
        worldLists.Add(worldList);

        return worldList;
    }

    public static StageList GetCurrentStageList(WorldList worldList)
    {
        string path = SceneManager.GetActiveScene().path;
        foreach(StageList stageList in worldList.stageLists)
        {
            if(stageList.path == path) return stageList;
        }

        Debug.Log("Current stage not found. Creating a new one.");
        StageList newList = new StageList(path);
        worldList.stageLists.Add(newList);

        return newList;
    }

    //pra contablização do menu de fases
    public static List<bool> GetWorldCollectionStateAt(int id)
    {
        WorldList worldList = GetWorldList(id);
        List<bool> worldCollectionStates = new List<bool>();
        
        foreach(StageList stageList in worldList.stageLists)
        {
            foreach(bool b in stageList.collectableStates)
            {
                worldCollectionStates.Add(b);
            } 
        }
        return worldCollectionStates;
    }

    public static int GetWorldCollectedCount(int id)
    {
        List<bool> states = GetWorldCollectionStateAt(id);

        int count = 0;
        foreach(bool b in states)
        {
            if(b) count++;
        }
        return count;
    }
}
