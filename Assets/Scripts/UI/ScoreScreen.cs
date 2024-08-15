using System;
using TMPro;
using UnityEngine;

public class ScoreScreen : MonoBehaviour
{
    public event EventHandler OnDismiss;

    [SerializeField] public string title;
    [SerializeField] public int pointsPerHP;
    [SerializeField] public TextMeshProUGUI titleLabel;
    [SerializeField] public TextMeshProUGUI initialScoreLabel;
    [SerializeField] public TextMeshProUGUI healthValueLabel;
    [SerializeField] public TextMeshProUGUI totalScoreLabel;

    private bool isUpdatingScore = false;
    private int initialScore;
    private int remainingHealth;
    private int totalScore;

    private float timeSinceLastUpdate = float.NegativeInfinity;
    private float durationEachUpdate = 0.1f;
    private bool dismissed = false;

    private void Start() {
        isUpdatingScore = true;
        titleLabel.text = title;
        initialScore = PlayerPrefs.GetInt(PrefsHelper.SCORE, 0);
        remainingHealth = PlayerPrefs.GetInt(PrefsHelper.HEALTH, 0);
        totalScore = initialScore;
        if (remainingHealth == 0) {
            titleLabel.text = "GAME OVER";
        }
        RefreshScreen();
    }

    private void Update() {
        if (isUpdatingScore && 
            (Time.timeSinceLevelLoad - timeSinceLastUpdate > durationEachUpdate)) {
            timeSinceLastUpdate = Time.timeSinceLevelLoad;
            if (remainingHealth > 0) {
                remainingHealth -= 1;
                totalScore += pointsPerHP;
                RefreshScreen();
                SoundManager.Instance.Play(SoundManager.SoundType.Tick);
            } else {
                isUpdatingScore = false;
            }
        } else if (!isUpdatingScore && IsSelectionMade()) {
            if (!dismissed) {
                dismissed = true;
                PlayerPrefs.SetInt(PrefsHelper.SCORE, totalScore);
                OnDismiss?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private bool IsSelectionMade() {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown(InputHelper.BTN_ATTACK);
    }

    private void RefreshScreen() {
        initialScoreLabel.text = initialScore.ToString();
        healthValueLabel.text = remainingHealth.ToString();
        totalScoreLabel.text = totalScore.ToString();
    }
}
