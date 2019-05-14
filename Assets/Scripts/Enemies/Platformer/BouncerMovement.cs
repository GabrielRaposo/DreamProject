using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncerMovement : MonoBehaviour
{
    [SerializeField] private Collider2D bounceHitbox;
    [SerializeField] private PhysicsMaterial2D bouncyMaterial;

    private PlatformerCreature platformerCreature;
    private Rigidbody2D m_rigidbody2D;    
    private Vector2 velocity;
    
    private int bounceCount;
    private bool canCount;

    private void Awake() 
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Launch(PlatformerCreature platformerCreature, Vector2 velocity)
    {
        this.platformerCreature = platformerCreature;
        this.velocity = velocity;

        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_rigidbody2D.velocity = velocity;
        m_rigidbody2D.sharedMaterial = bouncyMaterial;

        bounceHitbox.enabled = true;
        bounceHitbox.GetComponent<Hitbox>().direction = velocity.normalized;

        bounceCount = 2;
        StartCoroutine(CountDelay());
    }
    
    private IEnumerator CountDelay()
    {
        canCount = false;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        canCount = true;
    }

    public bool CountBounce()
    {
        if (canCount)
        {
            bounceCount--;
            StartCoroutine(CountDelay());
        }
        return bounceCount > 0;
    }
}
