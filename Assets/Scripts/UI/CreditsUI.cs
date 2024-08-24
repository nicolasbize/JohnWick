using System;
using UnityEngine;

public class CreditsUI : MonoBehaviour
{
    [SerializeField] private RectTransform creditsContainer;
    [SerializeField] private int startPositionY;
    [SerializeField] private int endPositionY;
    [SerializeField] private float durationRoll;

    private FadingController fader;
    private BaseMenuScreen menu;
    private MenuKeyboardController keyboard;
    private float preciseY;
    private bool isRolling = false;
    private float timeSinceStartedRolling = float.NegativeInfinity;

    private void Awake() {
        keyboard = GetComponent<MenuKeyboardController>();
        keyboard.OnEnterKeyPress += OnEnterKeyPress;

        fader = GetComponent<FadingController>();
        fader.OnCompleteFade += OnReadyToDismiss;

        menu = GetComponent<BaseMenuScreen>();
    }

    private void Start() {
        StartRolling();
        timeSinceStartedRolling = Time.timeSinceLevelLoad;
    }

    private void OnEnterKeyPress(object sender, EventArgs e) {
        if (Time.timeSinceLevelLoad - timeSinceStartedRolling > 3f) {
            isRolling = false;
            fader.StartFadingOut();
        }
    }

    private void OnReadyToDismiss(object sender, EventArgs e) {
        menu.CloseScreen();
    }

    private void StartRolling() {
        ResetPosition();
        timeSinceStartedRolling = Time.timeSinceLevelLoad;
        isRolling = true;
    }

    private void ResetPosition() {
        SetYPosition(startPositionY);
    }

    private void SetYPosition(float y) {
        creditsContainer.anchoredPosition = new Vector3(0, Mathf.FloorToInt(y), 0);
    }


    private void Update() {
        if (isRolling) {
            float progress = (Time.timeSinceLevelLoad - timeSinceStartedRolling) / durationRoll;
            if (progress >= 1) {
                progress = 1f;
            }
            preciseY = Mathf.Lerp(startPositionY, endPositionY, progress);
            SetYPosition(preciseY);
        }
    }

}
