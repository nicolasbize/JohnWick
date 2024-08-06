using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] private List<Transform> numbers;
    [SerializeField] private bool showInitialZero;

    private int currentValue;

    private void Start() {
        Refresh();
    }

    public void Add(int amount) {
        currentValue += amount;
        Refresh();
    }

    public void SetValue(int newValue) {
        currentValue = newValue;
        Refresh();
    }

    public void Refresh() {
        var number = currentValue;
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        if (currentValue > 0) {
            while (number > 0) {
                int digit = number % 10;
                number = (number - digit) / 10;
                Transform instance = Instantiate(numbers[digit]);
                instance.SetParent(transform);
            }
        } else if (showInitialZero) {
            Transform instance = Instantiate(numbers[0]);
            instance.SetParent(transform);
        }
    }
}
