using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Checkbox : BaseMenuScreen, IActivable
{
    public event EventHandler OnValueChange;

    [SerializeField] private Transform checkboxOn;
    [SerializeField] private Transform checkboxOff;
    [field: SerializeField] public bool IsSelected { get; private set; }

    private MenuKeyboardController keyboard;
    private bool isActivated = false;


    private void Awake() {
        keyboard = GetComponent<MenuKeyboardController>();
        keyboard.OnEnterKeyPress += OnEnterKeyPress;
    }

    private void OnEnterKeyPress(object sender, EventArgs e) {
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
