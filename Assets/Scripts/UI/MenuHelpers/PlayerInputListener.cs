using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputListener : MonoBehaviour
{
    public event EventHandler OnUpPress;
    public event EventHandler OnDownPress;
    public event EventHandler OnRightPress;
    public event EventHandler OnLeftPress;
    public event EventHandler OnSelectPress;
    public event EventHandler OnCancelPress;
    public event EventHandler OnJumpPress;
    public event EventHandler OnAttackPress;

    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private bool isUpPressed = false;
    private bool isDownPressed = false;
    private bool hasResetDPad = true;
    private Vector2 inputVector;

    public static PlayerInputListener Instance;

    private void Awake() {
        Instance = this;
    }

    public void OnCancelButtonPressed(InputAction.CallbackContext context) {
        if (context.performed) {
            OnCancelPress?.Invoke(this, EventArgs.Empty);
        }
    }

    public void OnSelectButtonPressed(InputAction.CallbackContext context) {
        if (context.performed) {
            OnSelectPress?.Invoke(this, EventArgs.Empty);
        }
    }

    public void OnUIMovementPerformed(InputAction.CallbackContext context) {
        inputVector = context.ReadValue<Vector2>();
        if (inputVector.x == -1f && !isLeftPressed) {
            isLeftPressed = true;
        } else if (inputVector.x == 1f && !isRightPressed) {
            isRightPressed = true;
        }

        if (inputVector.y == -1f && !isDownPressed) {
            isDownPressed = true;
        } else if (inputVector.y == 1f && !isUpPressed) {
            isUpPressed = true;
        }
    }

    public Vector2 GetInputVector() { return inputVector; }

    public void OnJumpButtonPressed(InputAction.CallbackContext context) {
        if (context.performed) {
            OnJumpPress?.Invoke(this, EventArgs.Empty);
        }
    }

    public void OnAttackButtonPressed(InputAction.CallbackContext context) {
        if (context.performed) {
            OnAttackPress?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update() {
        if (isLeftPressed && hasResetDPad) {
            OnLeftPress?.Invoke(this, EventArgs.Empty);
            hasResetDPad = false;
        }
        if (isRightPressed && hasResetDPad) {
            OnRightPress?.Invoke(this, EventArgs.Empty);
            hasResetDPad = false;
        }
        if (isUpPressed && hasResetDPad) {
            OnUpPress?.Invoke(this, EventArgs.Empty);
            hasResetDPad = false;
        }
        if (isDownPressed && hasResetDPad) {
            OnDownPress?.Invoke(this, EventArgs.Empty);
            hasResetDPad = false;
        }

        if (inputVector == Vector2.zero) {
            hasResetDPad = true;
            isLeftPressed = false;
            isRightPressed = false;
            isDownPressed = false;
            isUpPressed = false;
        }
    }
}
