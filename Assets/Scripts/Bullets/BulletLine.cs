using System.Collections;
using UnityEngine;

public class BulletLine : Bullet
{
    public override void Launch(Vector2 velocity) 
    {
        base.Launch(velocity);
        transform.rotation = Quaternion.Euler(Vector3.forward * (Vector2.SignedAngle(Vector2.up, velocity.normalized) + 90));
    }

    public void MoveStraight()
    {
        m_animator.SetTrigger("Reset");
        m_rigidbody.velocity = velocity;
    }
}
