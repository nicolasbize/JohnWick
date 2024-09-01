using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static BaseMenuScreen;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private List<TextMeshProUGUI> menuOptions;

    private int currentMenuSelectionIndex = 0;
    private FadingController fader;
    private BaseMenuScreen menu;
    private ScreenType selectedScreen = ScreenType.None;
    private float timeSinceEnabled = float.NegativeInfinity;

    private void Awake() {
        fader = GetComponent<FadingController>();
        menu = GetComponent<BaseMenuScreen>();

        fader.OnCompleteFade += OnReadyToDismiss;
        PlayerInputListener.Instance.OnSelectPress += OnSelectPress;
        PlayerInputListener.Instance.OnUpPress += OnUpPress;
        PlayerInputListener.Instance.OnDownPress += OnDownPress;
    }

    private void OnDestroy() {
        fader.OnCompleteFade -= OnReadyToDismiss;
        PlayerInputListener.Instance.OnSelectPress -= OnSelectPress;
        PlayerInputListener.Instance.OnUpPress -= OnUpPress;
        PlayerInputListener.Instance.OnDownPress -= OnDownPress;
    }

    private void OnEnable() {
        timeSinceEnabled = Time.realtimeSinceStartup;
        currentMenuSelectionIndex = 0;
        RefreshSelection();
        if (!fader.enabled) {
            fader.DisplayContent();
        }
    }

    private void Start() {
        RefreshSelection();
        if (!fader.enabled) {
            fader.DisplayContent();
        }
    }

    private void OnReadyToDismiss(object sender, EventArgs e) {
        menu.SwitchScreen(selectedScreen);
    }

    private void OnUpPress(object sender, EventArgs e) {
        currentMenuSelectionIndex -= 1;
        if (currentMenuSelectionIndex < 0) {
            currentMenuSelectionIndex = menuOptions.Count - 1;
        }
        SoundManager.Instance.PlayMenuMove();
        RefreshSelection();
    }

    private void OnDownPress(object sender, EventArgs e) {
        currentMenuSelectionIndex += 1;
        if (currentMenuSelectionIndex > menuOptions.Count - 1) {
            currentMenuSelectionIndex = 0;
        }
        SoundManager.Instance.PlayMenuMove();
        RefreshSelection();
    }

    private void OnSelectPress(object sender, EventArgs e) {
        if (Time.realtimeSinceStartup - timeSinceEnabled > 0.3f) {
            SoundManager.Instance.PlayMenuSelect();
            EnterSelection();
        }
    }

    private void RefreshSelection() {
        foreach (TextMeshProUGUI menuOption in menuOptions) {
            menuOption.color = ColorHelper.UnselectedColor;
        }
        menuOptions[currentMenuSelectionIndex].color = ColorHelper.SelectedColor;
    }

    private void EnterSelection() {
        if (currentMenuSelectionIndex == 0) {
            selectedScreen = ScreenType.Intro;
        } else if (currentMenuSelectionIndex == 1) {
            selectedScreen = ScreenType.Options;
        } else {
            selectedScreen = ScreenType.Credits;
        }

        if (fader.enabled) { 
           fader.StartFadingOut();
        } else {
           menu.SwitchScreen(selectedScreen);
        }
    }

}
