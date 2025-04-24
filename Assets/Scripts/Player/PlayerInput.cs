using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    public Vector2 movementInput;
    public bool jumpInput;
    public bool actionInput;
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        actionInput = context.ReadValueAsButton();
    }
}
