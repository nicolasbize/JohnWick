using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private CharacterController character;
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private SpriteRenderer meterSprite;

    private int size = 0;

    private void Start() {
        size = character.MaxHP;
        backgroundSprite.gameObject.transform.localScale = new Vector3(size, 1, 1);
        backgroundSprite.gameObject.transform.localPosition = new Vector3(-size / 2, 0, 0);
        meterSprite.gameObject.transform.localScale = new Vector3(size, 1, 1);
        meterSprite.gameObject.transform.localPosition = new Vector3(-size / 2, 0, 0);
        character.OnHealthChange += Character_OnHealthChange;
        RefreshMeter();
    }

    private void Character_OnHealthChange(object sender, System.EventArgs e) {
        RefreshMeter();
    }

    private void RefreshMeter() {
        if (gameObject != null) {
            if (character.CurrentHP <= 0) {
                character.OnHealthChange -= Character_OnHealthChange;
                meterSprite.gameObject.transform.localScale = new Vector3(0, 1, 1);
                Destroy(gameObject);
            } else {
                meterSprite.gameObject.transform.localScale = new Vector3(character.CurrentHP, 1, 1);
            }
        }
    }
}
