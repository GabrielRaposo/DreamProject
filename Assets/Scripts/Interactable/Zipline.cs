using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zipline : MonoBehaviour
{
    private SpriteRenderer m_renderer;
    private BoxCollider2D m_collider;

    private void OnEnable()
    {
        m_renderer = GetComponent<SpriteRenderer>();
        m_collider = GetComponent<BoxCollider2D>();

        m_collider.size = m_renderer.size;

        tag = "Zipline";
    }
}
