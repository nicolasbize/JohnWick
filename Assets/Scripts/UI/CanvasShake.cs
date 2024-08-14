using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CanvasShake : MonoBehaviour
{

    private Vector3 realPosition = Vector3.zero; // might be different than shown position due to screenshake
    private bool isShaking = false;
    private float shakeDuration = 0f;
    private int shakeIntensity = 1;
    private float timeSinceStartShake = float.NegativeInfinity;
    private RectTransform rect;

    private void Start() {
        rect = GetComponent<RectTransform>();
        realPosition = rect.anchoredPosition;
    }

    public void Shake(float duration, int intensity) {
        timeSinceStartShake = Time.timeSinceLevelLoad;
        shakeDuration = duration;
        shakeIntensity = intensity;
        isShaking = true;
    }

    private void LateUpdate() {
        if (isShaking) {
            if (Time.timeSinceLevelLoad - timeSinceStartShake < shakeDuration) {
                rect.anchoredPosition = realPosition + new Vector3(Random.Range(0, shakeIntensity + 1), Random.Range(0, shakeIntensity + 1), 0);
            } else {
                isShaking = false;
                rect.anchoredPosition = realPosition;
            }
        }
    }

}
