using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionScreen : MonoBehaviour
{
    public event EventHandler OnReadyToLoadContent;
    public event EventHandler OnReadyToPlay;

    private Animator animator;

    public static TransitionScreen Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        animator = GetComponent<Animator>();
    }

    public void StartTransition() {
        animator.SetTrigger("HidePlayArea");
        Debug.Log("hide");
    }

    public void FinishTransition() {
        animator.SetTrigger("RevealPlayArea");
        Debug.Log("reveal");
    }

    public void OnScreenHiddenFrame() {
        OnReadyToLoadContent?.Invoke(this, EventArgs.Empty);
    }

    public void OnScreenShownFrame() {
        OnReadyToPlay?.Invoke(this, EventArgs.Empty);
    }

}
