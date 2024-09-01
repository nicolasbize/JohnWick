using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerClickHandler {

    public event EventHandler OnClick;

    public void OnPointerClick(PointerEventData eventData) {
        OnClick?.Invoke(this, EventArgs.Empty);
    }
}
