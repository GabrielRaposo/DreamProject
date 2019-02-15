using UnityEngine;

public interface IPhaseManager
{
    void SetDreamPhase(GameObject nightmatrix);
    void SetNightmarePhase(GameObject nightmatrix);

    void TakeDamage(int damage);
    int GetHealth();
    void SetHealth(int value);
    void Die();
}
