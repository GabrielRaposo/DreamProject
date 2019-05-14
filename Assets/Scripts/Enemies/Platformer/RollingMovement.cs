using UnityEngine;

public class RollingMovement : MonoBehaviour
{
    [SerializeField] private Collider2D rollHitbox;

    private PlatformerCreature platformerCreature;
    private Rigidbody2D m_rigidbody2D;    
    private Vector2 velocity;

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

        rollHitbox.enabled = true;
        rollHitbox.GetComponent<Hitbox>().direction = velocity.normalized;
    }

    void Update()
    {
        m_rigidbody2D.velocity = new Vector2(velocity.x, m_rigidbody2D.velocity.y);
    }
}
