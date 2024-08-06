using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public event EventHandler OnComplete;

    [SerializeField] private int maxEnemiesAtOnce = 2;
    [field:SerializeField] public int CameraLockTargetX { get; private set; }
    
    private Queue<EnemyController> enemiesLeft = new Queue<EnemyController>();
    private List<EnemyController> activeEnemies = new List<EnemyController>();

    private void Start()
    {
        foreach (EnemyController enemy in GetComponentsInChildren<EnemyController>()) {
            enemiesLeft.Enqueue(enemy);
            enemy.CheckForGarageInitialPosition();
            enemy.CheckForRoofInitialPosition();
            if (enemy.transform.position.x < CameraLockTargetX) {
                enemy.gameObject.SetActive(false);
            } else {
                enemy.GetComponent<EnemyController>().enabled = false;
            }
        }
    }

    public void Run() {
        for (int i=0; i<maxEnemiesAtOnce; i++) {
            TryActivateNewEnemy();
        }
    }

    private void TryActivateNewEnemy() {
        if (activeEnemies.Count < maxEnemiesAtOnce && enemiesLeft.Count > 0) {
            EnemyController enemy = enemiesLeft.Dequeue();
            enemy.OnDeath += OnEnemyDeath;
            enemy.gameObject.SetActive(true);
            enemy.GetComponent<EnemyController>().enabled = true;
            activeEnemies.Add(enemy);
        }
    }

    private void OnEnemyDeath(object sender, System.EventArgs e) {
        activeEnemies.Remove((EnemyController)sender);
        TryActivateNewEnemy();
        if (activeEnemies.Count == 0) {
            OnComplete?.Invoke(this, EventArgs.Empty);
        }
    }

}
