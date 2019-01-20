using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : MonoBehaviour
{
    public float moveSpeed;
    public bool aimAtPlayerOnStart;
    public bool turnAroundOnWall;

    public LayerMask reverseOnLayer;

    private bool facingRight;
    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;

    private void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (aimAtPlayerOnStart)
        {
            PlayerGroundMovement player = PlayerGroundMovement.instance;
            if (player)
            {
                SetFacingSide((transform.position.x < player.transform.position.x) ? true : false);
            }
        }
    }

    void Update()
    {
        Vector2 velocity = rigidbody.velocity;
        velocity.x = moveSpeed * (facingRight ? 1 : -1);
        rigidbody.velocity = velocity;
    }

    public void SetFacingSide(bool lookingRight)
    {
        this.facingRight = lookingRight;
        if (renderer)
        {
            renderer.flipX = lookingRight;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEvent(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionEvent(collision);
    }

    void CollisionEvent(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            return;
        }

        if (collision.contactCount > 0)
        {
            foreach (ContactPoint2D cp in collision.contacts)
            {
                Vector2 point = cp.point - (Vector2)transform.position;
                if (point.y > -.4f)
                {
                    SetFacingSide(point.x < 0 ? true : false);
                    break;
                }
            }
        }
    }
}
