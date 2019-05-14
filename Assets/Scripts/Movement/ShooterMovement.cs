using UnityEngine;

public class ShooterMovement : MonoBehaviour
{
    protected Rigidbody2D m_rigidbody;
    protected Nightmatrix nightmatrix;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    public virtual void Call(Nightmatrix nightmatrix) { }

    private void OnDisable() 
    {
        if (nightmatrix != null)
        {
            nightmatrix = null;
        }
    }

    public virtual void NotifyGround() { }
}
