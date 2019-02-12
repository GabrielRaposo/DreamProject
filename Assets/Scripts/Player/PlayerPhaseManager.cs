using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerPhaseManager : MonoBehaviour, IPhaseManager
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private GameObject transitionEffect;

    [Header("Phases")]
    [SerializeField] PlayerDreamPhase dreamPhase;
    [SerializeField] PlayerNightmarePhase nightmarePhase;

    [Header("Health")]
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private int maxHealth;

    private bool switchLock;
    private Transform targetTransform;

    public static GameManager gameManager;
    public static PlayerPhaseManager instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

        targetTransform = dreamPhase.transform;
        transitionEffect.SetActive(false);
    }

    private void OnEnable()
    {
        dreamPhase.Init(this);
        nightmarePhase.Init(this);
    }

    private void Start()
    {
        if (healthDisplay)
        {
            healthDisplay.Init(maxHealth);
        }

        dreamPhase.transform.position = nightmarePhase.transform.position;

        nightmarePhase.gameObject.SetActive(false);
        dreamPhase.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (targetTransform != null)
        {
            followTarget.position = targetTransform.position;
        }
    }

    public Vector3 GetTargetPosition()
    {
        if (targetTransform != null)
        {
            return targetTransform.position;
        }
        return Vector3.zero;
    }

    private IEnumerator SwitchLockTimer()
    {
        switchLock = true;
        yield return new WaitForSeconds(.1f);
        switchLock = false;
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
        nightmarePhase.gameObject.SetActive(false);

        Vector3 movement = GetMovement(nightmarePhase.transform.position, nightmatrix.transform);

        yield return MoveTransitionEffect(nightmarePhase.transform, movement, true);

        dreamPhase.transform.position = transitionEffect.transform.position;
        dreamPhase.gameObject.SetActive(true);
        targetTransform = dreamPhase.transform;

        dreamPhase.SwitchIn(nightmatrix.transform.position, movement.y > -.1f);
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
        targetTransform = nightmarePhase.transform;

        nightmarePhase.SwitchIn(nightmatrix.transform.position, nightmatrix.GetComponent<Nightmatrix>());
    }

    private Vector3 GetMovement(Vector3 body, Transform matrix)
    {
        Vector3 direction = Vector3.zero;
        float extraOffset = .5f;

        if (body.y > matrix.position.y + (matrix.localScale.y / 2) - extraOffset)
            direction += Vector3.up;
        else if (body.y < matrix.position.y - (matrix.localScale.y / 2) + extraOffset)
            direction += Vector3.down;
        
        if (body.x > matrix.position.x + (matrix.localScale.x / 2) - extraOffset)
            direction += Vector3.right;
        else if (body.x < matrix.position.x - (matrix.localScale.x / 2) + extraOffset)
            direction += Vector3.left;

        return direction * .8f;
    }

    private IEnumerator MoveTransitionEffect(Transform startingPosition, Vector3 movement, bool moveIn)
    {
        transitionEffect.transform.position = startingPosition.position;
        transitionEffect.SetActive(true);
        targetTransform = transitionEffect.transform;

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

    public void TakeDamage(int damage)
    {
        if (healthDisplay)
        {
            healthDisplay.ChangeValue(-damage);
        }
    }

    public int Health()
    {
        return healthDisplay.value;
    }

    public void CheckHealth()
    {
        if (healthDisplay && healthDisplay.value < 1)
        {
            if (gameManager) gameManager.RestartScene();
            Destroy(gameObject);
        }
    }

    public void Die()
    {
        if (gameManager) gameManager.RestartScene();
        Destroy(gameObject);
    }
}
