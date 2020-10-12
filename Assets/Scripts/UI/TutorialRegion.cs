using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRegion : MonoBehaviour
{
    [SerializeField] private string text;
    [SerializeField] private TutorialTab tutorialTab;

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            if(tutorialTab) tutorialTab.Show(text);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(collision.CompareTag("Player"))
        {
            if(tutorialTab) tutorialTab.Hide();
        }
    }
}
