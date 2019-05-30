using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayData
{
    public static int world1MoonMax = 4;
    public static int world1MoonCount;
    public static int world1CollectCount;
    public static int world1DeathCount;

    public static int world2MoonMax = 3;
    public static int world2MoonCount;
    public static int world2CollectCount;
    public static int world2DeathCount;

    public static int currentWorld = 1;

    public static void Reset()
    {
        world1MoonCount = world1CollectCount =  world1DeathCount = 0;
        world2MoonCount = world2CollectCount =  world2DeathCount = 0;

        CollectableDisplay.savedScore = 0;
    }
}
