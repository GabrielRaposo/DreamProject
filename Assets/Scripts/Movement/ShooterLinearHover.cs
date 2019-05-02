using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterLinearHover : ShooterMovement
{
    [SerializeField] private float speed;

    private float targetSpeed;
    private Vector2 colliderPosition = new Vector2();
    private Vector2 colliderHalfSize = new Vector2();

    public override void Call(Nightmatrix nightmatrix)
    {
        this.nightmatrix = nightmatrix;

        colliderPosition = nightmatrix.transform.position;
        colliderHalfSize = nightmatrix.GetComponent<BoxCollider2D>().size / 2;

        m_rigidbody.velocity = Vector2.zero;        
        targetSpeed = speed * ((transform.position.y > nightmatrix.transform.position.y) ? -1 : 1);
    }

    void Update()
    {
        if (m_rigidbody.velocity.y != targetSpeed)
        {
            m_rigidbody.velocity += Vector2.up * targetSpeed / 20;
            if(Mathf.Abs(m_rigidbody.velocity.y) > Mathf.Abs(targetSpeed))
            {
                m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, targetSpeed);
            }
        }

        if (colliderHalfSize != Vector2.zero)
        {
            float margin = 1.5f;
            if(transform.position.y + (targetSpeed > 0 ? 1 : -1) > colliderPosition.y + colliderHalfSize.y - margin ||
               transform.position.y + (targetSpeed > 0 ? 1 : -1) < colliderPosition.y - colliderHalfSize.y + margin )
            {
                targetSpeed *= -1;
            }
        } 
    }
}
