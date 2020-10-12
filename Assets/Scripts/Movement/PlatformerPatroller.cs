using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerPatroller : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] bool animateWalk;
    [SerializeField] bool aimAtPlayerOnStart;
    [SerializeField] bool turnAroundOnPit;

    private bool facingRight;
    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;

    private void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();

        if (animateWalk)
        {
            Animator _animator = GetComponent<Animator>();
            if(_animator) _animator.SetBool("Walk", true);
        }
    }

    void Start()
    {
        if (aimAtPlayerOnStart)
        {
            PlayerPhaseManager player = PlayerPhaseManager.instance;
            if (player)
            {
                SetFacingRight((transform.position.x < player.transform.position.x) ? true : false);
            }
        }
    }

    void Update()
    {
        Vector2 velocity = rigidbody.velocity;
        velocity.x = moveSpeed * (facingRight ? 1 : -1);
        rigidbody.velocity = velocity;

        if(turnAroundOnPit)
        {
            if(CheckPit()) SetFacingRight(!facingRight);
        }
    }

    //private void OnDrawGizmos() 
    //{
    //    if(turnAroundOnPit)
    //    {
    //        Vector3 pos = transform.position + ((facingRight ? Vector3.right : Vector3.left) * .5f);
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(pos, pos + Vector3.down* .75f);
    //    }
    //}

    private bool CheckPit()
    {
        Vector3 pos = transform.position + ((facingRight ? Vector3.right : Vector3.left) * .5f);
        return !Physics2D.Linecast(pos, pos + Vector3.down * .75f, 1 << LayerMask.NameToLayer("Ground"));
    }

    public void SetFacingRight(bool facingRight)
    {
        this.facingRight = facingRight;
        if (renderer)
        {
            renderer.flipX = facingRight;
        }
    }

}
