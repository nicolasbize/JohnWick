using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static BaseMenuScreen;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private Transform menuOptionsContainer;

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
        foreach (Clickable option in menuOptionsContainer.GetComponentsInChildren<Clickable>()) {
            option.OnClick += OnOptionClick;
        }
    }

    private void OnOptionClick(object sender, EventArgs e) {
        for (int i = 0; i < menuOptions.Count; i++) {
            if (menuOptions[i].GetComponent<Clickable>() == (Clickable) sender) {
                currentMenuSelectionIndex = i;
                RefreshSelection();
                EnterSelection();
            }
        }
    }

    private void OnDestroy() {
        fader.OnCompleteFade -= OnReadyToDismiss;
        PlayerInputListener.Instance.OnSelectPress -= OnSelectPress;
        PlayerInputListener.Instance.OnUpPress -= OnUpPress;
        PlayerInputListener.Instance.OnDownPress -= OnDownPress;
        foreach (Clickable option in menuOptionsContainer.GetComponentsInChildren<Clickable>()) {
            option.OnClick += OnOptionClick;
        }
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
        SoundManager.Instance.PlayMenuSelect();
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
