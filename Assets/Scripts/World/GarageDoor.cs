using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageDoor : MonoBehaviour
{
    public event EventHandler OnDoorOpened;

    [SerializeField] private float speedToOpen;
    [SerializeField] private float heightOpenPosition;
    [SerializeField] private Transform closedDoor;
    [SerializeField] private Transform openedDoor;

    public bool IsOpened { get; private set; }
    public bool IsOpening { get; private set; }
    private float timeTriggered = float.NegativeInfinity;

    public void Open() {
        if (!IsOpening && !IsOpened) {
            IsOpening = true;
            timeTriggered = Time.timeSinceLevelLoad;
        }
    }

    private void Update() {
        if (IsOpening) {
            float progress = (Time.timeSinceLevelLoad - timeTriggered) / speedToOpen;
            if (progress >= 1) {
                progress = 1;
                IsOpening = false;
                IsOpened = true;
                OnDoorOpened?.Invoke(this, EventArgs.Empty);
            }
            float newY = Mathf.FloorToInt(Mathf.Lerp(0, heightOpenPosition, progress));
            closedDoor.transform.localPosition = new Vector3(0, newY, 0);
        }
    }


}
