using System;
using UnityEngine;

public class MenuKeyboardController : MonoBehaviour
{
    public event EventHandler OnUpKeyPress;
    public event EventHandler OnDownKeyPress;
    public event EventHandler OnRightKeyPress;
    public event EventHandler OnLeftKeyPress;
    public event EventHandler OnEnterKeyPress;

    private bool isVerticalMovementDetected = false;
    private bool isHorizontalMovementDetected = false;

    private void Update() {
        float verticalMovement = Input.GetAxisRaw(InputHelper.AXIS_VERTICAL);
        if (!isVerticalMovementDetected && verticalMovement > 0) {
            isVerticalMovementDetected = true;
            OnUpKeyPress?.Invoke(this, EventArgs.Empty);
        } else if (!isVerticalMovementDetected && verticalMovement < 0) {
            isVerticalMovementDetected = true;
            OnDownKeyPress?.Invoke(this, EventArgs.Empty);
        } else if (verticalMovement == 0) {
            isVerticalMovementDetected = false;
        }

        float horizontalMovement = Input.GetAxisRaw(InputHelper.AXIS_HORIZONTAL);
        if (!isHorizontalMovementDetected && horizontalMovement > 0) {
            isHorizontalMovementDetected = true;
            OnRightKeyPress?.Invoke(this, EventArgs.Empty);
        } else if (!isHorizontalMovementDetected && horizontalMovement < 0) {
            isHorizontalMovementDetected = true;
            OnLeftKeyPress?.Invoke(this, EventArgs.Empty);
        } else if (horizontalMovement == 0) {
            isHorizontalMovementDetected = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown(InputHelper.BTN_ATTACK)) {
            OnEnterKeyPress?.Invoke(this, EventArgs.Empty);
        }
    }
}
