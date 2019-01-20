using UnityEngine;

public enum ID
{
    Player, 
    Enemy,
    Other
}

public class Hitbox : MonoBehaviour
{
    public enum Type { Hammer, Simple }

    public ID id;
    public int damage = 1;
    [HideInInspector] public Vector2 direction;
}
