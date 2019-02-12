using UnityEngine;

public interface IPhaseManager
{
    void SetDreamPhase(GameObject nightmatrix);
    void SetNightmarePhase(GameObject nightmatrix);

    void TakeDamage(int damage);
    void CheckHealth();
    void Die();
}
