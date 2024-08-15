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
    [SerializeField] private Options optionsScreen;
    [SerializeField] private ScoreScreen scoreScreen;
    [SerializeField] private Counter score;

    private float timeSinceLastHealthRefresh = float.NegativeInfinity;
    private bool isBossMode = false;
    private bool isGameOver = false;

    public static UI Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        continueScreen.gameObject.SetActive(false);
        HideEnemyHealthbar();
        player.OnDeath += OnPlayerDeath;
        continueScreen.OnContinue += OnContinueGame;
        continueScreen.OnGameOver += OnGameOver;
        scoreScreen.OnDismiss += OnDismissScore;
        goIndicatorAnimator.gameObject.SetActive(false);
        optionsScreen.OnDismiss += OnOptionsDismiss;
        score.SetValue(PlayerPrefs.GetInt(PrefsHelper.SCORE, 0));
    }

    private void OnOptionsDismiss(object sender, System.EventArgs e) {
        optionsScreen.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnDismissScore(object sender, System.EventArgs e) {
        if (isGameOver) {
            // Fade out music
            SceneManager.LoadScene(SceneHelper.MENU_SCENE, LoadSceneMode.Single);
        }
    }

    private void OnGameOver(object sender, System.EventArgs e) {
        continueScreen.gameObject.SetActive(false);
        PlayerPrefs.SetInt(PrefsHelper.SCORE, score.GetValue());
        PlayerPrefs.SetInt(PrefsHelper.HEALTH, 0);
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

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Time.timeScale = 0f;
            optionsScreen.gameObject.SetActive(true);
            optionsScreen.RefreshOptions();
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
