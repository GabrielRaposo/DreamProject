using UnityEngine;

public class ConferSuperJump : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerDreamPhase player = collision.GetComponent<PlayerDreamPhase>();
            if (player)
            {
                player.SetJump(true);
            }
        }
    }
}
