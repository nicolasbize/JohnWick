using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] PlayerController player;
    [SerializeField] int viewDistance;
    [SerializeField] float cameraSpeed;

    private Vector3 positionBeforeSwap = Vector3.zero;
    private float lastDirectionChangeTime = float.NegativeInfinity;
    private bool isChangingDirection = false;

    private void Start() {
        player.OnDirectionChange += Player_OnDirectionChange;
    }

    private void Player_OnDirectionChange(object sender, System.EventArgs e) {
        lastDirectionChangeTime = Time.timeSinceLevelLoad;
        positionBeforeSwap = transform.position;
        isChangingDirection = true;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(Mathf.FloorToInt(player.transform.position.x), 0f, -10f);
        targetPosition += (player.IsFacingLeft ? Vector3.left : Vector3.right) * viewDistance;
        if (isChangingDirection) {
            float timeSinceSwap = Time.timeSinceLevelLoad - lastDirectionChangeTime;
            float progress = Mathf.Min(timeSinceSwap / cameraSpeed);
            if (progress >= 1) {
                isChangingDirection = false;
            }
            float lerpedPosX = Mathf.Lerp(positionBeforeSwap.x, targetPosition.x, progress);
            transform.position = new Vector3(lerpedPosX, 0f, -10f);
        } else {
            //transform.position = new Vector3(Mathf.FloorToInt(targetPosition.x), 0f, -10f);
            transform.position = targetPosition;
        }
        //transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
        
    }

    public Vector2 GetScreenXBoundaries() {
        return new Vector2(transform.position.x - 32, transform.position.x + 32);
    }
}
