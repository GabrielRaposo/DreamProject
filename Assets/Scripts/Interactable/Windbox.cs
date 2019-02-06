using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windbox : MonoBehaviour
{
    [SerializeField] private ParticleSystem windFX;

    private void Start()
    {
        AreaEffector2D m_areaEffector = GetComponent<AreaEffector2D>();

        windFX.gameObject.SetActive(true);

        BoxCollider2D m_collider = GetComponent<BoxCollider2D>();
        windFX.transform.localPosition = Vector3.right * m_collider.size.x * .5f * (m_areaEffector.forceAngle > 179 ? 1 : -1);
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = windFX.velocityOverLifetime;
        velocityOverLifetime.x = m_collider.size.x * .5f * (m_areaEffector.forceAngle > 179 ? -1 : 1);
    }
}
