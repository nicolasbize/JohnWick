using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MenuScreen
{
    [SerializeField] private bool skipSplash;
    [SerializeField] private bool skipIntro;
    [SerializeField] private SplashScreen splashScreen;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private Credits creditsScreen;
    [SerializeField] private Options optionsScreen;
    [SerializeField] private SplashScreen introScreen;
    [SerializeField] private SplashScreen outroScreen;
    [SerializeField] private ScoreScreen scoreScreen;

    private int currentMenuSelectionIndex = 0;
    private bool inMainMenu = false;
    private bool isVerticalMovementDetected = false;

    private void Awake() {
        splashScreen.OnFinishSplash += OnFinishSplashScreen;
        introScreen.OnFinishSplash += OnFinishIntro;
        creditsScreen.OnDismiss += OnCreditsDismiss;
        optionsScreen.OnDismiss += OnOptionsDismiss;
        scoreScreen.OnDismiss += OnScoreScreenDismiss;
        outroScreen.OnFinishSplash += OnFinishOutro;
        MusicManager.Instance.OnFadeOut += OnMusicFadeOut;
    }

    private void OnMusicFadeOut(object sender, MusicManager.OnFadeEventArgs e) {
        if (e.musicFaded == MusicManager.Melody.IntroOutro) {
            StartNewGame();
        }
    }

    private void OnFinishOutro(object sender, System.EventArgs e) {
        outroScreen.gameObject.SetActive(false);
        ShowCredits();
    }

    private void Start() {

        if (!PlayerPrefs.HasKey(PrefsHelper.NEW_GAME)) {
            ResetAllPrefs();
        }

        creditsScreen.gameObject.SetActive(false);
        optionsScreen.gameObject.SetActive(false);
        introScreen.gameObject.SetActive(false);
        outroScreen.gameObject.SetActive(false);
        scoreScreen.gameObject.SetActive(false);

        bool isMenuMusicPlayed = true;
        if (PlayerPrefs.HasKey(PrefsHelper.GAME_OVER)) {
            inMainMenu = false;
            outroScreen.gameObject.SetActive(true);
            isMenuMusicPlayed = false;
        } else {
            inMainMenu = skipSplash;
            splashScreen.gameObject.SetActive(!skipSplash);
        }
        if (isMenuMusicPlayed) {
            MusicManager.Instance.Play(MusicManager.Melody.MainMenu);
        } else {
            MusicManager.Instance.Play(MusicManager.Melody.IntroOutro);
        }
        RefreshSelection();
    }

    private void OnScoreScreenDismiss(object sender, System.EventArgs e) {
        // let player refresh if needed
    }

    private void OnFinishIntro(object sender, System.EventArgs e) {
        MusicManager.Instance.Stop();
    }

    private void OnFinishSplashScreen(object sender, System.EventArgs e) {
        splashScreen.gameObject.SetActive(false);
        inMainMenu = true;
    }

    private void OnOptionsDismiss(object sender, System.EventArgs e) {
        optionsScreen.gameObject.SetActive(false);
        inMainMenu = true;
    }

    private void OnCreditsDismiss(object sender, System.EventArgs e) {
        creditsScreen.gameObject.SetActive(false);
        if (PlayerPrefs.HasKey(PrefsHelper.GAME_OVER)) {
            scoreScreen.gameObject.SetActive(true);
        } else {
            inMainMenu = true;
        }
    }

    private void RefreshSelection() {
        foreach (TextMeshProUGUI menuOption in menuOptions) {
            menuOption.color = ColorHelper.UnselectedColor;
        }
        menuOptions[currentMenuSelectionIndex].color = ColorHelper.SelectedColor;
    }

    private void Update() {
        if (inMainMenu) {
            float upDownMovement = Input.GetAxisRaw(InputHelper.AXIS_VERTICAL);
            if (!isVerticalMovementDetected && upDownMovement > 0) {
                currentMenuSelectionIndex -= 1;
                isVerticalMovementDetected = true;
                if (currentMenuSelectionIndex < 0) {
                    currentMenuSelectionIndex = menuOptions.Count - 1;
                }
                SoundManager.Instance.PlayMenuMove();
                RefreshSelection();
            } else if (!isVerticalMovementDetected && upDownMovement < 0) {
                currentMenuSelectionIndex += 1;
                isVerticalMovementDetected = true;
                if (currentMenuSelectionIndex > menuOptions.Count - 1) {
                    currentMenuSelectionIndex = 0;
                }
                SoundManager.Instance.PlayMenuMove();
                RefreshSelection();
            } else if (upDownMovement == 0) {
                isVerticalMovementDetected = false;
            }

            if (IsSelectionMade()) {
                SoundManager.Instance.PlayMenuSelect();
                EnterSelection();
            }
        }
    }

    private void EnterSelection() {
        inMainMenu = false;
        if (currentMenuSelectionIndex == 0) {
            ShowIntro();
        } else if (currentMenuSelectionIndex == 1) {
            ShowOptions();
        } else {
            ShowCredits();
        }
    }



    private void ShowOptions() {
        optionsScreen.gameObject.SetActive(true);
        optionsScreen.RefreshOptions();
    }

    private void ShowCredits() {
        creditsScreen.gameObject.SetActive(true);
        creditsScreen.Activate();
    }

    private void ShowIntro() {
        PlayerPrefs.DeleteKey(PrefsHelper.GAME_OVER);
        if (!skipIntro) {
            MusicManager.Instance.Play(MusicManager.Melody.IntroOutro);
            introScreen.gameObject.SetActive(true);
        } else {
            StartNewGame();
        }
    }

    private void StartNewGame() {
        ResetAllPrefs();
        SceneManager.LoadScene(SceneHelper.GAME_SCENE, LoadSceneMode.Single);
    }

    private void ResetAllPrefs() {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(PrefsHelper.NEW_GAME, 1);
    }

}
