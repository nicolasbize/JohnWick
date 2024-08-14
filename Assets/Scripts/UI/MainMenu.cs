using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private bool skipSplash;
    [SerializeField] private SplashScreen splashScreen;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private Credits creditsScreen;

    private int currentMenuSelectionIndex = 0;
    private bool inMainMenu = false;
    private bool isVerticalMovementDetected = false;

    private void Awake() {
        splashScreen.OnFinishSplash += OnFinishSplashScreen;
    }

    private void OnFinishSplashScreen(object sender, System.EventArgs e) {
        splashScreen.gameObject.SetActive(false);
        inMainMenu = true;
    }

    private void Start() {
        inMainMenu = skipSplash;
        splashScreen.gameObject.SetActive(!skipSplash);
        creditsScreen.OnDismiss += OnCreditsDismiss;
        creditsScreen.gameObject.SetActive(false);
        RefreshSelection();
    }

    private void OnCreditsDismiss(object sender, System.EventArgs e) {
        creditsScreen.gameObject.SetActive(false);
        inMainMenu = true;
    }

    private void SplashScreen_OnFinishSplash(object sender, System.EventArgs e) {
        throw new System.NotImplementedException();
    }

    private void RefreshSelection() {
        ColorUtility.TryParseHtmlString("#59594E", out Color unselectedColor);
        ColorUtility.TryParseHtmlString("#D2D27C", out Color selectedColor);
        foreach (TextMeshProUGUI menuOption in menuOptions) {
            menuOption.color = unselectedColor;
        }
        menuOptions[currentMenuSelectionIndex].color = selectedColor;
    }

    private void Update() {
        if (inMainMenu) {
            float upDownMovement = Input.GetAxisRaw("Vertical");
            if (!isVerticalMovementDetected && upDownMovement > 0) {
                currentMenuSelectionIndex -= 1;
                isVerticalMovementDetected = true;
                if (currentMenuSelectionIndex < 0) {
                    currentMenuSelectionIndex = menuOptions.Count - 1;
                }
                RefreshSelection();
            } else if (!isVerticalMovementDetected && upDownMovement < 0) {
                currentMenuSelectionIndex += 1;
                isVerticalMovementDetected = true;
                if (currentMenuSelectionIndex > menuOptions.Count - 1) {
                    currentMenuSelectionIndex = 0;
                }
                RefreshSelection();
            } else if (upDownMovement == 0) {
                isVerticalMovementDetected = false;
            }

            if (IsSelectionMade()) {
                EnterSelection();
            }
        }
    }

    private void EnterSelection() {
        inMainMenu = false;
        if (currentMenuSelectionIndex == 0) {
            // Fade to black
            // Play the Intro
            // Play the game
        } else if (currentMenuSelectionIndex == 1) {
            // Show Options
        } else {
            ShowCredits();
        }
    }

    private bool IsSelectionMade() {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Attack");
    }

    private void ShowCredits() {
        creditsScreen.gameObject.SetActive(true);
        creditsScreen.Activate();
    }

}
