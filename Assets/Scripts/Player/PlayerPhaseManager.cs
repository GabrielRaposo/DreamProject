using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerPhaseManager : MonoBehaviour, IPhaseManager
{
    [SerializeField] private GameObject transitionEffect;
    [SerializeField] private Transform actionRegion;

    [Header("Phases")]
    [SerializeField] PlayerPlatformer platformerPhase;
    [SerializeField] PlayerShooter shooterPhase;

    [Header("Health")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int maxHealth;

    [Header("Effects")]
    [SerializeField] private GameObject spawnWindow;
    [SerializeField] private ParticleSystem deathFX;
    [SerializeField] private AudioSource deathSFX;
    [SerializeField] private AudioSource transitionChargeSFX;
    [SerializeField] private float screenShakeVelocity;

    [HideInInspector] public bool onTransition;

    private bool paused;

    private bool switchLock;
    private Transform targetTransform;

    private CameraPriorityManager cameraPriorityManager;
    private CinemachineImpulseSource cinemachineImpulseSource;
    private PauseScreen pauseScreen;

    public static GameManager gameManager;
    public static PlayerPhaseManager instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

        targetTransform = platformerPhase.transform;
        transitionEffect.SetActive(false);
    }

    private void OnEnable()
    {
        platformerPhase.Init(this);
        shooterPhase.Init(this);
    }

    private void Start()
    {
        pauseScreen = PauseScreen.instance;
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
        cameraPriorityManager = CameraPriorityManager.instance;

        if (playerHealth)
        {
            playerHealth.Init(maxHealth);
        }

        platformerPhase.transform.position = shooterPhase.transform.position;
        cameraPriorityManager.SetFocus(CameraPriorityManager.GameState.PlatformAirborne);

        shooterPhase.gameObject.SetActive(false);
        platformerPhase.gameObject.SetActive(false);

        StartCoroutine(SpawnAnimation());
    }

    private IEnumerator SpawnAnimation()
    {
        yield return new WaitForSeconds(.75f);
        if(spawnWindow) 
        {
            spawnWindow.SetActive(true);
        }
        yield return new WaitForSeconds(.1f);
        platformerPhase.gameObject.SetActive(true);
    }

    private void Update() 
    {
        if(onTransition) return;

        actionRegion.position = targetTransform.position;

        if(Input.GetButtonDown("Start"))
        {
            paused = pauseScreen.ToggleState();
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
        Vector2 shooterMovement = shooterPhase.GetComponent<Rigidbody2D>().velocity;
        shooterPhase.gameObject.SetActive(false);

        Vector3 movement = GetMovement(shooterPhase.transform.position, nightmatrix);

        yield return MoveTransitionEffect(shooterPhase.transform, movement * multiplier, true, nightmatrix.transform);

        platformerPhase.transform.position = transitionEffect.transform.position;
        if (Physics2D.CircleCast(transitionEffect.transform.position, .1f, Vector2.one, .1f, 1 << LayerMask.NameToLayer("Ground"))) 
        {
            StartCoroutine(TransitionToNightmare(nightmatrix, 1.5f));
        }
        else
        {
            platformerPhase.gameObject.SetActive(true);
            targetTransform = platformerPhase.transform;
            cameraPriorityManager.SetFocus(CameraPriorityManager.GameState.PlatformAirborne);


            platformerPhase.SwitchIn (
                nightmatrix.transform.position, 
                movement, 
                shooterPhase.PlayerState == PlayerShooter.State.Dashing ? true : false,
                shooterMovement
            );
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
        platformerPhase.gameObject.SetActive(false);

        Vector3 movement = GetMovement(platformerPhase.transform.position, nightmatrix);
        yield return MoveTransitionEffect(platformerPhase.transform, movement * multiplier, false, nightmatrix.transform);

        shooterPhase.transform.position = transitionEffect.transform.position;
        if (Physics2D.CircleCast(transitionEffect.transform.position, .1f, Vector2.one, .1f, 1 << LayerMask.NameToLayer("Ground")))
        {
            if(movement != Vector3.zero) {
                StartCoroutine(TransitionToDream(nightmatrix, 1.5f));
            }
            else {
                shooterPhase.gameObject.SetActive(true);
                Die();
            }
        }
        else
        {
            shooterPhase.gameObject.SetActive(true);
            targetTransform = shooterPhase.transform;
            cameraPriorityManager.SetFocus(CameraPriorityManager.GameState.Shooter);

            shooterPhase.SwitchIn(nightmatrix.transform.position, nightmatrix.GetComponent<Nightmatrix>());
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

    public void TakeDamage(float damage)
    {
        //cinemachineImpulseSource.GenerateImpulse(Vector2.right * screenShakeVelocity);

        if (playerHealth)
        {
            playerHealth.ChangeHealth(-damage);
        }
    }

    public void Heal(int value)
    {
        if (playerHealth)
        {
            playerHealth.ChangeHealth(value);
        }
    }

    public float GetHealth()
    {
        return PlayerHealth.value;
    }

    public void SetHealth(float value)
    {
        //
    }

    public void Die()
    {
        if (deathFX && deathSFX) 
        { 
            deathFX.transform.position = targetTransform.position;
            deathFX.Play();
            deathSFX.Play();
        }
        platformerPhase.gameObject.SetActive(false);
        shooterPhase.gameObject.SetActive(false);
        if (gameManager) 
        {
            onTransition = true;
            gameManager.RestartScene();
        }
    }

    public void TravelThroughExit(Vector3 exitPosition, Window window)
    {
        if(onTransition) return;
        onTransition = true;

        platformerPhase.enabled = false;
        StartCoroutine(TravelThroughWindowAnimation(exitPosition, window));
    }

    private IEnumerator TravelThroughWindowAnimation(Vector3 exitPosition, Window window)
    {
        Rigidbody2D playerRB = platformerPhase.GetComponent<Rigidbody2D>();
        playerRB.bodyType = RigidbodyType2D.Kinematic;
        playerRB.velocity = ((playerRB.transform.position - exitPosition).normalized * 2f);
        if (playerRB.velocity.y < .3f)  playerRB.velocity = new Vector2(playerRB.velocity.x, 2f);
        transitionChargeSFX.Play();
        yield return new WaitForSecondsRealtime(.5f);

        transitionEffect.transform.position = playerRB.transform.position;
        transitionEffect.SetActive(true);
        playerRB.gameObject.SetActive(false);
        while (Vector3.Distance(transitionEffect.transform.position, exitPosition) > .1f)
        {
            yield return new WaitForEndOfFrame();
            transitionEffect.transform.position += (exitPosition - transitionEffect.transform.position).normalized * .1f;
        }
        transitionEffect.SetActive(false);
        if(window) 
        {
            window.Close();
            if(window.isFinal())
            {
                gameManager.ReturnToStageSelect();
            }
            else 
            {
                gameManager.CallNextStage();
            }
        }
        else if (gameManager) gameManager.CallNextStage();
    }
}
