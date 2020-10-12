using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerPlatformer controller;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private LayerMask groundLayer;

    private LayerMask platformLayer;

    private void Awake()
    {
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollidingWith(collision, true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CollidingWith(collision, false);
    }

    private void CollidingWith(Collider2D collision, bool firstFrame)
    {
        int layer = collision.gameObject.layer;

        if (playerRB.velocity.y > 0) return;

        if (groundLayer == (groundLayer | (1 << layer)))
        {
            controller.OnGround(true, collision); 

            if (layer == platformLayer)
            {
                controller.transform.parent = collision.transform;

                Collider2D coll = collision.GetComponent<Collider2D>();
                if (firstFrame && coll)
                {
                    Vector2 position = controller.transform.position;
                    position.y = collision.transform.position.y + coll.bounds.extents.y + .5f;
                    if (Mathf.Abs(controller.transform.position.y - position.y) < .2f)
                    {
                        controller.transform.position = position;
                    }

                    IPlatformEvent platformEvent = collision.GetComponent<IPlatformEvent>();
                    if (platformEvent != null)
                    {
                        platformEvent.OnLandEvent();
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IPlatformEvent platformEvent = collision.GetComponent<IPlatformEvent>();
        if (platformEvent != null)
        {
            platformEvent.OnLeaveEvent();
        }

        if (groundLayer == (groundLayer | (1 << collision.gameObject.layer)))
        {
            controller.OnGround(false, collision);

            if (collision.transform == controller.transform.parent)
            {
                controller.transform.parent = null;
            }
        }
    }
}
