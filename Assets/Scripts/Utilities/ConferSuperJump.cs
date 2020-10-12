using UnityEngine;

public class ConferSuperJump : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerPlatformer player = collision.GetComponent<PlayerPlatformer>();
            if (player)
            {
                player.SetJump(1.5f);
            }
        }
    }
}
