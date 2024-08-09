using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public event EventHandler OnPositionChange;

    [SerializeField] private PlayerController player;
    [SerializeField] private int viewDistance;
    [SerializeField] private float cameraSpeed;

    private Vector3 realPosition = Vector3.zero; // might be different than shown position due to screenshake
    private Vector3 positionBeforeUnlock = Vector3.zero;
    private float lastLockChangeTime = float.NegativeInfinity;
    private bool wasJustUnlocked = false;
    public bool IsLocked { get; private set; } = true;

    private bool isShaking = false;
    private float shakeDuration = 0f;
    private int shakeIntensity = 1;
    private float timeSinceStartShake = float.NegativeInfinity;

    private void Start() {
        realPosition = transform.position;
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

    public void Shake(float duration, int intensity) {
        timeSinceStartShake = Time.timeSinceLevelLoad;
        shakeDuration = duration;
        shakeIntensity = intensity;
        isShaking = true;
    }

    private void LateUpdate()
    {
        if (!IsLocked) {
            Vector3 originalPosition = realPosition;
            Vector3 targetPosition = new Vector3(Mathf.FloorToInt(player.transform.position.x + viewDistance), realPosition.y, realPosition.z);
            if (targetPosition.x >= realPosition.x) {  // cannot go backwards
                if (wasJustUnlocked) {
                    float timeSinceSwap = Time.timeSinceLevelLoad - lastLockChangeTime;
                    float progress = Mathf.Min(timeSinceSwap / cameraSpeed);
                    if (progress >= 1) {
                        wasJustUnlocked = false;
                    }
                    float lerpedPosX = Mathf.Lerp(positionBeforeUnlock.x, targetPosition.x, progress);

                    realPosition = new Vector3(lerpedPosX, realPosition.y, realPosition.z);
                } else {
                    realPosition = targetPosition;
                }
                if (realPosition != originalPosition) {
                    OnPositionChange?.Invoke(this, EventArgs.Empty);
                }
            }

            transform.position = realPosition;
        }

        if (isShaking) {
            if (Time.timeSinceLevelLoad - timeSinceStartShake < shakeDuration) {
                transform.position = realPosition + new Vector3(UnityEngine.Random.Range(0, shakeIntensity + 1), UnityEngine.Random.Range(0, shakeIntensity + 1), 0);
            } else {
                isShaking = false;
                transform.position = realPosition;
            }
        }
    }

    public Vector2 GetScreenXBoundaries() {
        return new Vector2(realPosition.x - 32, realPosition.x + 32);
    }
}
