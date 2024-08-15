using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class World : MonoBehaviour
{

    public event EventHandler OnLevelTransitionStart;

    [SerializeField] private List<Level> levels;
    [SerializeField] private Transform levelParent;
    [SerializeField] private ScoreScreen scoreScreen;
    [SerializeField] private Counter currentScore;

    public int CurrentLevelIndex { get; private set;} = 0;

    public static World Instance;

    private void Awake() {
        Instance = this;
        foreach (Transform existingLevel in levelParent) {
            Destroy(existingLevel.gameObject);
        }
        scoreScreen.OnDismiss += OnScoreDismiss;
    }

    private void OnScoreDismiss(object sender, EventArgs e) {
        if (PlayerController.Instance.CurrentHP > 0) {
            GoToNextLevel();
        }
    }

    private void Start() {
        LoadLevel(CurrentLevelIndex);
        Camera.main.GetComponent<CameraFollow>().OnPositionChange += OnCameraPositionChange;
        TransitionScreen.Instance.OnReadyToLoadContent += OnTransitionReadyToLoadLevel;
        TransitionScreen.Instance.OnReadyToPlay += OnTransitionReadyToPlay;
    }

    private void Update() {
        
    }

    private void OnTransitionReadyToPlay(object sender, EventArgs e) {
        PlayerController.Instance.ReturnControlsToPlayer();
    }

    private void OnCameraPositionChange(object sender, EventArgs e) {
        if (Camera.main.transform.position.x >= 320) {
            Camera.main.transform.position = new Vector3(320, 32, -10); // lock at end of level
            Camera.main.GetComponent<CameraFollow>().LockInPlace();
        }
    }

    private void LoadLevel(int levelIndex) {
        foreach(Transform existingLevel in levelParent) {
            Destroy(existingLevel.gameObject);
        }
        Level level = Instantiate(levels[levelIndex], levelParent);
        level.transform.position = Vector3.zero;
        level.OnStartTransition += OnStartTransitionLevel;
        level.OnFinishLastLevel += OnFinishLastLevel;
    }

    private void OnFinishLastLevel(object sender, EventArgs e) {
        PlayerPrefs.SetInt(PrefsHelper.SCORE, currentScore.GetValue());
        PlayerPrefs.SetInt(PrefsHelper.HEALTH, PlayerController.Instance.CurrentHP);
        PlayerPrefs.SetInt(PrefsHelper.GAME_OVER, 1);
        SceneManager.LoadScene(SceneHelper.MENU_SCENE, LoadSceneMode.Single);
    }

    private void OnStartTransitionLevel(object sender, EventArgs e) {
        OnLevelTransitionStart?.Invoke(this, EventArgs.Empty);

    }

    public void CompleteLevel() {
        // show high score for level
        Debug.Log("level cleared - score");
        if (CurrentLevelIndex < levels.Count -1 ) {
            CurrentLevelIndex += 1;
            TransitionScreen.Instance.StartTransition();
        } else {
            Debug.Log("complete game");
        }
    }

    private void OnTransitionReadyToLoadLevel(object sender, EventArgs e) {
        PlayerPrefs.SetInt(PrefsHelper.SCORE, currentScore.GetValue());
        PlayerPrefs.SetInt(PrefsHelper.HEALTH, PlayerController.Instance.CurrentHP);
        scoreScreen.gameObject.SetActive(true);

    }

    private void GoToNextLevel() {
        scoreScreen.gameObject.SetActive(false);
        Camera.main.GetComponent<CameraFollow>().StartNewLevel();
        PlayerController.Instance.StartNewLevel();
        LoadLevel(CurrentLevelIndex);
        TransitionScreen.Instance.FinishTransition();
    }
}
