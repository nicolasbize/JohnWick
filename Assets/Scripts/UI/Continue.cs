using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Continue : MonoBehaviour
{
    public event EventHandler OnGameOver;
    public event EventHandler OnContinue;

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
        isRunning = true;
        RefreshCounter();
    }

    private void RefreshCounter() {
        Transform number = Instantiate(numbers[currentIndex]);
        number.SetParent(numberContainer);
        number.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -2, 0);
        timeSinceLastCount = Time.timeSinceLevelLoad;
    }

    void Update()
    {
        if (isRunning && (Time.timeSinceLevelLoad - timeSinceLastCount > 1)) {
            currentIndex -= 1;
            RefreshCounter();
            if (currentIndex <= 0) {
                // game over
                isRunning = false;
                OnGameOver?.Invoke(this, EventArgs.Empty);
            } else {
                timeSinceLastCount = Time.timeSinceLevelLoad;
            }
        }

        if (isRunning && currentIndex < 9 && IsActionButtonPressed()) {
            OnContinue?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool IsActionButtonPressed() {
        return Input.GetButtonDown("Attack") || Input.GetButtonDown("Jump");
        //return Input.GetButtonDown("Attack") || Input.GetButtonDown("Block") || Input.GetButtonDown("Jump"); // commented because blocking is out
    }
}
