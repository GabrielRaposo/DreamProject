using UnityEngine;

public class ActionRegionSpawner : MonoBehaviour
{
    private GameObject spawn;    

    public void Setup(GameObject spawn)
    {
        this.spawn = spawn;
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.CompareTag("ActionRegion") && spawn != null)
        {
            spawn.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
