using System;
using UnityEngine;

public class TransitionScreen : MonoBehaviour
{
    public event EventHandler OnReadyToLoadContent;
    public event EventHandler OnReadyToPlay;

    private Animator animator;

    public static TransitionScreen Instance;

    private void Awake() {
        Instance = this;
        animator = GetComponent<Animator>();
    }

    public void StartTransition() {
        animator.SetTrigger("HidePlayArea");
    }

    public void FinishTransition() {
        animator.SetTrigger("RevealPlayArea");
    }

    public void OnScreenHiddenFrame() {
        OnReadyToLoadContent?.Invoke(this, EventArgs.Empty);
    }

    public void OnScreenShownFrame() {
        OnReadyToPlay?.Invoke(this, EventArgs.Empty);
    }

}
