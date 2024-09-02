using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{

    [SerializeField] private float durationHealthDisplayed;
    [SerializeField] private HealthBar enemyHealthbar;
    [SerializeField] private Image enemyAvatar;
    [SerializeField] private List<EnemySO> enemyData;
    [SerializeField] private HealthBar heroHealthbar;
    [SerializeField] private Animator goIndicatorAnimator;
    [SerializeField] private PlayerController player;
    [SerializeField] private Continue continueScreen;
    [SerializeField] private BaseMenuScreen optionsScreen;
    [SerializeField] private BaseMenuScreen scoreScreen;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Counter score;
    [SerializeField] private Transform touchControls;

    private float timeSinceLastHealthRefresh = float.NegativeInfinity;
    private bool isBossMode = false;
    private bool isGameOver = false;
    private float timeSinceOptionsShown = float.NegativeInfinity;

    public static UI Instance;

    private void Awake() {
        Instance = this;
        PlayerInputListener.Instance.OnCancelPress += OnCancelPress;
    }

    private void OnDestroy() {
        PlayerInputListener.Instance.OnCancelPress -= OnCancelPress;
    }

    private void OnCancelPress(object sender, EventArgs e) {
        ShowOptions();
    }

    private void Start() {
        continueScreen.gameObject.SetActive(false);
        HideEnemyHealthbar();
        player.OnDeath += OnPlayerDeath;
        continueScreen.OnContinue += OnContinueGame;
        continueScreen.OnGameOver += OnGameOver;
        scoreScreen.OnCloseScreen += OnDismissScore;
        optionsScreen.OnCloseScreen += OnOptionsDismiss;
        goIndicatorAnimator.gameObject.SetActive(false);
        touchControls.gameObject.SetActive(GameState.IsUsingTouchControls);
        score.SetValue(GameState.PlayerScore);
    }

    private void OnOptionsDismiss(object sender, System.EventArgs e) {
        if (optionsScreen.gameObject.activeSelf && (Time.realtimeSinceStartup - timeSinceOptionsShown > 0.3f)) {
            optionsScreen.gameObject.SetActive(false);
            touchControls.gameObject.SetActive(GameState.IsUsingTouchControls);
            Time.timeScale = 1f;
        }
    }

    private void OnDismissScore(object sender, System.EventArgs e) {
        if (isGameOver) {
            // Fade out music
            SceneManager.LoadScene(SceneHelper.MENU_SCENE, LoadSceneMode.Single);
        }
    }

    private void OnGameOver(object sender, System.EventArgs e) {
        continueScreen.gameObject.SetActive(false);
        GameState.PlayerScore = score.GetValue();
        GameState.PlayerHealth = 0;
        scoreScreen.gameObject.SetActive(true);
        isGameOver = true;
    }

    private void OnContinueGame(object sender, System.EventArgs e) {
        continueScreen.gameObject.SetActive(false);
        // substract 1000 pts for each death
        score.SetValue(Mathf.Max(0, score.GetValue() - 1000));
        player.Respawn();
    }

    public void AddScore(int value) {
        score.Add(value);
    }

    private void OnPlayerDeath(object sender, System.EventArgs e) {
        continueScreen.gameObject.SetActive(true);
        continueScreen.StartCountdown();
    }

    private void Update() {
        if (enemyAvatar.gameObject.activeSelf && 
            (Time.timeSinceLevelLoad - timeSinceLastHealthRefresh > durationHealthDisplayed)) {
            HideEnemyHealthbar();
        }
    }

    public void ShowOptions() {
        if (!optionsScreen.gameObject.activeSelf) {
            Time.timeScale = 0f;
            optionsScreen.gameObject.SetActive(true);
            timeSinceOptionsShown = Time.realtimeSinceStartup;
        }
    }

    public void SetBossMode(BaseCharacterController enemy, EnemyController.Type enemyType) {
        isBossMode = true;
        NotifyEnemyHealthChange(enemy, enemyType);
    }

    public void RemoveBossMode() {
        isBossMode = false;
        HideEnemyHealthbar();
    }

    public void NotifyEnemyHealthChange(BaseCharacterController enemy, EnemyController.Type enemyType) {
        EnemySO enemySO = enemyData.Find(e => e.enemyType == enemyType);
        Rect spriteRect = new Rect(0.0f, 0.0f, enemySO.avatarImage.width, enemySO.avatarImage.height);
        enemyAvatar.sprite = Sprite.Create(enemySO.avatarImage, spriteRect, Vector2.zero, 1f);
        enemyHealthbar.RefreshMeter(enemy.MaxHP, enemy.CurrentHP);
        enemyAvatar.gameObject.SetActive(true);
        enemyHealthbar.gameObject.SetActive(true);
        timeSinceLastHealthRefresh = Time.timeSinceLevelLoad;
    }

    public void NotifyHeroHealthChange(PlayerController player) {
        heroHealthbar.RefreshMeter(player.MaxHP, player.CurrentHP);
    }

    public void NotifyGoGoGo() {
        goIndicatorAnimator.gameObject.SetActive(true);
        goIndicatorAnimator.SetTrigger("Flash");
        SoundManager.Instance.Play(SoundManager.SoundType.Gogogo);
    }

    private void HideEnemyHealthbar() {
        if (!isBossMode) {
            enemyAvatar.gameObject.SetActive(false);
            enemyHealthbar.gameObject.SetActive(false);
        }
    }
}
