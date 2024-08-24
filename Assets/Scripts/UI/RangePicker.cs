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

    private MenuKeyboardController keyboard;
    private bool isActivated = false;

    private void Awake() {
        keyboard = GetComponent<MenuKeyboardController>();
        keyboard.OnLeftKeyPress += OnLeftKeyPress;
        keyboard.OnRightKeyPress += OnRightKeyPress;
    }

    private void OnRightKeyPress(object sender, EventArgs e) {
        Value += 1;
        if (Value > maxValue) {
            Value = maxValue;
        }
        RefreshPicker();
        OnValueChange?.Invoke(this, EventArgs.Empty);
    }

    private void OnLeftKeyPress(object sender, EventArgs e) {
        Value -= 1;
        if (Value < 0) {
            Value = 0;
        }
        RefreshPicker();
        OnValueChange?.Invoke(this, EventArgs.Empty);
    }

    private void Start() {
        RefreshPicker();
    }

    private void RefreshPicker() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < Value; i++) {
            Transform fullTick = Instantiate(fullTickPrefab, transform);
            fullTick.GetComponent<RawImage>().color = isActivated ? ColorHelper.SelectedColor : ColorHelper.UnselectedColor;
        }
        for (int i = 0; i < maxValue - Value; i++) {
            Transform emptyTick = Instantiate(emptyTickPrefab, transform);
            emptyTick.GetComponent<RawImage>().color = isActivated ? ColorHelper.SelectedColor : ColorHelper.UnselectedColor;
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

    public void SetValue(int value) {
        Value = value;
        RefreshPicker();
    }

}
