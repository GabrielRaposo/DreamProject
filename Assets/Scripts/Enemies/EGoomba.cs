using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EGoomba : MonoBehaviour, IPhaseManager
{
    public int health;

    [Header("Phases")]
    [SerializeField] private YumeGoomba dreamPhase;
    [SerializeField] private AkumuGoomba nightmarePhase;

    [Header("Effects")]
    [SerializeField] private GameObject transitionEffect;
    [SerializeField] private GameObject destructionFX;

    private Transform currentPhase;
    private bool switchLock;

    void Start()
    {
        dreamPhase.Init(this);
        nightmarePhase.Init(this);

        currentPhase = dreamPhase.transform;
    }

    public int Health()
    {
        return health;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    public void Die()
    {
        if (destructionFX != null)
        {
            Instantiate(destructionFX, currentPhase.transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    public void SetDreamPhase(GameObject nightmatrix)
    {
        if (!switchLock)
        {
            StartCoroutine(SwitchLockTimer());
            StartCoroutine(TransitionToDream(nightmatrix));
        }
    }

    private IEnumerator TransitionToDream(GameObject nightmatrix)
    {
        nightmarePhase.SwitchOut();
        nightmarePhase.gameObject.SetActive(false);

        Vector3 movement = GetMovement(nightmarePhase.transform.position, nightmatrix.transform);

        yield return MoveTransitionEffect(nightmarePhase.transform, movement, true);

        dreamPhase.transform.position = transitionEffect.transform.position;
        dreamPhase.gameObject.SetActive(true);

        currentPhase = dreamPhase.transform;
    }

    public void SetNightmarePhase(GameObject nightmatrix)
    {
        if (!switchLock)
        {
            StartCoroutine(SwitchLockTimer());
            StartCoroutine(TransitionToNightmare(nightmatrix));
        }
    }

    private IEnumerator TransitionToNightmare(GameObject nightmatrix)
    {
        dreamPhase.gameObject.SetActive(false);

        Vector3 movement = GetMovement(dreamPhase.transform.position, nightmatrix.transform);

        yield return MoveTransitionEffect(dreamPhase.transform, movement, false);

        nightmarePhase.transform.position = transitionEffect.transform.position;
        nightmarePhase.gameObject.SetActive(true);

        currentPhase = nightmarePhase.transform;
        nightmarePhase.SwitchIn(nightmatrix.GetComponent<Nightmatrix>());
    }

    private Vector3 GetMovement(Vector3 body, Transform matrix)
    {
        Vector3 direction = Vector3.zero;
        float extraOffset = .5f;

        if (body.y > matrix.position.y + (matrix.localScale.y / 2) - extraOffset)
            direction += Vector3.up;
        if (body.y < matrix.position.y - (matrix.localScale.y / 2) + extraOffset)
            direction += Vector3.down;

        if (body.x > matrix.position.x + (matrix.localScale.x / 2) - extraOffset)
            direction += Vector3.right;
        if (body.x < matrix.position.x - (matrix.localScale.x / 2) + extraOffset)
            direction += Vector3.left;

        return direction * .8f;
    }

    private IEnumerator MoveTransitionEffect(Transform startingPosition, Vector3 movement, bool moveIn)
    {
        transitionEffect.transform.position = startingPosition.position;
        transitionEffect.SetActive(true);

        if (!moveIn) movement *= -1;

        int iterations = 8;
        for (int i = 0; i < iterations; i++)
        {
            yield return new WaitForFixedUpdate();
            transitionEffect.transform.position += (movement / iterations);
        }
        transitionEffect.transform.position = startingPosition.position + movement;

        transitionEffect.SetActive(false);
    }

    private IEnumerator SwitchLockTimer()
    {
        switchLock = true;
        yield return new WaitForSeconds(.1f);
        switchLock = false;
    }

    public void CheckHealth()
    {
        if (health < 0)
        {
            Die();
        }
    }
}
