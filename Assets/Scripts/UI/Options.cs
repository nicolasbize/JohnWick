using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class Options : MonoBehaviour
{
    public event EventHandler OnDismiss;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private RangePicker musicRangePicker;
    [SerializeField] private RangePicker soundRangePicker;
    [SerializeField] private Checkbox shakeCheckbox;
    [SerializeField] private AudioMixerGroup musicMixer;
    [SerializeField] private AudioMixerGroup soundMixer;

    private int currentSelectionIndex = 0;
    private bool isVerticalMovementDetected = false;
    private List<IActivable> controls;
    private CanvasShake canvasShake;

    private void Awake() {
        musicRangePicker.OnValueChange += OnMusicVolumeChange;
        soundRangePicker.OnValueChange += OnSoundVolumeValueChange;
        shakeCheckbox.OnValueChange += OnShakeValueChange;
        controls = new List<IActivable>() {
            musicRangePicker, soundRangePicker, shakeCheckbox
        };
        canvasShake = GetComponent<CanvasShake>();
    }

    private void OnShakeValueChange(object sender, EventArgs e) {
        MainMenu.Instance.PlayMenuSelectSound();

        if (shakeCheckbox.IsSelected) {
            canvasShake.Shake(0.1f, 2);
        }
    }

    private void OnSoundVolumeValueChange(object sender, EventArgs e) {
        float percentValue = soundRangePicker.Value * 8f / 100f;
        if (percentValue == 0) {
            soundMixer.audioMixer.SetFloat("Volume-SFX", -80f);
        } else {
            soundMixer.audioMixer.SetFloat("Volume-SFX", 20.0f * Mathf.Log10(percentValue));
        }
        MainMenu.Instance.PlayMenuMovementSound();
    }

    private void OnMusicVolumeChange(object sender, EventArgs e) {
        float percentValue = musicRangePicker.Value * 8f / 100f;
        if (percentValue == 0) {
            musicMixer.audioMixer.SetFloat("Volume-Music", -80f);
        } else {
            musicMixer.audioMixer.SetFloat("Volume-Music", 20.0f * Mathf.Log10(percentValue));
        }
        MainMenu.Instance.PlayMenuMovementSound();
    }

    private void Start() {
        RefreshOptions();
    }

    private void Update() {
        float upDownMovement = Input.GetAxisRaw("Vertical");
        if (!isVerticalMovementDetected && upDownMovement > 0) {
            currentSelectionIndex -= 1;
            isVerticalMovementDetected = true;
            if (currentSelectionIndex < 0) {
                currentSelectionIndex = menuOptions.Count - 1;
            }
            MainMenu.Instance.PlayMenuMovementSound();
            RefreshOptions();
        } else if (!isVerticalMovementDetected && upDownMovement < 0) {
            currentSelectionIndex += 1;
            isVerticalMovementDetected = true;
            if (currentSelectionIndex > menuOptions.Count - 1) {
                currentSelectionIndex = 0;
            }
            MainMenu.Instance.PlayMenuMovementSound();
            RefreshOptions();
        } else if (upDownMovement == 0) {
            isVerticalMovementDetected = false;
        }

        if ((currentSelectionIndex == menuOptions.Count - 1) && MainMenu.Instance.IsSelectionMade()) {
            currentSelectionIndex = 0;
            MainMenu.Instance.PlayMenuSelectSound();
            OnDismiss?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RefreshOptions() {
        for (int i=0; i<menuOptions.Count; i++) {
            if (currentSelectionIndex == i) {
                menuOptions[i].color = MainMenu.Instance.SelectedColor;
            } else {
                menuOptions[i].color = MainMenu.Instance.UnselectedColor;
            }
        }

        for (int i = 0; i < controls.Count; i++) {
            if (currentSelectionIndex == i) {
                controls[i].Activate();
            } else {
                controls[i].Deactivate();
            }
        }
    }
}
