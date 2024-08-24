using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static BaseMenuScreen;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private List<TextMeshProUGUI> menuOptions;

    private int currentMenuSelectionIndex = 0;
    private FadingController fader;
    private MenuKeyboardController keyboard;
    private BaseMenuScreen menu;
    private ScreenType selectedScreen = ScreenType.None;

    private void Awake() {
        keyboard = GetComponent<MenuKeyboardController>();
        keyboard.OnEnterKeyPress += OnEnterKeyPress;
        keyboard.OnUpKeyPress += OnUpKeyPress;
        keyboard.OnDownKeyPress += OnDownKeyPress;

        fader = GetComponent<FadingController>();
        fader.OnCompleteFade += OnReadyToDismiss;

        menu = GetComponent<BaseMenuScreen>();
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

    private void OnUpKeyPress(object sender, EventArgs e) {
        currentMenuSelectionIndex -= 1;
        if (currentMenuSelectionIndex < 0) {
            currentMenuSelectionIndex = menuOptions.Count - 1;
        }
        SoundManager.Instance.PlayMenuMove();
        RefreshSelection();
    }

    private void OnDownKeyPress(object sender, EventArgs e) {
        currentMenuSelectionIndex += 1;
        if (currentMenuSelectionIndex > menuOptions.Count - 1) {
            currentMenuSelectionIndex = 0;
        }
        SoundManager.Instance.PlayMenuMove();
        RefreshSelection();
    }

    private void OnEnterKeyPress(object sender, EventArgs e) {
        SoundManager.Instance.PlayMenuSelect();
        EnterSelection();
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
