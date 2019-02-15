using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChaserMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationTime;

    private Transform target;
    private Rigidbody2D m_rigidbody;

    private void OnEnable()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 posDiff = target.position - transform.position;
            transform.DORotate(new Vector3(0, 0, (Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg) + 180), rotationTime);
        }
        m_rigidbody.velocity = RaposUtil.RotateVector(Vector3.left * speed, transform.rotation.eulerAngles.z);
    }

    private void OnDisable()
    {
        transform.DOKill();
    }
}
