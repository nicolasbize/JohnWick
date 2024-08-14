using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{

    public event EventHandler OnDismiss;
    private Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    public void Activate() {
        animator.SetBool("IsRolling", true);
    }

    private void Update() {
        animator.SetBool("IsRolling", false);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Attack")) {
            OnDismiss?.Invoke(this, EventArgs.Empty);
        }
    }

}
