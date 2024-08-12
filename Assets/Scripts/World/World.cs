using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

    public event EventHandler OnLevelTransitionStart;

    [SerializeField] private List<Level> levels;
    [SerializeField] private Transform levelParent;

    private int currentLevel = 0;

    public static World Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadLevel(currentLevel);
        Camera.main.GetComponent<CameraFollow>().OnPositionChange += OnCameraPositionChange;
        TransitionScreen.Instance.OnReadyToLoadContent += OnTransitionReadyToLoadLevel;
        TransitionScreen.Instance.OnReadyToPlay += OnTransitionReadyToPlay;
    }

    private void OnTransitionReadyToPlay(object sender, EventArgs e) {
        PlayerController.Instance.ReturnControlsToPlayer();
    }

    private void OnTransitionReadyToLoadLevel(object sender, EventArgs e) {
        Camera.main.GetComponent<CameraFollow>().StartNewLevel();
        PlayerController.Instance.StartNewLevel();
        LoadLevel(currentLevel);
        TransitionScreen.Instance.FinishTransition();
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
    }

    private void OnStartTransitionLevel(object sender, EventArgs e) {
        OnLevelTransitionStart?.Invoke(this, EventArgs.Empty);

    }

    public void CompleteLevel() {
        // show high score for level
        Debug.Log("level cleared - score");
        if (currentLevel < levels.Count -1 ) {
            currentLevel += 1;
            TransitionScreen.Instance.StartTransition();
        } else {
            Debug.Log("complete game");
        }
    }
}
