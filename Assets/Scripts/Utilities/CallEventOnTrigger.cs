using UnityEngine;

public class CallEventOnTrigger : MonoBehaviour
{
    public virtual void CallEvent(EventSwitch eventSwitch)
    {
        Debug.Log("Event not overrided.");
    }
}
