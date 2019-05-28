using UnityEngine;

public interface IPhaseManager
{
    void SetDreamPhase(Nightmatrix nightmatrix);
    void SetNightmarePhase(Nightmatrix nightmatrix);

    void TakeDamage(float damage);
    float GetHealth();
    void SetHealth(float value);
    void Die();
}
