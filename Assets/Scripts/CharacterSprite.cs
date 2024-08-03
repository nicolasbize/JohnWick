using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterSprite : MonoBehaviour
{
    public event EventHandler OnAttackAnimationComplete;
    public event EventHandler OnAttackFrame;
    public event EventHandler OnInvincibilityEnd;


    public void OnAttackAnimationEnd() {
        OnAttackAnimationComplete?.Invoke(this, null);
    }

    public void OnInvincibilityFrameEnd() {
        OnInvincibilityEnd?.Invoke(this, null);
    }

    public void OnAttackFrameEvent() {
        OnAttackFrame?.Invoke(this, null);
    }

}
