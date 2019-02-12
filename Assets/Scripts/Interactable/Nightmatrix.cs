using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nightmatrix : MonoBehaviour
{
    public bool active { get; private set; }

    private List<IObserver> observers = new List<IObserver>();

    public void Activate()
    {
        active = true;

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
