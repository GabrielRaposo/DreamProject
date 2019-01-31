using UnityEngine;

public class LinearDirection : MonoBehaviour
{
    public Vector3 velocity;

    private void FixedUpdate()
    {
        //if (Time.timeScale == 0) return;
        
        transform.position += velocity;
    }
}
