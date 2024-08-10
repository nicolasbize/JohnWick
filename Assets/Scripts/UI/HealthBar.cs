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
    [SerializeField] private RectTransform supplementalMeter;

    private const int maxSize = 17;

    public void RefreshMeter(int maxHp, int currentHp) {
        if (currentHp < 0) { currentHp = 0; }
        int extraHealth = 0;
        if (currentHp > maxSize) extraHealth = currentHp - maxSize;
        int cappedHealth = Mathf.Min(currentHp, maxSize);
        if (maxHp > maxSize) maxHp = maxSize;

        topBorder.GetComponent<RectTransform>().localScale = new Vector3(maxHp, 1, 1);
        background.GetComponent<RectTransform>().localScale = new Vector3(maxHp, 1, 1);
        meter.GetComponent<RectTransform>().localScale = new Vector3(cappedHealth, 1, 1);
        if (extraHealth > 0) {
            supplementalMeter.GetComponent<RectTransform>().localScale = new Vector3(extraHealth, 1, 1);
        } else {
            supplementalMeter.GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
        }
        GetComponent<RectTransform>().sizeDelta = new Vector2(maxHp + 2, 4);

        if (isRightAligned) {

            meter.GetComponent<RectTransform>().anchoredPosition = new Vector3(maxHp - cappedHealth + 1, -1, 0);
            supplementalMeter.GetComponent<RectTransform>().anchoredPosition = new Vector3(maxHp - extraHealth + 1, -1, 0);
            rightSide.GetComponent<RectTransform>().anchoredPosition = new Vector3(maxHp + 1, 0, 0);
            
        } else {
            rightSide.GetComponent<RectTransform>().anchoredPosition = new Vector3(Mathf.Min(maxHp, maxSize) + 1, 0, 0);
        }
        
    }
}
