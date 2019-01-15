using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceBlock : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed;
    [SerializeField] private BalanceBlock[] connectedBlocks;

    private Vector2 originalPosition;
    private Vector2 max;
    private Vector2 min;

    private BoxCollider2D coll;
    private float speedMultiplier;
    private int hammerPush;

    private enum Movement { None, Up, Down }
    private Movement movement;

    private void OnEnable()
    {
        originalPosition = transform.position;

        if(originalPosition.y > target.position.y)
        {
            max = originalPosition;
            min = target.position;
        }
        else
        {
            max = target.position;
            min = originalPosition; 
        }

        coll = GetComponent<BoxCollider2D>();
        moveSpeed *= .01f;
        movement = Movement.None;
    }

    void FixedUpdate()
    {
        switch (movement)
        {
            case Movement.Down:
                if (transform.position.y + (Vector3.down * moveSpeed).y < min.y)
                {
                    transform.position = min;
                    movement = Movement.None;
                }
                else
                {
                    if (hammerPush > 0)
                    {
                        transform.position += Vector3.down * moveSpeed * 10;
                    }
                    else
                    {
                        transform.position += Vector3.down * moveSpeed;
                    }
                }
                if (connectedBlocks.Length > 0) UpdatePosition();
                break;

            case Movement.Up:
                if (transform.position.y + (Vector3.up * moveSpeed).y > max.y)
                {
                    transform.position = max;
                    movement = Movement.None;
                }
                else
                {
                    transform.position += Vector3.up * moveSpeed;
                }
                if (connectedBlocks.Length > 0) UpdatePosition();
                break;
        }

        if (hammerPush > 0) hammerPush--;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            foreach(ContactPoint2D cp in collision.contacts)
            {
                if(cp.point.y > transform.position.y + coll.bounds.extents.y - .1f)
                {
                    movement = Movement.Down;
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if(originalPosition.y > min.y)
            {
                movement = Movement.Up;
            }
            else
            {
                movement = Movement.None;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            hammerPush = 2;    
        }
    }

    private void UpdatePosition()
    {
        foreach(BalanceBlock bb in connectedBlocks)
        {
            float percent = (transform.position.y - originalPosition.y) / (target.position.y - originalPosition.y);
            bb.GetPositionPercent(Mathf.Abs(percent));
        }
    }

    public void GetPositionPercent(float percent)
    {
        float yPosition = (target.position.y - originalPosition.y) * percent;
        transform.position = new Vector2(transform.position.x, yPosition + originalPosition.y);
        movement = Movement.None;
    }
}
