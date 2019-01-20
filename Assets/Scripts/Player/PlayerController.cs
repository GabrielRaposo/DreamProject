using UnityEngine;

[RequireComponent(typeof(PlayerGroundMovement))]
public class PlayerController : MonoBehaviour {

    private IPlayerMovement currentMovement;

    void OnEnable ()
    {
        currentMovement = GetComponent<PlayerGroundMovement>();
    }

	void Update ()
    {
        if (currentMovement.Stunned()) return;

        currentMovement.SetHorizontalInput(Input.GetAxisRaw("Horizontal"));
        currentMovement.SetVerticalInput(Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            currentMovement.SetJumpInput(true);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            currentMovement.SetJumpInput(false);
        }

        if (Input.GetButtonDown("Attack"))
        {
            currentMovement.SetAttackInput();
        }
    }
}
