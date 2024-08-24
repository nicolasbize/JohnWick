using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashMenuScreen : BaseMenuScreen
{

    //public enum Step { UIDisappearing, UIAppearing, UIStable, UIBlack}

    //[SerializeField] private float durationFade;
    //[SerializeField] private float durationStable;
    //[SerializeField] private List<Transform> screensToFade;
    //[SerializeField] private Transform blackScreen;
    //[SerializeField] private bool isInteractive;

    //private Step currentStep = Step.UIAppearing;
    //private float timeStartFade = float.NegativeInfinity;
    //private float timeStartStable = float.NegativeInfinity;
    //private int currentScreenIndex = 0;
    //private bool hasCompleted = false;

    //private void Awake() {
        
    //}

    //private void OnEnterPress(object sender, EventArgs e) {
    //    // when not interactive, allow enter key to skip faster through the stable state
    //    if (currentStep == Step.UIStable && !isInteractive) {
    //        StartFadingOut();
    //    }
    //}

    //private void Start() {
    //    timeStartFade = Time.timeSinceLevelLoad;
    //}

    //private void Update() {
    //    if (currentStep == Step.UIAppearing) { // black screen needs to fade
    //        float progress = (Time.timeSinceLevelLoad - timeStartFade) / durationFade;
    //        if (progress >= 1) {
    //            progress = 1.01f;
    //            currentStep = Step.UIStable;
    //            timeStartStable = Time.timeSinceLevelLoad;
    //        }
    //        blackScreen.GetComponent<RawImage>().material.SetFloat("_Fade", 1 - progress);
    //    } else if (currentStep == Step.UIStable &&
    //                !isInteractive &&
    //                Time.timeSinceLevelLoad - timeStartStable > durationStable) {
    //        // fade out after enough time at stable state
    //        StartFadingOut();
    //    } else if (currentStep == Step.UIDisappearing) {
    //        float progress = (Time.timeSinceLevelLoad - timeStartFade) / durationFade;
    //        if (progress >= 1) {
    //            progress = 1.01f;
    //            currentStep = Step.UIBlack;
    //            timeStartFade = Time.timeSinceLevelLoad;
    //        }
    //        blackScreen.GetComponent<RawImage>().material.SetFloat("_Fade", progress);
    //    } else if (currentStep == Step.UIBlack) {
    //        if (currentScreenIndex < screensToFade.Count - 1) {
    //            screensToFade[currentScreenIndex].transform.gameObject.SetActive(false);
    //            currentScreenIndex += 1;
    //            screensToFade[currentScreenIndex].transform.gameObject.SetActive(true);
    //            currentStep = Step.UIAppearing;
    //        } else {
    //            if (!hasCompleted) {
    //                hasCompleted = true;
    //                //DismissScreen();
    //            }
    //        }
    //    }
    //}

    //public void StartFadingOut() {
    //    currentStep = Step.UIDisappearing;
    //    timeStartFade = Time.timeSinceLevelLoad;
    //}

    //private bool IsInputSkipped() {
    //    //if (currentStep == Step.UIStable && (Time.timeSinceLevelLoad - timeStartStable > 1f)) {
    //    //    return IsSelectionMade();
    //    //}
    //    return false;
    //}
}
