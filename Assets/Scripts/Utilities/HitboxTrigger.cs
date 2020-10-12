using UnityEngine;

public class HitboxTrigger : MonoBehaviour
{
    [SerializeField] private GameObject source;

    public IChildHitboxEvent enemy;

    private Hitbox hitbox;

    private void Awake() 
    {
        hitbox = GetComponent<Hitbox>();
        if (source) enemy = source.GetComponent<IChildHitboxEvent>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemy != null)
        {
            enemy.ChildHitboxEnterEvent(collision, hitbox);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (enemy != null)
        {
            enemy.ChildHitboxExitEvent(collision, hitbox);
        }
    }
}
