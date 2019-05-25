using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nightmatrix : MonoBehaviour
{
    [SerializeField] private BoxCollider2D borderCollider;
    [SerializeField] private AudioSource transitionSFX;
    
    public bool active { get; private set; }
    public Vector2 size { get; private set; }

    private List<IObserver> observers = new List<IObserver>();

    private void OnEnable() 
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        if(spriteRenderer && boxCollider2D && particleSystem)
        {
            size = boxCollider2D.size = borderCollider.size = new Vector2(spriteRenderer.size.x - 1, spriteRenderer.size.y - 1);

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.scale = size;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = (size.x * size.y) / 3;

            particleSystem.Play();
        }
        else size = Vector2.zero;
    }

    public void Activate()
    {
        active = true;
        transitionSFX.Play();

        foreach (IObserver observer in observers)
        {
            observer.OnNotify();
        }
    }

    public void Deactivate()
    {
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
}
