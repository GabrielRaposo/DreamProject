using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EGoomba : MonoBehaviour
{
    public int health;

    [SerializeField] private DenemyGoomba dreamPhase;
    [SerializeField] private NenemyGoomba nightmarePhase;

    void Start()
    {
        
    }

    // private void TransitionTo(State state) { }
}
