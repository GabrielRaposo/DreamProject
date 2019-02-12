using System.Collections;
using UnityEngine;

public class BulletLine : Bullet
{
    public void MoveStraight()
    {
        m_rigidbody.velocity = velocity;
    }
}
