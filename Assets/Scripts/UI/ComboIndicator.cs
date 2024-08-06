using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ComboIndicator : MonoBehaviour
{
    [SerializeField] Transform multiplierIndicator;
    [SerializeField] Counter comboIndicator;
    [SerializeField] Counter scoreIndicator;

    private int currentComboMultiplier = 0;

    public static ComboIndicator Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        RefreshCounter();
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
