using UnityEngine;

public class InheritAnchorMovement : MonoBehaviour
{
    private Transform anchor;
    private Vector3 anchorPreviousPosition;

    public void Set(Transform anchor)
    {
        this.anchor = anchor;
        anchorPreviousPosition = anchor.position;
    }

    void Update()
    {
        if(anchor != null)
        {
            Vector3 difference = anchor.position - anchorPreviousPosition;
            transform.position += difference;

            anchorPreviousPosition = anchor.position;
        }
    }

    private void OnDisable() 
    {
        anchor = null;
    }
}
