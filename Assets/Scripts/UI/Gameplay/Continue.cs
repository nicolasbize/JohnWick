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

    private void Awake() {
        PlayerInputListener.Instance.OnSelectPress += OnSelectPress;
    }

    private void OnDestroy() {
        PlayerInputListener.Instance.OnSelectPress -= OnSelectPress;
    }

    private void OnSelectPress(object sender, EventArgs e) {
        TryContinue();
    }

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

    private void Update()
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
    }

    private void TryContinue() {
        if (isRunning && currentIndex < 9) {
            OnContinue?.Invoke(this, EventArgs.Empty);
        }
    }

}
