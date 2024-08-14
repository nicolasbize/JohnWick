using System;
using UnityEngine;
using UnityEngine.UI;

public class Checkbox : MonoBehaviour, IActivable
{
    public event EventHandler OnValueChange;

    [SerializeField] private Transform checkboxOn;
    [SerializeField] private Transform checkboxOff;
    [field: SerializeField] public bool IsSelected { get; private set; }

    private bool isActivated = false;

    private void Start() {
        RefreshCheckbox();
    }

    private void Update() {
        if (isActivated) {
            if (MainMenu.Instance.IsSelectionMade()) {
                IsSelected = !IsSelected;
                RefreshCheckbox();
                OnValueChange?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void RefreshCheckbox() {
        checkboxOn.GetComponent<RawImage>().color = isActivated ? MainMenu.Instance.SelectedColor : MainMenu.Instance.UnselectedColor;
        checkboxOff.GetComponent<RawImage>().color = isActivated ? MainMenu.Instance.SelectedColor : MainMenu.Instance.UnselectedColor;
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
