using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour {

    private PlayerMovement movement;
    private bool inputLock;

    void Start ()
    {
        movement = GetComponent<PlayerMovement>();
    }

	void Update ()
    {
        if (movement.stunned) return;

        movement.SetHorizontalMovement(Input.GetAxisRaw("Horizontal"));
        movement.SetVerticalInput(Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            movement.SetJump(true);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            movement.ReleaseJump();
        }

        if (Input.GetButtonDown("Attack"))
        {
            movement.SetAttack();
        }
    }
}
