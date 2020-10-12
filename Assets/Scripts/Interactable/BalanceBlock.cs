using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceBlock : MonoBehaviour, IPlatformEvent
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed;
    [SerializeField] private BalanceBlock[] connectedBlocks;

    private Vector2 originalPosition;
    private Vector2 max;
    private Vector2 min;

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
                    transform.position += Vector3.up * moveSpeed * .3f;
                }
                if (connectedBlocks.Length > 0) UpdatePosition();
                break;
        }

        if (hammerPush > 0) hammerPush--;
    }

    public void OnLandEvent()
    {
        movement = Movement.Down;
    }

    public void OnLeaveEvent()
    {
        if (originalPosition.y > min.y)
        {
            movement = Movement.Up;
        }
        else
        {
            movement = Movement.None;
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
    }

}
