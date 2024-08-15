using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MenuScreen
{
    public event EventHandler OnFinishSplash;

    public enum Step { UIDisappearing, UIAppearing, UIStable, UIBlack}

    [SerializeField] private float durationFade;
    [SerializeField] private float durationStable;
    [SerializeField] private List<Transform> screensToFade;
    [SerializeField] private Transform blackScreen;

    private Step currentStep = Step.UIAppearing;
    private float blackScreenAlpha = 1f;
    private float timeStartFade = float.NegativeInfinity;
    private float timeStartStable = float.NegativeInfinity;
    private int currentScreenIndex = 0;
    private bool hasCompleted = false;

    private void Start() {
        timeStartFade = Time.timeSinceLevelLoad;
        blackScreenAlpha = blackScreen.GetComponent<RawImage>().material.GetFloat("_Fade");
    }

    private void Update() {
        if (currentStep == Step.UIAppearing) { // black screen needs to fade
            float progress = (Time.timeSinceLevelLoad - timeStartFade) / durationFade;
            if (progress >= 1) {
                progress = 1.01f;
                currentStep = Step.UIStable;
                timeStartStable = Time.timeSinceLevelLoad;
            }
            blackScreen.GetComponent<RawImage>().material.SetFloat("_Fade", 1 - progress);
        } else if (currentStep == Step.UIStable) {
            if ((Time.timeSinceLevelLoad - timeStartStable > durationStable) || IsInputSkipped()) {
                currentStep = Step.UIDisappearing;
                timeStartFade = Time.timeSinceLevelLoad;
            }
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
                    OnFinishSplash?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    private bool IsInputSkipped() {
        if (currentStep == Step.UIStable) {
            return IsSelectionMade();
        }
        return false;
    }
}
