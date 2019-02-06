using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zipline : MonoBehaviour
{
    public float speed;

    private SpriteRenderer m_renderer;
    private BoxCollider2D m_collider;

    public Vector3 startPosition { get; private set; }
    public Vector3 targetPosition { get; private set; }
    private Vector3 moveDirection;

    private bool disabled;
    public bool Disabled
    {
        get
        {
            return disabled;
        }
        set
        {
            disabled = value;
            if (value)
            {
                StartCoroutine(DisableTimer());
            }
        }
    }

    private IEnumerator DisableTimer()
    {
        for (int i = 0; i < 15; i++)
            yield return new WaitForFixedUpdate();
        disabled = false;
    }

    private void OnEnable()
    {
        m_renderer = GetComponent<SpriteRenderer>();
        m_collider = GetComponent<BoxCollider2D>();

        m_collider.size = m_renderer.size;

        targetPosition = RaposUtil.RotateVector(Vector3.right * (m_renderer.size / 2), transform.rotation.eulerAngles.z);
        startPosition = transform.position - targetPosition; 
        targetPosition += transform.position;
        moveDirection = (targetPosition - startPosition).normalized;

        tag = "Zipline";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, .3f);
        Gizmos.DrawSphere(startPosition, .3f);
    }

    public Vector3 Movement()
    {
        return moveDirection * speed;
    }

    public Vector3 SnapedPosition(Vector3 playerPosition)
    {
        Vector2 p0 = startPosition;
        Vector2 p1 = targetPosition;
        Vector2 p2 = playerPosition;

        if (Mathf.Abs(p0.x - p1.x) < .001f)
        {
            return new Vector3(p0.x, playerPosition.y);
        }

        if (Mathf.Abs(p0.y - p1.y) < .001f)
        {
            return new Vector3(playerPosition.x, p0.y);
        }

        float A1 = (p1.y - p0.y) / (p1.x - p0.x);
        float B1 = p0.y - (A1 * p0.x);

        float A2 = (-1 / A1);
        float B2 = p2.y - (A2 * p2.x);

        float x = (B2 - B1) / (A1 - A2);
        float y = EqReta(A2, x, p2.x, p2.y);

        return new Vector2(x, y);
    } 

    private float EqReta(float A, float x, float x0, float y0)
    {
        return (A * (x - x0)) + y0;
    }
}
