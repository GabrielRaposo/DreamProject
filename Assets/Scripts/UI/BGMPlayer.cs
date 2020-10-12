using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    private float baseVolume;
    private float lowVolume;
    
    private float deltaVolume;
    private float targetVolume = -1f;

    private AudioSource audioSource;    

    public static BGMPlayer instance;

    private void Awake() 
    {
        if(instance == null)
        {
            instance = this;
            
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start() 
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();

        baseVolume = audioSource.volume;
        lowVolume = baseVolume / 4;
    }

    public void LowerVolume()
    {
        targetVolume = lowVolume;
        deltaVolume = ((targetVolume - audioSource.volume) > 0 ? 1 : -1) * .02f; 
    }

    public void RiseVolume()
    {
        targetVolume = baseVolume;
        deltaVolume = ((targetVolume - audioSource.volume) > 0 ? 1 : -1) * .02f; 
    }

    private void Update() 
    {
        if (targetVolume != -1f)
        {
            if(Mathf.Abs(targetVolume - audioSource.volume) < .03f)
            {
                audioSource.volume = targetVolume;
                targetVolume = -1f;
                return;
            }
            audioSource.volume += deltaVolume;
        }
    }

}
