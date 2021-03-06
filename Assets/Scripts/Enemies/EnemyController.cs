﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IPhaseManager, IRespawnable
{
    [SerializeField] protected float health;

    [Header("Region Spawn")]
    [SerializeField] private GameObject regionSpawner;

    [Header("Phases")]
    [SerializeField] protected PlatformerCreature platformerPhase;
    [SerializeField] protected ShooterCreature shooterPhase;

    [Header("Effects")]
    [SerializeField] protected GameObject transitionEffect;
    [SerializeField] protected GameObject destructionFX;

    protected Vector3 movement;
    protected Transform currentPhase;
    protected bool switchLock;
    protected bool isShooterPhase;

    private EnemyRespawner respawner;

    [HideInInspector] public ChallengeBarrier challengeBarrier;

    public void ChangePhase()
    {
        isShooterPhase = !isShooterPhase;

        platformerPhase.gameObject.SetActive(!isShooterPhase);
        shooterPhase.gameObject.SetActive(isShooterPhase);
    }

    void Start()
    {
        platformerPhase.Init(this);
        shooterPhase.Init(this);

        isShooterPhase = shooterPhase.isActiveAndEnabled;
        currentPhase = isShooterPhase ? shooterPhase.transform : platformerPhase.transform;

        regionSpawner = Instantiate(regionSpawner, transform.position, Quaternion.identity);
        regionSpawner.transform.position = transform.position;
        regionSpawner.SetActive(true);
        regionSpawner.GetComponent<ActionRegionSpawner>().Setup(gameObject);

        gameObject.SetActive(false);
    }
    
    public float Health()
    {
        return health;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public void Die()
    { 
        if (respawner != null)
        {
            respawner.Spawn();
        }

        if (destructionFX != null)
        {
            Instantiate(destructionFX, currentPhase.transform.position, Quaternion.identity);
        }

        if (challengeBarrier != null)
        {
            challengeBarrier.NotifyDeath();
            challengeBarrier = null;
        }

        Destroy(gameObject);
    }

        public void SetDreamPhase(Nightmatrix nightmatrix)
    {
        if (!switchLock)
        {
            StartCoroutine(SwitchLockTimer());
            StartCoroutine(TransitionToDream(nightmatrix));
        }
    }

    protected virtual IEnumerator TransitionToDream(Nightmatrix nightmatrix)
    {
        shooterPhase.SwitchOut();
        shooterPhase.gameObject.SetActive(false);

        movement = GetMovement(shooterPhase.transform.position, nightmatrix);

        yield return MoveTransitionEffect(shooterPhase.transform, movement, true, nightmatrix.transform);

        platformerPhase.transform.position = transitionEffect.transform.position;
        currentPhase = platformerPhase.transform;
        platformerPhase.gameObject.SetActive(true);
    }

        public void SetNightmarePhase(Nightmatrix nightmatrix)
    {
        if (!switchLock)
        {
            StartCoroutine(SwitchLockTimer());
            StartCoroutine(TransitionToNightmare(nightmatrix));
        }
    }

    protected virtual IEnumerator TransitionToNightmare(Nightmatrix nightmatrix)
    {
        platformerPhase.gameObject.SetActive(false);

        movement = GetMovement(platformerPhase.transform.position, nightmatrix);

        yield return MoveTransitionEffect(platformerPhase.transform, movement, false, nightmatrix.transform);

        shooterPhase.transform.position = transitionEffect.transform.position;
        currentPhase = shooterPhase.transform;
        shooterPhase.gameObject.SetActive(true);

        shooterPhase.SwitchIn(nightmatrix);
    }

    protected Vector3 GetMovement(Vector3 body, Nightmatrix matrix)
    {
        Vector3 direction = Vector3.zero;
        float extraOffset = .5f;

        if (body.y > matrix.transform.position.y + (matrix.size.y / 2) - extraOffset)
            direction += Vector3.up;
        if (body.y < matrix.transform.position.y - (matrix.size.y / 2) + extraOffset)
            direction += Vector3.down;

        if (body.x > matrix.transform.position.x + (matrix.size.x / 2) - extraOffset)
            direction += Vector3.right;
        if (body.x < matrix.transform.position.x - (matrix.size.x / 2) + extraOffset)
            direction += Vector3.left;

        return direction * 1.5f;
    }

    protected IEnumerator MoveTransitionEffect(Transform startingPosition, Vector3 movement, bool moveIn, Transform matrix)
    {
        transitionEffect.transform.position = startingPosition.position;
        transitionEffect.SetActive(true);

        if (!moveIn) movement *= -1;

        Vector3 matrixPos = matrix.position;

        int iterations = 8;
        for (int i = 0; i < iterations; i++)
        {
            yield return new WaitForFixedUpdate();
            transitionEffect.transform.position += (movement / iterations);
            if (matrixPos != matrix.position) transitionEffect.transform.position += (matrix.position - matrixPos);
            matrixPos = matrix.position;
        }

        transitionEffect.SetActive(false);
    }

    protected IEnumerator SwitchLockTimer()
    {
        switchLock = true;
        yield return new WaitForSeconds(.1f);
        switchLock = false;
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float value)
    {
        health = value;
    }

    public void SetRespawner(EnemyRespawner respawner) 
    {
        this.respawner = respawner;
    }
}
