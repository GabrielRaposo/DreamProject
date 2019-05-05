using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeOutEffect : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 1;
    
    private SpriteRenderer spriteRenderer;    
    private Color originalColor;
    private bool fade;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        
        Color color = spriteRenderer.color;
        color.a = 0;
        spriteRenderer.color = color;
    }

    public void FadeOut()
    {
        originalColor.a = 1;
        spriteRenderer.color = originalColor;
        fade = true;
    }

    private void Update() 
    {
        if (fade) 
        {
            Color color = spriteRenderer.color;
            color.a -= fadeSpeed/255f;
            if(color.a < 1f/255f)
            {
                color.a = 0f;
                fade = false;
                enabled = false;
            }
            spriteRenderer.color = color;
        }   
    }
}
