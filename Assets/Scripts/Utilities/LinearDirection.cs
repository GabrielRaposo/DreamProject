using UnityEngine;

public class LinearDirection : MonoBehaviour
{
    public Vector3 velocity;

    private void Update()
    {
        transform.position += velocity;
    }
}
