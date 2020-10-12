using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnEffect : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private ParticleSystem releaseFX;

    private GameObject spawn;

    public void Set(GameObject spawn)
    {
        this.spawn = spawn;
    }

    public void Release()
    {
        m_animator.SetTrigger("Release");
        releaseFX.Play();
        if(spawn != null) spawn.SetActive(true);
        Destroy(gameObject, 1f);
    }
}
