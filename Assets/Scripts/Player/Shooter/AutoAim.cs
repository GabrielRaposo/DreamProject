using System.Collections;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    private ICanTarget shooter;
    
    public void Init(ICanTarget shooter) 
    {
        this.shooter = shooter;
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(shooter != null)
            {
                shooter.SetTarget(collision.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(shooter != null)
            {
                shooter.RemoveTarget(collision.transform);
            }
        }
    }
}
