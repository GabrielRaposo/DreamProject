using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windbox : MonoBehaviour
{
    public Vector3 force;
    [SerializeField] private ParticleSystem windFX;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            collision.transform.position += (force / 100);
        }
    }

    private void Start()
    {
        windFX.gameObject.SetActive(true);

        BoxCollider2D m_collider = GetComponent<BoxCollider2D>();
        windFX.transform.localPosition = Vector3.right * m_collider.size.x * .5f * (force.x < 0 ? 1 : -1);
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = windFX.velocityOverLifetime;
        velocityOverLifetime.x = m_collider.size.x * .5f * (force.x < 0 ? -1 : 1);
    }
}
