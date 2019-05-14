using UnityEngine;

public interface IChildHitboxEvent
{
    void ChildHitboxEnterEvent(Collider2D collision, Hitbox hitbox);
    void ChildHitboxExitEvent(Collider2D collision, Hitbox hitbox);
}
