using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ScoreMenuUI : BaseMenuScreen
{
    [SerializeField] private string title;
    [SerializeField] private int pointsPerHP;
    [SerializeField] private float durationBeforeUpdating;
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private TextMeshProUGUI initialScoreLabel;
    [SerializeField] private TextMeshProUGUI healthValueLabel;
    [SerializeField] private TextMeshProUGUI totalScoreLabel;

    private float timeSinceShown = float.NegativeInfinity;
    private bool isUpdatingScore = false;
    private bool isDoneUpdatingScore = false;
    private int initialScore;
    private int remainingHealth;
    private int totalScore;

    private float timeSinceLastUpdate = float.NegativeInfinity;
    private float durationEachUpdate = 0.1f;

    private FadingController fader;
    private BaseMenuScreen menu;

    private void Awake() {
        fader = GetComponent<FadingController>();
        menu = GetComponent<BaseMenuScreen>();

        PlayerInputListener.Instance.OnSelectPress += OnSelectPress;
        fader.OnCompleteFade += OnReadyToDismiss;

    }

    private void OnDestroy() {
        PlayerInputListener.Instance.OnSelectPress -= OnSelectPress;
        fader.OnCompleteFade -= OnReadyToDismiss;
    }

    private void Start() {
        titleLabel.text = title;
        timeSinceShown = Time.timeSinceLevelLoad;
        InitScreen();
    }

    private void OnSelectPress(object sender, EventArgs e) {
        if (isDoneUpdatingScore) {
            GameState.PlayerScore = totalScore;
            fader.StartFadingOut();
        }
    }

    private void OnReadyToDismiss(object sender, EventArgs e) {
        menu.CloseScreen();
    }

    private void Update() {
        if (!isUpdatingScore && (Time.timeSinceLevelLoad - timeSinceShown > durationBeforeUpdating)) {
            isUpdatingScore = true;
            isDoneUpdatingScore = false;
            initialScore = GameState.PlayerScore;
            remainingHealth = GameState.PlayerHealth;
            totalScore = initialScore;
            RefreshScreen();
            if (remainingHealth == 0) {
                titleLabel.text = "GAME OVER";
            }
        } else if (isUpdatingScore && !isDoneUpdatingScore &&
            (Time.timeSinceLevelLoad - timeSinceLastUpdate > durationEachUpdate)) {
            timeSinceLastUpdate = Time.timeSinceLevelLoad;
            if (remainingHealth > 0) {
                remainingHealth -= 1;
                totalScore += pointsPerHP;
                GameState.PlayerScore = totalScore;
                RefreshScreen();
                SoundManager.Instance.Play(SoundManager.SoundType.Tick);
            } else {
                isDoneUpdatingScore = true;

            }
        }

    }

    private void InitScreen() {
        initialScoreLabel.text = "";
        healthValueLabel.text = "";
        totalScoreLabel.text = "";
    }

    private void RefreshScreen() {
        initialScoreLabel.text = initialScore.ToString();
        healthValueLabel.text = remainingHealth.ToString();
        totalScoreLabel.text = totalScore.ToString();
    }
}
