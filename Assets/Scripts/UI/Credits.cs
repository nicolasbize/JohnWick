using System;
using UnityEngine;

public class Credits : MenuScreen
{

    public event EventHandler OnDismiss;

    [SerializeField] private RectTransform creditsContainer;
    [SerializeField] private int startPositionY;
    [SerializeField] private int endPositionY;
    [SerializeField] private float durationRoll;

    private float preciseY;
    private bool isStarted = false;
    private bool isActivated = false;
    private bool isRolling = false;
    private float timeSinceStartedRolling = float.NegativeInfinity;

    private void Start() {
        isStarted = true;
        if (isActivated) {
            StartRolling();
        }
    }

    public void Activate() {
        if (isStarted) {
            StartRolling();
        } else {
            isActivated = true;
        }
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
        if (IsSelectionMade()) {
            isRolling = false;
            SoundManager.Instance.PlayMenuSelect();
            OnDismiss?.Invoke(this, EventArgs.Empty);
        }

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
