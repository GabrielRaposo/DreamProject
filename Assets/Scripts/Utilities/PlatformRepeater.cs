using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformRepeater : MonoBehaviour
{
    [SerializeField] private Transform output;

    private LayerMask platformLayer;
    private LinearDirection linearDirection;

    private void OnEnable()
    {
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void Update()
    {
        if (linearDirection != null)
        {
            if((transform.position - linearDirection.transform.position).magnitude < linearDirection.velocity.magnitude)
            {
                linearDirection.transform.position = output.position;
                linearDirection = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;
        if (layer == platformLayer)
        {
            linearDirection = collision.GetComponent<LinearDirection>();
        }
    }
}
