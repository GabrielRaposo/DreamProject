using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    public static Checkpoint currentCheckpoint { get; private set; }
    private static Checkpoint previousCheckpoint;

    void Awake()
    {
        if(currentCheckpoint != null)
        {
            currentCheckpoint.Activate();
        }
    }

    public static void SetCheckpoint(Checkpoint cp)
    {
        if (previousCheckpoint != null) previousCheckpoint.Deactivate();
        currentCheckpoint = cp;
        previousCheckpoint = currentCheckpoint;
    }
}
