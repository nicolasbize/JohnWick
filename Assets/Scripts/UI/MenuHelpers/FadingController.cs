using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static BaseMenuScreen;

public class FadingController : MonoBehaviour
{
    public event EventHandler OnCompleteFade;

    public enum Step { UIDisappearing, UIAppearing, UIStable, UIBlack }

    [SerializeField] private float durationFade;
    [SerializeField] private float durationStable;
    [SerializeField] private List<Transform> screensToFade;
    [SerializeField] private Transform blackScreen;
    [SerializeField] private bool isAutoDismissable;

    private Step currentStep = Step.UIAppearing;
    private float timeStartFade = float.NegativeInfinity;
    private float timeStartStable = float.NegativeInfinity;
    private int currentScreenIndex = 0;
    private bool hasCompleted = false;
    private MenuKeyboardController keyboard;
    private BaseMenuScreen menu;

    private void Awake() {
        keyboard = GetComponent<MenuKeyboardController>();
        keyboard.OnEnterKeyPress += OnEnterPress;

        menu = GetComponent<BaseMenuScreen>();
    }

    private void Start() {
        timeStartFade = Time.timeSinceLevelLoad;
    }

    private void OnEnterPress(object sender, EventArgs e) {
        // when not interactive, allow enter key to skip faster through the stable state
        if (currentStep == Step.UIStable && isAutoDismissable) {
            StartFadingOut();
        }
    }

    private void Update() {
        if (currentStep == Step.UIAppearing) { // black screen needs to fade
            float progress = (Time.timeSinceLevelLoad - timeStartFade) / durationFade;
            if (progress >= 1) {
                DisplayContent();
            }
            blackScreen.GetComponent<RawImage>().material.SetFloat("_Fade", 1 - progress);
        } else if (currentStep == Step.UIStable &&
                    isAutoDismissable &&
                    Time.timeSinceLevelLoad - timeStartStable > durationStable) {
            // fade out after enough time at stable state
            StartFadingOut();
        } else if (currentStep == Step.UIDisappearing) {
            float progress = (Time.timeSinceLevelLoad - timeStartFade) / durationFade;
            if (progress >= 1) {
                progress = 1.01f;
                currentStep = Step.UIBlack;
                timeStartFade = Time.timeSinceLevelLoad;
            }
            blackScreen.GetComponent<RawImage>().material.SetFloat("_Fade", progress);
        } else if (currentStep == Step.UIBlack) {
            if (currentScreenIndex < screensToFade.Count - 1) {
                screensToFade[currentScreenIndex].transform.gameObject.SetActive(false);
                currentScreenIndex += 1;
                screensToFade[currentScreenIndex].transform.gameObject.SetActive(true);
                currentStep = Step.UIAppearing;
            } else {
                if (!hasCompleted) {
                    hasCompleted = true;
                    OnCompleteFade?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public void DisplayContent() {
        blackScreen.GetComponent<RawImage>().material.SetFloat("_Fade", 0f);
        currentStep = Step.UIStable;
        timeStartStable = Time.timeSinceLevelLoad;
    }

    public void StartFadingOut() {
        currentStep = Step.UIDisappearing;
        timeStartFade = Time.timeSinceLevelLoad;
    }
}
