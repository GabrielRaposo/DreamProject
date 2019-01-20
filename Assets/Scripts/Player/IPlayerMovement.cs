using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMovement
{
    bool Stunned();
    void SetHorizontalInput(float horizontalInput);
    void SetVerticalInput(float horizontalInput);
    void SetJumpInput(bool super);
    void SetAttackInput();
}
