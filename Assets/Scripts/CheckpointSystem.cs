using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSystem : MonoBehaviour
{   
    private static string scene = string.Empty;    
    public static Vector2 spawnPosition { get; private set; }

    public static Vector2 GetSpawnPosition ()
    {
        string currentScene = SceneManager.GetActiveScene().path;

        if(scene != currentScene)
        {
            scene = currentScene;
            return Vector2.zero;
        }
        else 
        {
            return spawnPosition;
        }
    }

    public static void SetSpawnPosition (Vector2 position)
    {
        scene = SceneManager.GetActiveScene().path;
        spawnPosition = position;
    }
}
