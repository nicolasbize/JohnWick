using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public event EventHandler OnComplete;

    [SerializeField] private int maxEnemiesAtOnce;
    [SerializeField] private BaseCharacterController boss;
    [field:SerializeField] public int CameraLockTargetX { get; private set; }
    [field:SerializeField] public bool IsTransitionCheckpoint { get; private set; }

    private bool hasStarted = false;
    private bool isCompleted = false;
    private bool hasNotifiedCompletion = false;

    private void Start()
    {
        foreach (EnemyController enemy in GetComponentsInChildren<EnemyController>()) {
            enemy.InitializeFromCheckpoint(this);
        }
    }

    public void Run() {
        hasStarted = true;
        if (boss != null) {
            // boss stage
            boss.OnDeath += OnBossDeath;
            ((IBoss) boss).Activate();
        }
    }

    private void Update() {
        if (hasStarted) {
            if (boss == null) {
                List<EnemyController> enemies = new List<EnemyController>(GetComponentsInChildren<EnemyController>());
                isCompleted = GetComponentsInChildren<EnemyController>().Length == 0;
                if (!isCompleted) {
                    for (int i = 0; i < maxEnemiesAtOnce; i++) {
                        if (i < enemies.Count && !enemies[i].IsActivatedForCheckpoint) {
                            enemies[i].ActivateFromCheckpoint();
                        }
                    }
                }
            }
            if (isCompleted && !hasNotifiedCompletion) {
                hasNotifiedCompletion = true;
                OnComplete?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void OnBossDeath(object sender, EventArgs e) {
        UI.Instance.RemoveBossMode();
        OnComplete?.Invoke(this, EventArgs.Empty);
        isCompleted = true;
    }


}
