using UnityEngine;

public class LaunchMovement : MonoBehaviour
{
    [SerializeField] private Collider2D launchHitbox;

    private ShooterCreature shooterCreature;
    private Rigidbody2D m_rigidbody2D;
    private Vector2 velocity;

    private void Awake() 
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Launch(ShooterCreature shooterCreature, Vector2 velocity)
    {
        this.shooterCreature = shooterCreature;
        this.velocity = velocity;

        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_rigidbody2D.velocity = velocity;

        launchHitbox.enabled = true;
        launchHitbox.GetComponent<Hitbox>().direction = velocity.normalized;
    }
    
    void Update()
    {
        m_rigidbody2D.velocity = new Vector2(velocity.x, m_rigidbody2D.velocity.y);
    }
}
