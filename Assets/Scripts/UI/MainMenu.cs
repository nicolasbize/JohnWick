using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private bool skipSplash;
    [SerializeField] private SplashScreen splashScreen;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private Credits creditsScreen;
    [SerializeField] private Options optionsScreen;
    [SerializeField] private AudioClip moveMenuSound;
    [SerializeField] private AudioClip selectSound;

    private int currentMenuSelectionIndex = 0;
    private bool inMainMenu = false;
    private bool isVerticalMovementDetected = false;
    private AudioSource audioSource;

    public static MainMenu Instance;

    private void Awake() {
        splashScreen.OnFinishSplash += OnFinishSplashScreen;
        audioSource = GetComponent<AudioSource>();
        Instance = this;
    }

    public void PlayMenuMovementSound() {
        audioSource.PlayOneShot(moveMenuSound);
    }

    public void PlayMenuSelectSound() {
        audioSource.PlayOneShot(selectSound);
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
        optionsScreen.gameObject.SetActive(false);
        optionsScreen.OnDismiss += OnOptionsDismiss;
        RefreshSelection();
    }

    private void OnOptionsDismiss(object sender, System.EventArgs e) {
        optionsScreen.gameObject.SetActive(false);
        inMainMenu = true;
    }

    private void OnCreditsDismiss(object sender, System.EventArgs e) {
        creditsScreen.gameObject.SetActive(false);
        inMainMenu = true;
    }

    private void SplashScreen_OnFinishSplash(object sender, System.EventArgs e) {
        throw new System.NotImplementedException();
    }

    public Color UnselectedColor {
        get {
            ColorUtility.TryParseHtmlString("#59594E", out Color unselectedColor);
            return unselectedColor;
        }
    }

    public Color SelectedColor {
        get {
            ColorUtility.TryParseHtmlString("#D2D27C", out Color unselectedColor);
            return unselectedColor;
        }
    }

    private void RefreshSelection() {
        foreach (TextMeshProUGUI menuOption in menuOptions) {
            menuOption.color = UnselectedColor;
        }
        menuOptions[currentMenuSelectionIndex].color = SelectedColor;
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
                PlayMenuMovementSound();
                RefreshSelection();
            } else if (!isVerticalMovementDetected && upDownMovement < 0) {
                currentMenuSelectionIndex += 1;
                isVerticalMovementDetected = true;
                if (currentMenuSelectionIndex > menuOptions.Count - 1) {
                    currentMenuSelectionIndex = 0;
                }
                PlayMenuMovementSound();
                RefreshSelection();
            } else if (upDownMovement == 0) {
                isVerticalMovementDetected = false;
            }

            if (IsSelectionMade()) {
                PlayMenuSelectSound();
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
            ShowOptions();
        } else {
            ShowCredits();
        }
    }

    public bool IsSelectionMade() {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Attack");
    }

    private void ShowOptions() {
        optionsScreen.gameObject.SetActive(true);
        optionsScreen.RefreshOptions();
    }

    private void ShowCredits() {
        creditsScreen.gameObject.SetActive(true);
        creditsScreen.Activate();
    }

}
