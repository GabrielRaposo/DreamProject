using UnityEngine;

public class HitboxTrigger : MonoBehaviour
{
    [SerializeField] private PlatformerCreature enemy;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemy)
        {
            enemy.ChildHitboxEnterEvent(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (enemy)
        {
            enemy.ChildHitboxExitEvent(collision);
        }
    }
}
