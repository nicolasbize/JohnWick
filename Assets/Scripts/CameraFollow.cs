using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] PlayerController player;
    [SerializeField] int viewDistance;
    [SerializeField] float cameraSpeed;
    [SerializeField] bool isPixelPerfect;

    void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(player.transform.position.x, 0f, -10f);
        targetPosition += (player.IsFacingLeft() ? Vector3.left : Vector3.right) * viewDistance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
        if (isPixelPerfect) {
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), transform.position.y, transform.position.z);

        }
    }

    public Vector2 GetScreenXBoundaries() {
        return new Vector2(transform.position.x - 32, transform.position.x + 32);
    }
}
