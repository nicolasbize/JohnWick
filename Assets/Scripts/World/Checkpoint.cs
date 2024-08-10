using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public event EventHandler OnComplete;

    [SerializeField] private int maxEnemiesAtOnce = 2;
    [SerializeField] private BaseCharacterController boss;
    [field:SerializeField] public int CameraLockTargetX { get; private set; }

    private bool isCompleted = false;

    private Queue<EnemyController> enemiesLeft = new Queue<EnemyController>();
    private List<EnemyController> activeEnemies = new List<EnemyController>();

    private void Start()
    {
        foreach (EnemyController enemy in GetComponentsInChildren<EnemyController>()) {
            enemiesLeft.Enqueue(enemy);
            enemy.InitializeFromCheckpoint(this);
        }
    }

    public void Run() {
        if (boss != null) {
            // boss stage
            ((IBoss) boss).Activate();
        }
        for (int i=0; i<maxEnemiesAtOnce; i++) {
            TryActivateNewEnemy();
        }
    }

    private void TryActivateNewEnemy() {
        if (activeEnemies.Count < maxEnemiesAtOnce && enemiesLeft.Count > 0) {
            EnemyController enemy = enemiesLeft.Dequeue();
            enemy.OnDying += OnEnemyDying;
            enemy.OnDeath += OnEnemyDeath;
            enemy.ActivateFromCheckpoint();
            activeEnemies.Add(enemy);
        }
    }

    private void OnEnemyDying(object sender, EventArgs e) {
        activeEnemies.Remove((EnemyController)sender);
        TryActivateNewEnemy();
    }

    private void OnEnemyDeath(object sender, System.EventArgs e) {
        if (activeEnemies.Count == 0 && !isCompleted) {
            OnComplete?.Invoke(this, EventArgs.Empty);
            isCompleted = true;
        }
    }

}
