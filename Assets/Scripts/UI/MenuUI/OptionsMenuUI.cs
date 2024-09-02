using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenuUI : MonoBehaviour {

    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private RangePicker musicRangePicker;
    [SerializeField] private RangePicker soundRangePicker;
    [SerializeField] private Checkbox shakeCheckbox;
    [SerializeField] private Checkbox touchControlCheckbox;
    [SerializeField] private AudioMixerGroup musicMixer;
    [SerializeField] private AudioMixerGroup soundMixer;
    [SerializeField] private Clickable returnLabel;

    private FadingController fader;
    private BaseMenuScreen menu;
    private int currentSelectionIndex = 0;
    private List<IActivable> controls;
    private CanvasShake canvasShake;
    private float timeSinceEnabled = float.NegativeInfinity;

    private void Awake() {
        controls = new List<IActivable>() {
            musicRangePicker, soundRangePicker, shakeCheckbox, touchControlCheckbox
        };
        fader = GetComponent<FadingController>();
        menu = GetComponent<BaseMenuScreen>();
        canvasShake = GetComponent<CanvasShake>();

        returnLabel.OnClick += OnReadyToExit;
        fader.OnCompleteFade += OnReadyToDismiss;
        PlayerInputListener.Instance.OnUpPress += OnUpPress;
        PlayerInputListener.Instance.OnDownPress += OnDownPress;
        PlayerInputListener.Instance.OnSelectPress += OnSelectPress;
        PlayerInputListener.Instance.OnCancelPress += OnReadyToDismiss;
        musicRangePicker.OnValueChange += OnMusicVolumeChange;
        soundRangePicker.OnValueChange += OnSoundVolumeValueChange;
        shakeCheckbox.OnValueChange += OnShakeValueChange;
        touchControlCheckbox.OnValueChange += OnTouchControlValueChange;
        foreach (var control in controls) {
            control.OnRequestActivation += OnControlActivationRequest;
        }
    }

    private void OnDestroy() {
        returnLabel.OnClick -= OnReadyToExit;
        fader.OnCompleteFade -= OnReadyToDismiss;
        PlayerInputListener.Instance.OnUpPress -= OnUpPress;
        PlayerInputListener.Instance.OnDownPress -= OnDownPress;
        PlayerInputListener.Instance.OnSelectPress -= OnSelectPress;
        PlayerInputListener.Instance.OnCancelPress -= OnReadyToDismiss;
        musicRangePicker.OnValueChange -= OnMusicVolumeChange;
        soundRangePicker.OnValueChange -= OnSoundVolumeValueChange;
        shakeCheckbox.OnValueChange -= OnShakeValueChange;
        touchControlCheckbox.OnValueChange -= OnTouchControlValueChange;
        foreach (var control in controls) {
            control.OnRequestActivation -= OnControlActivationRequest;
        }
    }

    private void OnControlActivationRequest(object sender, EventArgs e) {
        for (int i=0; i<controls.Count; i++) {
            if (sender == controls[i]) {
                currentSelectionIndex = i;
            }
        }
        RefreshOptions();
    }

    private void OnTouchControlValueChange(object sender, EventArgs e) {
        SoundManager.Instance.PlayMenuSelect();
        GameState.IsUsingTouchControls = touchControlCheckbox.IsSelected;
    }

    private void Start() {
        if (!PlayerPrefs.HasKey(PrefsHelper.CAMERA_SHAKE)) {
            PlayerPrefs.SetInt(PrefsHelper.CAMERA_SHAKE, 1);
        }
        RefreshOptions();
        if (!fader.enabled) {
            fader.DisplayContent();
        }
    }

    private void OnEnable() {
        timeSinceEnabled = Time.realtimeSinceStartup;
        currentSelectionIndex = 0;
        RefreshOptions();
    }

    private void OnReadyToDismiss(object sender, EventArgs e) {
        menu.CloseScreen(); // let the parent controller decide what to show next, it might be game or main menu
    }

    private void OnReadyToExit(object sender, EventArgs e) {
        ExitScreen();
    }

    private void OnShakeValueChange(object sender, EventArgs e) {
        SoundManager.Instance.PlayMenuSelect();

        if (shakeCheckbox.IsSelected) {
            canvasShake.Shake(0.1f, 2);
        }
        PlayerPrefs.SetInt(PrefsHelper.CAMERA_SHAKE, shakeCheckbox.IsSelected ? 1 : 0);
        if (Camera.main.GetComponent<CameraFollow>() != null) {
            Camera.main.GetComponent<CameraFollow>().IsCameraShakeEnabled = shakeCheckbox.IsSelected;
        }
    }

    private void OnSoundVolumeValueChange(object sender, EventArgs e) {
        float percentValue = soundRangePicker.Value * 8f / 100f;
        if (percentValue == 0) {
            soundMixer.audioMixer.SetFloat("Volume-Master", -80f);
        } else {
            soundMixer.audioMixer.SetFloat("Volume-Master", 20.0f * Mathf.Log10(percentValue));
        }
        PlayerPrefs.SetInt(PrefsHelper.SFX_VOLUME, soundRangePicker.Value);
        SoundManager.Instance.PlayMenuMove();
    }

    private void OnMusicVolumeChange(object sender, EventArgs e) {
        float percentValue = musicRangePicker.Value * 8f / 100f;
        if (percentValue == 0) {
            musicMixer.audioMixer.SetFloat("Volume-Music", -80f);
        } else {
            musicMixer.audioMixer.SetFloat("Volume-Music", 20.0f * Mathf.Log10(percentValue));
        }
        PlayerPrefs.SetInt(PrefsHelper.MUSIC_VOLUME, musicRangePicker.Value);
        SoundManager.Instance.PlayMenuMove();
    }

    private void OnUpPress(object sender, EventArgs e) {
        currentSelectionIndex -= 1;
        if (currentSelectionIndex < 0) {
            currentSelectionIndex = menuOptions.Count - 1;
        }
        SoundManager.Instance.PlayMenuMove();
        RefreshOptions();
    }

    private void OnDownPress(object sender, EventArgs e) {
        currentSelectionIndex += 1;
        if (currentSelectionIndex > menuOptions.Count - 1) {
            currentSelectionIndex = 0;
        }
        SoundManager.Instance.PlayMenuMove();
        RefreshOptions();
    }

    private void OnSelectPress(object sender, EventArgs e) {
        if (Time.realtimeSinceStartup - timeSinceEnabled > 0.3f &&
            currentSelectionIndex == menuOptions.Count - 1) {
            ExitScreen();
        }
    }

    private void ExitScreen() {
        currentSelectionIndex = 0;
        SoundManager.Instance.PlayMenuSelect();

        if (fader.enabled) {
            fader.StartFadingOut();
        } else {
            menu.CloseScreen();
        }
    }

    public void RefreshOptions() {
        musicRangePicker.SetValue(PlayerPrefs.GetInt(PrefsHelper.MUSIC_VOLUME, 4));
        soundRangePicker.SetValue(PlayerPrefs.GetInt(PrefsHelper.SFX_VOLUME, 4));
        shakeCheckbox.SetValue(PlayerPrefs.GetInt(PrefsHelper.CAMERA_SHAKE, 1) == 1);
        touchControlCheckbox.SetValue(GameState.IsUsingTouchControls);
        for (int i=0; i<menuOptions.Count; i++) {
            if (currentSelectionIndex == i) {
                menuOptions[i].color = ColorHelper.SelectedColor;
            } else {
                menuOptions[i].color = ColorHelper.UnselectedColor;
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
