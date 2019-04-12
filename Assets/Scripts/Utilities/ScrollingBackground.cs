using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private float backgroundSize;
    [SerializeField] private float parallaxSpeed;

    private Transform cameraTransform;
    private Transform[] layers;
    private float viewZone = 10;

    private int leftIndex;
    private int rightIndex;

    private float lastCameraX;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraX = cameraTransform.position.x;

        layers = new Transform[transform.childCount];
        for(int i = 0; i < layers.Length; i++)
        {
            layers[i] = transform.GetChild(i);
        }

        leftIndex = 0;
        rightIndex = layers.Length - 1;
    }

    void Update()
    {
        float deltaX = cameraTransform.position.x - lastCameraX;
        transform.position += Vector3.right * (deltaX * parallaxSpeed);
        lastCameraX = cameraTransform.position.x;

        if (cameraTransform.position.x < layers[leftIndex].transform.position.x + viewZone)
        {
            ScrollLeft();
        }
        if (cameraTransform.position.x > layers[rightIndex].transform.position.x - viewZone)
        {
            ScrollRight();
        }
    }

    private void ScrollLeft()
    {
        int lastRight = rightIndex;
        layers[rightIndex].position = Vector3.right * (layers[leftIndex].position.x - backgroundSize);
        leftIndex = rightIndex;
        rightIndex--;
        if(rightIndex < 0) rightIndex = layers.Length - 1;
    }

    private void ScrollRight()
    {
        int lastLeft = leftIndex;
        layers[leftIndex].position = Vector3.right * (layers[rightIndex].position.x + backgroundSize);
        rightIndex = leftIndex;
        leftIndex = (leftIndex + 1) % layers.Length;
    }
}
