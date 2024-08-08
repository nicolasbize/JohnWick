using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Continue : MonoBehaviour
{
    [SerializeField] private List<Transform> numbers;
    [SerializeField] private Transform numberContainer;

    private int currentIndex;
    private float timeSinceLastCount = float.NegativeInfinity;
    private bool isRunning;

    public void StartCountdown() {
        currentIndex = 9;
        foreach (Transform child in numberContainer) {
            Destroy(child.gameObject);
        }
        timeSinceLastCount = Time.timeSinceLevelLoad;
        isRunning = true;
        RefreshCounter();
    }

    private void RefreshCounter() {
        Transform number = Instantiate(numbers[currentIndex]);
        number.SetParent(numberContainer);
    }

    void Update()
    {
        if (isRunning && (Time.timeSinceLevelLoad - timeSinceLastCount > 1)) {
            currentIndex -= 1;
            if (currentIndex < 0) {
                // game over
                isRunning = false;
            } else {
                timeSinceLastCount = Time.timeSinceLevelLoad;
            }
        }
    }
}
