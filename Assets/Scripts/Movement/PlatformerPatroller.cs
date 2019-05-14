using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerPatroller : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] bool animateWalk;
    [SerializeField] bool aimAtPlayerOnStart;

    private bool facingRight;
    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;

    private void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (aimAtPlayerOnStart)
        {
            PlayerPhaseManager player = PlayerPhaseManager.instance;
            if (player)
            {
                SetFacingSide((transform.position.x < player.transform.position.x) ? true : false);
            }
        }

        if (animateWalk)
        {
            Animator _animator = GetComponent<Animator>();
            if(_animator) _animator.SetBool("Walk", true);
        }
    }

    void Update()
    {
        Vector2 velocity = rigidbody.velocity;
        velocity.x = moveSpeed * (facingRight ? 1 : -1);
        rigidbody.velocity = velocity;
    }

    public void SetFacingSide(bool lookingRight)
    {
        this.facingRight = lookingRight;
        if (renderer)
        {
            renderer.flipX = lookingRight;
        }
    }
}
