using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour, IPlatformEvent
{
    [SerializeField] private CircleCollider2D superJumpHtbox;
    [SerializeField] private ParticleSystem vanishFX;

    private float yPosition;
    private Coroutine timer;

    private Animator m_animator;

    private void OnEnable()
    {
        m_animator = GetComponent<Animator>();
    }

    private void Start()
    {
        yPosition = transform.position.y;
    }

    public void OnLandEvent()
    {
        if (timer != null) StopCoroutine(timer);
        StartCoroutine(ReactToWeight());
    }

    public void OnLeaveEvent()
    {
        if (timer != null) StopCoroutine(timer);
        timer = StartCoroutine(DeactivationTimer());
    }

    private IEnumerator DeactivationTimer()
    {
        yield return new WaitForSeconds(.1f);

        m_animator.SetInteger("State", 1);
        vanishFX.Play();

        StartCoroutine(RestaurationTimer());
    }

    private IEnumerator RestaurationTimer()
    {
        yield return new WaitForSeconds(3f);
        m_animator.SetInteger("State", 0);
    }

    private IEnumerator ReactToWeight()
    {
        transform.position += Vector3.down * .1f;

        while (transform.position.y < yPosition)
        {
            yield return new WaitForFixedUpdate();
            transform.position += Vector3.up * .01f;
        }
        transform.position = new Vector3(transform.position.x, yPosition);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            if (timer != null) StopCoroutine(timer);
            StartCoroutine(Charge());
        }
    }

    private IEnumerator Charge()
    {
        m_animator.SetTrigger("Charge");

        float targetPosition = yPosition - .5f;
        while (transform.position.y > targetPosition)
        {
            yield return new WaitForFixedUpdate();
            transform.position += Vector3.down * .05f;
        }

        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        m_animator.SetTrigger("Launch");

        float targetPosition = yPosition + .5f;
        while (transform.position.y < targetPosition)
        {
            yield return new WaitForFixedUpdate();
            transform.position += Vector3.up * .5f;
        }

        superJumpHtbox.enabled = true;
        yield return new WaitForFixedUpdate();
        superJumpHtbox.enabled = false;

        OnLeaveEvent();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (transform.position.y > collision.transform.position.y)
            {
                OnLeaveEvent();
            }
        }
    }
}
