using UnityEngine;

public class ChallengeBarrier : MonoBehaviour
{
    [SerializeField] private EnemyController[] enemies;
    private int count;

    private void Start()
    {
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        boxCollider2D.size = spriteRenderer.size - Vector2.one;

        foreach(EnemyController e in enemies)
        {
            e.challengeBarrier = this;
        }
        count = enemies.Length;
    
        CameraPriorityManager.instance.SetSpecialFocus(transform);
    }

    public void NotifyDeath() 
    {
        count--;
        if(count < 1)
        {
            CameraPriorityManager.instance.ReturnToPreviousFocus();
            gameObject.SetActive(false);
        }
    }
}
