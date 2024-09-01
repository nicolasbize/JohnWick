using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Checkbox : BaseMenuScreen, IActivable {
    public event EventHandler OnValueChange;
    public event EventHandler OnRequestActivation;

    [SerializeField] private Transform checkboxOn;
    [SerializeField] private Transform checkboxOff;
    [field: SerializeField] public bool IsSelected { get; private set; }

    private bool isActivated = false;

    private void Awake() {
        GetComponent<Clickable>().OnClick += OnClick;
        PlayerInputListener.Instance.OnSelectPress += OnSelectPress;
    }

    private void OnDestroy() {
        PlayerInputListener.Instance.OnSelectPress -= OnSelectPress;
    }

    private void OnClick(object sender, EventArgs e) {
        OnRequestActivation?.Invoke(this, EventArgs.Empty);
        ToggleSelectedValue();
    }

    private void OnSelectPress(object sender, EventArgs e) {
        ToggleSelectedValue();
    }

    public void ToggleSelectedValue() {
        if (isActivated) {
            IsSelected = !IsSelected;
            RefreshCheckbox();
            OnValueChange?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Start() {
        RefreshCheckbox();
    }

    public void SetValue(bool selected) {
        IsSelected = selected;
        RefreshCheckbox();
    }

    private void RefreshCheckbox() {
        checkboxOn.GetComponent<RawImage>().color = isActivated ? ColorHelper.SelectedColor : ColorHelper.UnselectedColor;
        checkboxOff.GetComponent<RawImage>().color = isActivated ? ColorHelper.SelectedColor : ColorHelper.UnselectedColor;
        checkboxOn.gameObject.SetActive(IsSelected);
        checkboxOff.gameObject.SetActive(!IsSelected);
    }

    public void Activate() {
        isActivated = true;
        RefreshCheckbox();
    }

    public void Deactivate() {
        isActivated = false;
        RefreshCheckbox();
    }

}
