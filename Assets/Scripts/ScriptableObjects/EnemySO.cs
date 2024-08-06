using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu()]
public class EnemySO : ScriptableObject
{
    public EnemyController.Type enemyType;
    public Texture2D avatarImage;
    public int maxHealth;
    public bool isStageBoss;
}
