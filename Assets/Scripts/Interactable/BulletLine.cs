using System.Collections;
using UnityEngine;

public class BulletLine : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_rigidbody;

    public void Launch (Vector2 velocity)
    {
        m_rigidbody.velocity = velocity;
    }
}
