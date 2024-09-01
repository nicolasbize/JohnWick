using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RangePicker : MonoBehaviour, IActivable {

    public event EventHandler OnValueChange;
    public event EventHandler OnRequestActivation;

    [SerializeField] private Transform emptyTickPrefab;
    [SerializeField] private Transform fullTickPrefab;
    [SerializeField] private int maxValue;
    [field: SerializeField] public int Value { get; private set; }

    private bool isActivated = false;

    private void Awake() {
        PlayerInputListener.Instance.OnLeftPress += OnLeftPress;
        PlayerInputListener.Instance.OnRightPress += OnRightPress;
    }

    private void OnDestroy() {
        PlayerInputListener.Instance.OnLeftPress -= OnLeftPress;
        PlayerInputListener.Instance.OnRightPress -= OnRightPress;
    }

    private void OnRightPress(object sender, EventArgs e) {
        IncreaseValue();
    }

    private void OnLeftPress(object sender, EventArgs e) {
        DecreaseValue();
    }

    private void IncreaseValue() {
        if (isActivated) {
            Value += 1;
            if (Value > maxValue) {
                Value = maxValue;
            }
            RefreshPicker();
            OnValueChange?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DecreaseValue() {
        if (isActivated) {
            Value -= 1;
            if (Value < 0) {
                Value = 0;
            }
            RefreshPicker();
            OnValueChange?.Invoke(this, EventArgs.Empty);
        }
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
            fullTick.GetComponent<Clickable>().OnClick += OnTickClick;
        }
        for (int i = 0; i < maxValue - Value; i++) {
            Transform emptyTick = Instantiate(emptyTickPrefab, transform);
            emptyTick.GetComponent<RawImage>().color = isActivated ? ColorHelper.SelectedColor : ColorHelper.UnselectedColor;
            emptyTick.GetComponent<Clickable>().OnClick += OnTickClick;
        }
    }

    private void OnTickClick(object sender, EventArgs e) {
        OnRequestActivation?.Invoke(this, EventArgs.Empty);
        for (int i=0; i<transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<Clickable>() == (Clickable) sender) {
                SetValue(Mathf.Min(i+1, maxValue - 1));
                return;
            }
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
