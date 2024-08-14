using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangePicker : MonoBehaviour, IActivable {

    public event EventHandler OnValueChange;

    [SerializeField] private Transform emptyTickPrefab;
    [SerializeField] private Transform fullTickPrefab;
    [SerializeField] private int maxValue;
    [field: SerializeField] public int Value { get; private set; }

    private bool isActivated = false;
    private bool isHorizontalMovementDetected = false;

    private void Start()
    {
        RefreshPicker();
    }

    private void RefreshPicker() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < Value; i++) {
            Transform fullTick = Instantiate(fullTickPrefab, transform);
            fullTick.GetComponent<RawImage>().color = isActivated ? MainMenu.Instance.SelectedColor : MainMenu.Instance.UnselectedColor;
        }
        for (int i = 0; i < maxValue - Value; i++) {
            Transform emptyTick = Instantiate(emptyTickPrefab, transform);
            emptyTick.GetComponent<RawImage>().color = isActivated ? MainMenu.Instance.SelectedColor : MainMenu.Instance.UnselectedColor;
        }
    }

    public void Activate() {
        isActivated = true;
        RefreshPicker();
    }

    public void Deactivate() {
        isActivated = false;
        RefreshPicker();
    }


    private void Update()
    {
        if (isActivated) {
            float leftRightMovement = Input.GetAxisRaw("Horizontal");
            if (!isHorizontalMovementDetected && leftRightMovement < 0) {
                int prevValue = Value;
                Value -= 1;
                isHorizontalMovementDetected = true;
                if (Value < 0) {
                    Value = 0;
                }
                RefreshPicker();
                if (prevValue != Value) {
                    OnValueChange?.Invoke(this, EventArgs.Empty);
                }
            } else if (!isHorizontalMovementDetected && leftRightMovement > 0) {
                int prevValue = Value;
                Value += 1;
                isHorizontalMovementDetected = true;
                if (Value > maxValue) {
                    Value = maxValue;
                }
                RefreshPicker();
                if (prevValue != Value) {
                    OnValueChange?.Invoke(this, EventArgs.Empty);
                }
            } else if (leftRightMovement == 0) {
                isHorizontalMovementDetected = false;
            }
        }
    }

}