using UnityEngine;
using System;
using static BaseMenuScreen;
using UnityEngine.EventSystems;

public class MovieUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private ScreenType nextScreen;

    private FadingController fader;
    private BaseMenuScreen menu;

    private void Awake() {
        fader = GetComponent<FadingController>();
        menu = GetComponent<BaseMenuScreen>();
        PlayerInputListener.Instance.OnSelectPress += OnSelectPress;
        fader.OnCompleteFade += OnReadyToDismiss;
    }

    private void OnDestroy() {
        PlayerInputListener.Instance.OnSelectPress -= OnSelectPress;
        fader.OnCompleteFade -= OnReadyToDismiss;
    }

    private void OnReadyToDismiss(object sender, EventArgs e) {
        if (nextScreen != ScreenType.None) {
            menu.SwitchScreen(nextScreen);
        } else {
            menu.CloseScreen();
        }
    }

    private void OnSelectPress(object sender, EventArgs e) {
        fader.SkipCurrentFrame();
    }

    public void OnPointerClick(PointerEventData eventData) {
        fader.SkipCurrentFrame();
    }
}
