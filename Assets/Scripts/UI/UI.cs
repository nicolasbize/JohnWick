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

    private float timeSinceLastHealthRefresh = float.NegativeInfinity;

    public static UI Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        HideEnemyHealthbar();
    }

    private void Update() {
        if (enemyAvatar.gameObject.activeSelf && 
            (Time.timeSinceLevelLoad - timeSinceLastHealthRefresh > durationHealthDisplayed)) {
            HideEnemyHealthbar();
        }
    }

    public void NotifyEnemyHealthChange(EnemyController enemy) {
        EnemySO enemySO = enemyData.Find(e => e.enemyType == enemy.EnemySO.enemyType);
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
        goIndicatorAnimator.SetTrigger("Flash");
    }

    private void HideEnemyHealthbar() {
        enemyAvatar.gameObject.SetActive(false);
        enemyHealthbar.gameObject.SetActive(false);
    }
}
