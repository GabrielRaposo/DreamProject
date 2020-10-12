using System;

public class PlaytimeData
{
    public static DateTime startingTime;
    public static int numberOfDeaths;
    public static int starsCount;
    public static int finishedStages;

    public static void Reset()
    {
        startingTime = DateTime.Now;
        numberOfDeaths = starsCount = finishedStages = 0;
        CollectableDisplay.savedScore = 0;
    }

    public static string GetPlayTime()
    {
        /*if(startingTime == null) */ startingTime = DateTime.Now;
        TimeSpan totalTime = DateTime.Now - startingTime;
        string s = totalTime.Minutes.ToString("00") + ":" + totalTime.Seconds.ToString("00") + ":" + totalTime.Milliseconds.ToString("000");

        return s;
    }
}
