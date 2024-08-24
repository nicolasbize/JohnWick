using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ScoreMenuUI : BaseMenuScreen
{
    [SerializeField] public string title;
    [SerializeField] public int pointsPerHP;
    [SerializeField] public float durationBeforeUpdating;
    [SerializeField] public TextMeshProUGUI titleLabel;
    [SerializeField] public TextMeshProUGUI initialScoreLabel;
    [SerializeField] public TextMeshProUGUI healthValueLabel;
    [SerializeField] public TextMeshProUGUI totalScoreLabel;

    private float timeSinceShown = float.NegativeInfinity;
    private bool isUpdatingScore = false;
    private bool isDoneUpdatingScore = false;
    private int initialScore;
    private int remainingHealth;
    private int totalScore;

    private float timeSinceLastUpdate = float.NegativeInfinity;
    private float durationEachUpdate = 0.1f;

    private FadingController fader;
    private MenuKeyboardController keyboard;
    private BaseMenuScreen menu;

    private void Awake() {
        keyboard = GetComponent<MenuKeyboardController>();
        keyboard.OnEnterKeyPress += OnEnterKeyPress;

        fader = GetComponent<FadingController>();
        fader.OnCompleteFade += OnReadyToDismiss;

        menu = GetComponent<BaseMenuScreen>();
    }

    private void Start() {
        titleLabel.text = title;
        timeSinceShown = Time.timeSinceLevelLoad;
        InitScreen();
    }

    private void OnEnterKeyPress(object sender, EventArgs e) {
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
