using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerPhaseManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera followCamera;

    [Header("Phases")]
    [SerializeField] PlayerDreamPhase dreamPhase;
    [SerializeField] PlayerNightmarePhase nightmarePhase;

    [Header("Health")]
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private int maxHealth;

    private bool switchLock;

    public static GameManager gameManager;
    public static PlayerPhaseManager instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        dreamPhase.Init(this);
        nightmarePhase.Init(this);
    }

    void Start()
    {
        if (healthDisplay)
        {
            healthDisplay.Init(maxHealth);
        }

        dreamPhase.transform.position = nightmarePhase.transform.position;
        if (followCamera) followCamera.Follow = dreamPhase.transform;

        nightmarePhase.gameObject.SetActive(false);
        dreamPhase.gameObject.SetActive(true);
    }

    public void ChangeHealth(int damage)
    {
        if (healthDisplay)
        {
            healthDisplay.ChangeValue(-damage);
        }
    }

    public void CheckHealth()
    {
        if (healthDisplay && healthDisplay.value < 1)
        {
            Die();
        }
    }

    public void Die()
    {
        if (gameManager) gameManager.RestartScene();
        Destroy(gameObject);
    }

    public void SetDreamPhase(GameObject nightmatrix)
    {
        if (switchLock) return;
        StartCoroutine(SwitchLockTimer());

        dreamPhase.transform.position = nightmarePhase.transform.position;
        if (followCamera) followCamera.Follow = dreamPhase.transform;

        nightmarePhase.gameObject.SetActive(false);
        dreamPhase.gameObject.SetActive(true);

        dreamPhase.SwitchIn();
    }

    public void SetNightmarePhase(GameObject nightmatrix)
    {
        if (switchLock) return;
        StartCoroutine(SwitchLockTimer());

        nightmarePhase.transform.position = dreamPhase.transform.position;
        if (followCamera) followCamera.Follow = nightmarePhase.transform;

        dreamPhase.gameObject.SetActive(false);
        nightmarePhase.gameObject.SetActive(true);

        nightmarePhase.SwitchIn(nightmatrix.transform.position);
    }

    private IEnumerator SwitchLockTimer()
    {
        switchLock = true;
        yield return new WaitForSeconds(.1f);
        switchLock = false;
    }
}
