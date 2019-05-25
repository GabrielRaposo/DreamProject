using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] 
public class ChangeComponentOnEditor : MonoBehaviour
{

    void Awake()
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>(); 
        if (audioSources != null)
        {
            foreach(AudioSource audioSource in audioSources)
            {
                Debug.Log("audioSource.name = " + audioSource.name);
                audioSource.spatialBlend = 1;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = 15;
            }
        }
    }

}
