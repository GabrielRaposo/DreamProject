using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoardablePlatform : MonoBehaviour, IPlatformEvent
{
    [SerializeField] private UnityEvent onBoardEvent;

    private void Start()
    {
        if(onBoardEvent == null)
        {
            onBoardEvent = new UnityEvent();
        }
    }

    public void OnLandEvent()
    {
        onBoardEvent.Invoke();
    }

    public void OnLeaveEvent()
    {
    }
}
