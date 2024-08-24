using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BaseMenuScreen;

public class ScreenManager : MonoBehaviour
{

    [SerializeField] private List<MenuScreenSO> screenData;
    [SerializeField] private bool skipSplash;
    [SerializeField] private bool skipIntro;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private SoundManager soundManager;

    private ScreenType currentScreen;

    private void Awake() {
        musicManager.OnFadeOut += OnMusicFadeOut;
    }

    private void OnMusicFadeOut(object sender, MusicManager.OnFadeEventArgs e) {
        if (currentScreen == ScreenType.Intro) {
            StartGame();
        }
    }

    private void Start() {
        if (GameState.HasCompletedGame) {
            musicManager.Play(MusicManager.Melody.IntroOutro);
            TransitionTo(ScreenType.Outro);
        } else {
            musicManager.Play(MusicManager.Melody.MainMenu);
            if (skipSplash) {
                TransitionTo(ScreenType.MainMenu);
            } else {
                TransitionTo(ScreenType.Splash);
            }
        }
    }

    private void TransitionTo(ScreenType type) {
        // clear children
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        // find which screen to instantiate
        BaseMenuScreen nextScreen = Instantiate(screenData.Find(x => x.screenType == type).screen, transform);
        currentScreen = type;
        nextScreen.OnSelectScreen += OnSelectNextScreen;
        nextScreen.OnCloseScreen += OnCloseScreen;
    }

    private void OnCloseScreen(object sender, System.EventArgs e) {
        if (GameState.HasCompletedGame && currentScreen == ScreenType.Credits) {
            TransitionTo(ScreenType.Score);
        } else if (currentScreen == ScreenType.Intro) {
            musicManager.Stop(); // start fading out before going to level
        } else {
            TransitionTo(ScreenType.MainMenu);
        }
    }

    private void OnSelectNextScreen(object sender, ScreenEventArgs e) {
        if (e.selectedScreen == ScreenType.Intro) {
            if (skipIntro) {
                StartGame();
            } else {
                musicManager.Play(MusicManager.Melody.IntroOutro);
            }
        }
        if (e.selectedScreen != ScreenType.None) {
            TransitionTo(e.selectedScreen);
        } else {
            Debug.Log("asked to transition to empty screen?>");
        }
    }

    private void StartGame() {
        SceneManager.LoadScene(SceneHelper.GAME_SCENE, LoadSceneMode.Single);
    }

}
