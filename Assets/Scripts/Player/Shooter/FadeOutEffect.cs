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

    public void CallFadeOut(bool facingRight)
    {
        originalColor.a = 1;
        spriteRenderer.color = originalColor;
        spriteRenderer.flipX = !facingRight;
        fade = true;
    }

    private void Update() 
    {
        if(Time.timeScale == 0) return;

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
