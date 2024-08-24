using System;
using UnityEngine;

public class BaseMenuScreen : MonoBehaviour {

    public enum ScreenType { None, Splash, MainMenu, Credits, Options, Intro, Outro, Score }

    public event EventHandler<ScreenEventArgs> OnSelectScreen;
    public class ScreenEventArgs : EventArgs {
        public ScreenType selectedScreen;
    }

    public event EventHandler OnCloseScreen;

    public void SwitchScreen(ScreenType screen) {
        OnSelectScreen?.Invoke(this, new ScreenEventArgs() {
            selectedScreen = screen
        });
    }

    public void CloseScreen() {
        OnCloseScreen?.Invoke(this, EventArgs.Empty);
    }

}
