using UnityEngine;

public interface ICanTarget
{
    void SetTarget(Transform target);
    void RemoveTarget(Transform target);
}
