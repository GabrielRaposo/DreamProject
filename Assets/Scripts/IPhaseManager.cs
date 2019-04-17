using UnityEngine;

public interface IPhaseManager
{
    void SetDreamPhase(Nightmatrix nightmatrix);
    void SetNightmarePhase(Nightmatrix nightmatrix);

    void TakeDamage(int damage);
    int GetHealth();
    void SetHealth(int value);
    void Die();
}
