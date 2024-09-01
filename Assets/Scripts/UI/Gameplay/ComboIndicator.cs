using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ComboIndicator : MonoBehaviour
{
    [SerializeField] private Transform multiplierIndicator;
    [SerializeField] private Counter comboIndicator;
    [SerializeField] private Counter scoreIndicator;
    [SerializeField] private float comboAttackMaxDuration; // s to perform combo

    private int currentComboMultiplier = 0;
    private float timeSinceLastComboIncrease = float.NegativeInfinity;

    public static ComboIndicator Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        RefreshCounter();
    }

    private void Update() {
        if (currentComboMultiplier > 0 && (Time.timeSinceLevelLoad - timeSinceLastComboIncrease > comboAttackMaxDuration)) {
            ResetCombo();
        }
    }

    public void ResetCombo() {
        if (currentComboMultiplier > 0) {
            var points = Mathf.FloorToInt((currentComboMultiplier * (currentComboMultiplier + 1)) / 2f);
            scoreIndicator.Add(points);
        }
        currentComboMultiplier = 0;
        RefreshCounter();
    }

    public void IncreaseCombo() {
        timeSinceLastComboIncrease = Time.timeSinceLevelLoad;
        currentComboMultiplier += 1;
        RefreshCounter();
    }

    private void RefreshCounter() {
        if (currentComboMultiplier > 0) {
            comboIndicator.gameObject.SetActive(true);
            multiplierIndicator.gameObject.SetActive(true);
            comboIndicator.SetValue(currentComboMultiplier);
        } else {
            comboIndicator.gameObject.SetActive(false);
            multiplierIndicator.gameObject.SetActive(false);
        }
    }

}
