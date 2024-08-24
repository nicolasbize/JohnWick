using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEditor;
using UnityEngine;
using System;
using static BaseMenuScreen;

public class MovieUI : MonoBehaviour
{
    [SerializeField] private ScreenType nextScreen;

    private FadingController fader;
    private MenuKeyboardController keyboard;
    private BaseMenuScreen menu;

    private void Awake() {
        fader = GetComponent<FadingController>();
        fader.OnCompleteFade += OnReadyToDismiss;

        keyboard = GetComponent<MenuKeyboardController>();
        keyboard.OnEnterKeyPress += OnEnterKeyPress;

        menu = GetComponent<BaseMenuScreen>();
    }

    private void OnReadyToDismiss(object sender, EventArgs e) {
        if (nextScreen != ScreenType.None) {
            menu.SwitchScreen(nextScreen);
        } else {
            menu.CloseScreen();
        }
    }

    private void OnEnterKeyPress(object sender, EventArgs e) {
        fader.SkipCurrentFrame();
    }
}
