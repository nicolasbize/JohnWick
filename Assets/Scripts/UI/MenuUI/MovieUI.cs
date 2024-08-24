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
    private BaseMenuScreen menu;

    private void Awake() {
        fader = GetComponent<FadingController>();
        fader.OnCompleteFade += OnReadyToDismiss;

        menu = GetComponent<BaseMenuScreen>();
    }

    private void OnReadyToDismiss(object sender, EventArgs e) {
        menu.SwitchScreen(nextScreen);
    }
}
