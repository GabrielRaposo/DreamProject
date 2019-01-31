using System.Collections;
using UnityEngine;

public class InheritPlatformMovement : MonoBehaviour
{
    private Vector3 colliderExtents;
    private LayerMask platformLayer;
    private Vector3 lastPlatformPosition;
    private Transform currentPlatform;

    private void OnEnable()
    {
        colliderExtents = GetComponent<Collider2D>().bounds.extents;
    }

    void Start()
    {
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            MoveWithPlatform();
        }
    }

    private void MoveWithPlatform()
    {
        if (lastPlatformPosition != currentPlatform.position)
        {
            Vector3 diff = currentPlatform.position - lastPlatformPosition;
            transform.position += diff;

            lastPlatformPosition = currentPlatform.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;
        if(layer == platformLayer)
        {
            foreach(ContactPoint2D cp in collision.contacts)
            {
                if(cp.point.y < transform.position.y - colliderExtents.y + .01f)
                {
                    SetPlatform(collision.transform);
                    break;
                }
            }
        }
    }

    public void SetPlatform(Transform t)
    {
        currentPlatform = t;
        lastPlatformPosition = currentPlatform.position;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;
        if (layer == platformLayer && collision.transform == currentPlatform)
        {
            currentPlatform = null;
        }
    }
}
