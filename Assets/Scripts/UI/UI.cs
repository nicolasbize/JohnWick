using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Counter score;
    [SerializeField] private AudioClip gogogoSound;

    private int highscore = 0;

    private float timeSinceLastHealthRefresh = float.NegativeInfinity;
    private AudioSource audioSource;
    private bool isBossMode = false;

    public static UI Instance;

    private void Awake() {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        continueScreen.gameObject.SetActive(false);
        HideEnemyHealthbar();
        player.OnDeath += OnPlayerDeath;
        continueScreen.OnContinue += OnContinueGame;
        continueScreen.OnGameOver += OnGameOver;
        goIndicatorAnimator.gameObject.SetActive(false);
    }

    private void OnGameOver(object sender, System.EventArgs e) {
        MaybeIncreaseHighScore();
    }

    private void OnContinueGame(object sender, System.EventArgs e) {
        MaybeIncreaseHighScore();
        continueScreen.gameObject.SetActive(false);
        // remove 300 points from current score
        score.SetValue(Mathf.Max(0, score.GetValue() - 300));
        player.Respawn();
    }

    public void AddScore(int value) {
        score.Add(value);
    }

    private void MaybeIncreaseHighScore() {
        int currentScore = score.GetValue();
        if (currentScore > highscore) {
            highscore = currentScore;
        }
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
        audioSource.PlayOneShot(gogogoSound);
    }

    private void HideEnemyHealthbar() {
        if (!isBossMode) {
            enemyAvatar.gameObject.SetActive(false);
            enemyHealthbar.gameObject.SetActive(false);
        }
    }
}
