using UnityEngine;

public class ChallengeBarrier : MonoBehaviour
{
    [SerializeField] private EnemyController[] enemies;
    [SerializeField] private EnemyRespawner enemyRespawner;
    [SerializeField] private bool autoFocus = true; 
    
    private int count;

    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;
    private AudioSource completeSFX;

    private void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        completeSFX = GetComponent<AudioSource>();

        boxCollider2D.size = spriteRenderer.size - Vector2.one;

        foreach(EnemyController e in enemies)
        {
            e.challengeBarrier = this;
        }
        count = enemies.Length;
    
        if (autoFocus) CameraPriorityManager.instance.SetSpecialFocus(transform, 5.5f);
    }

    public void NotifyDeath() 
    {
        count--;
        if(count < 1)
        {
            if (autoFocus) CameraPriorityManager.instance.ReturnToPreviousFocus();

            if(enemyRespawner != null) enemyRespawner.gameObject.SetActive(false);

            if (completeSFX) completeSFX.Play();
            boxCollider2D.enabled = false;
            spriteRenderer.enabled = false;

            Destroy(gameObject, 3f);
        }
    }
}
