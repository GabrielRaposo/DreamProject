using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private Transform cameraTransform;

    private void OnEnable()
    {
        cameraTransform = Camera.main.transform;
        if(cameraTransform == null)
        {
            enabled = false;
        }
    }

    void Update()
    {
        transform.position = (Vector2) cameraTransform.position;
    }
}
