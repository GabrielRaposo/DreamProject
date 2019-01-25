using UnityEngine;

public class ConferSuperJump : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player)
            {
                player.SetJump(true);
            }
        }
    }
}
