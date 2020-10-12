using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nightmatrix : MonoBehaviour
{
    [SerializeField] private BoxCollider2D borderCollider;
    [SerializeField] private AudioSource transitionSFX;
    [Header("Direction")]
    [SerializeField] private SpriteRenderer directionArrows;
    [SerializeField] private Color rightColor;
    [SerializeField] private Color leftColor;
    
    public bool active { get; private set; }
    public Vector2 size { get; private set; }
    public bool invertedDirection {get; private set;}

    private PlayerShooter player;
    private List<IObserver> observers = new List<IObserver>();

    private void OnEnable() 
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        if(spriteRenderer && boxCollider2D && particleSystem)
        {
            size = boxCollider2D.size = borderCollider.size = directionArrows.size = new Vector2(spriteRenderer.size.x - 1, spriteRenderer.size.y - 1);
            directionArrows.color = invertedDirection ? leftColor : rightColor;

            invertedDirection = directionArrows.flipX;

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.scale = size;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = (size.x * size.y) / 3;

            particleSystem.Play();
        }
        else size = Vector2.zero;
    }

    public void Activate(PlayerShooter player)
    {
        this.player = player;
        active = true;
        transitionSFX.Play();

        foreach (IObserver observer in observers)
        {
            observer.OnNotify();
        }
    }

    public void Deactivate()
    {
        player = null;
        active = false;

        foreach (IObserver observer in observers)
        {
            observer.OnNotify();
        }
    }

    public void AddObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void InvertDirection()
    {
        directionArrows.flipX = invertedDirection = !directionArrows.flipX;
        directionArrows.color = invertedDirection ? leftColor : rightColor;

        if (player != null) player.UpdateFacingRight(!invertedDirection);
    }
}
