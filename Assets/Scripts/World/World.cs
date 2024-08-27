using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour
{

    public event EventHandler OnLevelTransitionStart;

    [SerializeField] private List<Level> levels;
    [SerializeField] private Transform levelParent;
    [SerializeField] private BaseMenuScreen scoreScreen;
    [SerializeField] private Counter currentScore;
    [SerializeField] private TransitionScreen transitionScreen;

    public int CurrentLevelIndex { get; private set;} = 0;

    public static World Instance;

    private void Awake() {
        Instance = this;
        foreach (Transform existingLevel in levelParent) {
            Destroy(existingLevel.gameObject);
        }

        scoreScreen.OnCloseScreen += OnScoreDismiss;
    }

    private void OnScoreDismiss(object sender, EventArgs e) {
        if (PlayerController.Instance.CurrentHP > 0) {
            GoToNextLevel();
        }
    }

    private void Start() {
        LoadLevel(CurrentLevelIndex);
        Camera.main.GetComponent<CameraFollow>().OnPositionChange += OnCameraPositionChange;
        transitionScreen.OnReadyToLoadContent += OnTransitionReadyToLoadLevel;
        transitionScreen.OnReadyToPlay += OnTransitionReadyToPlay;
    }

    private void OnTransitionReadyToPlay(object sender, EventArgs e) {
        PlayerController.Instance.ReturnControlsToPlayer();
    }

    private void OnCameraPositionChange(object sender, EventArgs e) {
        if (Camera.main.transform.position.x >= 620) {
            Camera.main.transform.position = new Vector3(620, 32, -10); // lock at end of level
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
        GameState.PlayerHealth = PlayerController.Instance.CurrentHP;
        GameState.PlayerScore = currentScore.GetValue();
        GameState.HasCompletedGame = true;
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
            transitionScreen.StartTransition();
        } else {
            Debug.Log("complete game");
        }
    }

    private void OnTransitionReadyToLoadLevel(object sender, EventArgs e) {
        //PlayerPrefs.SetInt(PrefsHelper.SCORE, currentScore.GetValue());
        //PlayerPrefs.SetInt(PrefsHelper.HEALTH, PlayerController.Instance.CurrentHP);
        scoreScreen.gameObject.SetActive(true);

    }

    private void GoToNextLevel() {
        scoreScreen.gameObject.SetActive(false);
        Camera.main.GetComponent<CameraFollow>().StartNewLevel();
        PlayerController.Instance.StartNewLevel();
        LoadLevel(CurrentLevelIndex);
        transitionScreen.FinishTransition();
    }
}
