using UnityEngine;

public class EventSwitch : MonoBehaviour
{
    [SerializeField] protected CallEventOnTrigger[] eventsOnTrigger;

    protected int lockCount;

    public virtual void Lock() { }
    public virtual void Unlock() { }
}
