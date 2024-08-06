using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private bool isRightAligned;
    [SerializeField] private BaseCharacterController character;
    [SerializeField] private RectTransform leftSide;
    [SerializeField] private RectTransform rightSide;
    [SerializeField] private RectTransform topBorder;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform meter;

    public void RefreshMeter(int maxHp, int currentHp) {
        if (currentHp < 0) { currentHp = 0; }
        topBorder.GetComponent<RectTransform>().localScale = new Vector3(maxHp, 1, 1);
        background.GetComponent<RectTransform>().localScale = new Vector3(maxHp, 1, 1);
        meter.GetComponent<RectTransform>().localScale = new Vector3(currentHp, 1, 1);
        GetComponent<RectTransform>().sizeDelta = new Vector2(maxHp + 2, 4);
        if (isRightAligned) {
            meter.GetComponent<RectTransform>().anchoredPosition = new Vector3(maxHp - currentHp + 1, -1, 0);
            rightSide.GetComponent<RectTransform>().anchoredPosition = new Vector3(maxHp + 1, 0, 0);
        } else {
            rightSide.GetComponent<RectTransform>().anchoredPosition = new Vector3(maxHp + 1, 0, 0);
        }
        
    }
}
