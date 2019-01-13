using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour {

    private PlayerMovement movement;

    void OnEnable ()
    {
        movement = GetComponent<PlayerMovement>();
    }

	void Update ()
    {
        if (movement.stunned) return;

        movement.SetHorizontalInput(Input.GetAxisRaw("Horizontal"));
        movement.SetVerticalInput(Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            movement.SetJumpInput(true);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            movement.SetJumpInput(false);
        }

        if (Input.GetButtonDown("Attack"))
        {
            movement.SetAttackInput();
        }
    }
}
