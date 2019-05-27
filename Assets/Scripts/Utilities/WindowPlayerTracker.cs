using UnityEngine;

public class WindowPlayerTracker : MonoBehaviour
{
    [SerializeField] private Window window;

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            window.Open();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            window.Close();
        }
    }
}
