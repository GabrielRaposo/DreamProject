using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerLeaper : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] float jumpSpeed = 6;
    [SerializeField] float horizontalSpeed;
    [SerializeField] float numberOfLeaps;
    [SerializeField] bool aimAtPlayerOnStart;

    private bool facingRight;
    private bool onGround;
    private int leapCounter;

    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;
    private Animator animator;

    private void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
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
    }

    void Update()
    {
        CheckGround();

        if(!onGround)
        {
            animator.SetFloat("VerticalVelocity", rigidbody.velocity.y);
        }
    }

    private void CheckGround()
    {
        Vector2 axis = transform.position + (Vector3.down * .55f * transform.localScale.x);
        Vector2 border = new Vector2(.1f, .1f) * transform.localScale.x;

        bool previous = onGround;
        onGround = Physics2D.OverlapArea(axis - border, axis + border, groundLayer);
        if(onGround && previous != onGround && rigidbody.velocity.y < 0)
        {
            Land();
            if(numberOfLeaps > 0)
            {
                leapCounter++;
                if(leapCounter > numberOfLeaps)
                {
                    leapCounter = 0;
                    StartCoroutine(ChangeFacingSide());
                }
                else StartCoroutine(LeapAction());
            }
            else StartCoroutine(LeapAction());
        }
    }

    private void Land()
    {
        animator.SetTrigger("Land");
        rigidbody.velocity = Vector2.zero;
    }

    private IEnumerator LeapAction() 
    {  
        yield return new WaitForSeconds(1);
        
        animator.SetTrigger("Jump");
        rigidbody.velocity += Vector2.up * jumpSpeed + Vector2.right * horizontalSpeed * (facingRight ? 1 : -1);
        animator.SetFloat("VerticalVelocity", rigidbody.velocity.y);
    }

    private IEnumerator ChangeFacingSide() 
    {
        yield return new WaitForSeconds(1);

        animator.SetTrigger("Jump");
        rigidbody.velocity += Vector2.up * 2;
        animator.SetFloat("VerticalVelocity", rigidbody.velocity.y);

        yield return new WaitUntil(() => rigidbody.velocity.y < 0);

        SetFacingSide(!facingRight);
    }

    public void SetFacingSide(bool lookingRight)
    {
        this.facingRight = lookingRight;
        if (renderer)
        {
            renderer.flipX = lookingRight;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEvent(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionEvent(collision);
    }

    void CollisionEvent(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            return;
        }

        if (collision.contactCount > 0)
        {
            foreach (ContactPoint2D cp in collision.contacts)
            {
                Vector2 point = cp.point - (Vector2)transform.position;
                if (point.y > -.4f)
                {
                    SetFacingSide(point.x < 0 ? true : false);
                    break;
                }
            }
        }
    }
}
