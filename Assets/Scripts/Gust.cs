using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gust : MonoBehaviour
{
    private float moveSpeed;
    private float decreaseValue;

    private new Rigidbody2D rigidbody2D;
    private new SpriteRenderer renderer;

    public void Launch (float moveSpeed)
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();

        this.moveSpeed = moveSpeed;
        renderer.flipX = (moveSpeed < 0);
        decreaseValue = moveSpeed / 15;
    }
	
	void Update ()
    {
		if(Mathf.Abs(moveSpeed) > Mathf.Abs(decreaseValue))
        {
            moveSpeed -= decreaseValue;
            rigidbody2D.velocity = Vector2.right * moveSpeed;
        }
        else
        {
            gameObject.SetActive(false);
        }
	}
}
