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
        timeSinceStartShake = Time.realtimeSinceStartup;
        shakeDuration = duration;
        shakeIntensity = intensity;
        isShaking = true;
    }

    private void Update() {
        if (isShaking) {
            if (Time.realtimeSinceStartup - timeSinceStartShake < shakeDuration) {
                rect.anchoredPosition = realPosition + new Vector3(Random.Range(0, shakeIntensity + 1), Random.Range(0, shakeIntensity + 1), 0);
            } else {
                isShaking = false;
                rect.anchoredPosition = realPosition;
            }
        }
    }

}
