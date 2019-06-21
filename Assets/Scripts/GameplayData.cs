using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WorldData
{
    public WorldData(int id)
    {
        this.id = id;
        moonMax = 7; // temp
        collectCount = deathCount = 0;
    }

    public int id;
    public int moonMax;
    public int collectCount;
    public int deathCount;
}

public class GameplayData
{
    public static WorldData 
        world1Data = new WorldData(1),
        world2Data = new WorldData(2);

    public static int currentWorld = 1;

    public static void Setup()
    {
        CollectableDisplay.savedScore = 0;
    }

    public static WorldData GetWorldData(int id = -1)
    {
        if(id == -1) id = currentWorld;

        switch(id)
        {
            case 1:
                Debug.Log("Got world 1 data");
                return world1Data;

            case 2:
                Debug.Log("Got world 2 data");
                return world2Data;

            default:
                Debug.Log("World id (" + id +") option not found. Returning world 1 as default. ");
                return world1Data;
        }
    }
}
