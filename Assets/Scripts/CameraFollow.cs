using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public event EventHandler OnPositionChange;

    [SerializeField] private PlayerController player;
    [SerializeField] private int viewDistance;
    [SerializeField] private float cameraSpeed;

    private Vector3 positionBeforeUnlock = Vector3.zero;
    private float lastLockChangeTime = float.NegativeInfinity;
    private bool wasJustUnlocked = false;
    public bool IsLocked { get; private set; } = true;

    private void Start() {
        Unlock();
    }

    public void LockInPlace() {
        IsLocked = true;
    }

    public void Unlock() {
        IsLocked = false;
        lastLockChangeTime = Time.timeSinceLevelLoad;
        positionBeforeUnlock = transform.position;
        wasJustUnlocked = true;
    }

    private void LateUpdate()
    {
        if (!IsLocked) {
            Vector3 originalPosition = transform.position;
            Vector3 targetPosition = new Vector3(Mathf.FloorToInt(player.transform.position.x + viewDistance), originalPosition.y, originalPosition.z);
            if (targetPosition.x >= originalPosition.x) {  // cannot go backwards
                if (wasJustUnlocked) {
                    float timeSinceSwap = Time.timeSinceLevelLoad - lastLockChangeTime;
                    float progress = Mathf.Min(timeSinceSwap / cameraSpeed);
                    if (progress >= 1) {
                        wasJustUnlocked = false;
                    }
                    float lerpedPosX = Mathf.Lerp(positionBeforeUnlock.x, targetPosition.x, progress);
                    transform.position = new Vector3(lerpedPosX, transform.position.y, transform.position.z);
                } else {
                    transform.position = targetPosition;
                }
                if (transform.position != originalPosition) {
                    OnPositionChange?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public Vector2 GetScreenXBoundaries() {
        return new Vector2(transform.position.x - 32, transform.position.x + 32);
    }
}
