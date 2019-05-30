using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialCameraRegion : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float ortographicSize = 5.5f;

    private CameraPriorityManager cameraPriorityManager;  
    
    bool playerIn;

    private void Start() 
    {
        GetComponent<SpriteRenderer>().enabled = false;
        cameraPriorityManager = CameraPriorityManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(playerIn) return;

        if(collision.CompareTag("Player"))
        {
            playerIn = true;
            cameraPriorityManager.SetSpecialFocus(targetTransform, ortographicSize);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(!playerIn) return;    

        if(collision.CompareTag("Player"))
        {
            playerIn = false;
            cameraPriorityManager.ReturnToPreviousFocus();
        }
    }
}
