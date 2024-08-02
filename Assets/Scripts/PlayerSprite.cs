using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
    public event EventHandler OnAttackAnimationComplete;

    public void OnAttackAnimationEnd() {
        OnAttackAnimationComplete?.Invoke(this, null);
    }

}
