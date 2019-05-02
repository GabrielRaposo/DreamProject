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

    [Header("Effects")]
    [SerializeField] private ParticleSystem deathFX;

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

    public Transform GetTarget()
    {
        if (targetTransform != null)
        {
            return targetTransform;
        }
        return transform;
    }

    private IEnumerator SwitchLockTimer()
    {
        switchLock = true;
        yield return new WaitForSeconds(.1f);
        switchLock = false;
    }

    public void SetDreamPhase(Nightmatrix nightmatrix)
    {
        if (!switchLock)
        {
            StartCoroutine(SwitchLockTimer());
            StartCoroutine(TransitionToDream(nightmatrix));
        }
    }

    private IEnumerator TransitionToDream(Nightmatrix nightmatrix, float multiplier = 1f)
    {
        nightmarePhase.gameObject.SetActive(false);

        Vector3 movement = GetMovement(nightmarePhase.transform.position, nightmatrix);

        yield return MoveTransitionEffect(nightmarePhase.transform, movement * multiplier, true, nightmatrix.transform);

        dreamPhase.transform.position = transitionEffect.transform.position;
        if (Physics2D.CircleCast(transitionEffect.transform.position, .1f, Vector2.one, .1f, 1 << LayerMask.NameToLayer("Ground"))) 
        {
            Debug.Log("wall found");
            StartCoroutine(TransitionToNightmare(nightmatrix, 1.5f));
        }
        else
        {
            dreamPhase.gameObject.SetActive(true);
            targetTransform = dreamPhase.transform;

            dreamPhase.SwitchIn(nightmatrix.transform.position, movement.y > -.1f);
        }
    }

    public void SetNightmarePhase(Nightmatrix nightmatrix)
    {
        if (!switchLock)
        {
            StartCoroutine(SwitchLockTimer());
            StartCoroutine(TransitionToNightmare(nightmatrix));
        }
    }

    private IEnumerator TransitionToNightmare(Nightmatrix nightmatrix, float multiplier = 1f)
    {
        dreamPhase.gameObject.SetActive(false);

        Vector3 movement = GetMovement(dreamPhase.transform.position, nightmatrix);
        yield return MoveTransitionEffect(dreamPhase.transform, movement * multiplier, false, nightmatrix.transform);

        nightmarePhase.transform.position = transitionEffect.transform.position;
        if (Physics2D.CircleCast(transitionEffect.transform.position, .1f, Vector2.one, .1f, 1 << LayerMask.NameToLayer("Ground")))
        {
            if(movement != Vector3.zero) {
                StartCoroutine(TransitionToDream(nightmatrix, 1.5f));
            }
            else {
                nightmarePhase.gameObject.SetActive(true);
                Die();
            }
        }
        else
        {
            nightmarePhase.gameObject.SetActive(true);
            targetTransform = nightmarePhase.transform;

            nightmarePhase.SwitchIn(nightmatrix.transform.position, nightmatrix.GetComponent<Nightmatrix>());
        }
    }

    private Vector3 GetMovement(Vector3 body, Nightmatrix matrix)
    {
        Vector3 direction = Vector3.zero;
        float extraOffset = .5f;

        if (body.y > matrix.transform.position.y + (matrix.size.y / 2) - extraOffset)
            direction += Vector3.up;
        else if (body.y < matrix.transform.position.y - (matrix.size.y / 2) + extraOffset)
            direction += Vector3.down;
        
        if (body.x > matrix.transform.position.x + (matrix.size.x / 2) - extraOffset)
            direction += Vector3.right;
        else if (body.x < matrix.transform.position.x - (matrix.size.x / 2) + extraOffset)
            direction += Vector3.left;

        return direction * .8f;
    }

    private IEnumerator MoveTransitionEffect(Transform startingPosition, Vector3 movement, bool moveIn, Transform matrix)
    {        
        transitionEffect.transform.position = startingPosition.position;
        transitionEffect.SetActive(true);
        targetTransform = transitionEffect.transform;

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

    public void TakeDamage(int damage)
    {
        if (healthDisplay)
        {
            healthDisplay.ChangeValue(-damage);
        }
    }

    public int GetHealth()
    {
        return healthDisplay.value;
    }

    public void SetHealth(int value)
    {
        //
    }

    public void Die()
    {
        if (deathFX) 
        { 
            deathFX.transform.position = targetTransform.position;
            deathFX.Play();
        }
        dreamPhase.gameObject.SetActive(false);
        nightmarePhase.gameObject.SetActive(false);
        if (gameManager) gameManager.RestartScene();
        //Destroy(gameObject);
    }
}
