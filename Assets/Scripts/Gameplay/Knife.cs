using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Knife : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float yHitBuffer;
    [SerializeField] private float height;
    [field:SerializeField] public Vector2 Direction { get; set; }
    [field:SerializeField] public BaseCharacterController Emitter { get; set; }
    [SerializeField] private SpriteRenderer knifeSprite;

    private Vector2 position;

    void Start()
    {
        position = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        knifeSprite.flipX = Direction.x < 0;
        position += speed * Direction * Time.deltaTime;
        transform.position = new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y + height), Mathf.FloorToInt(position.y));

        if (!WithinBoundaries(Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries())) {
            Destroy(gameObject);
        }
    }

    private bool WithinBoundaries(Vector2 boundaries) {
        float padding = 16f;
        return (position.x > boundaries.x - padding) && (position.x < boundaries.y + padding);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision != null && collision.gameObject.GetComponent<BaseCharacterController>() != null && 
            collision.gameObject != Emitter.gameObject) {
            BaseCharacterController characterController = collision.GetComponent<BaseCharacterController>();
            if (characterController.IsVulnerable(position, false) && IsAlignedWith(collision.gameObject)) {
                int realDamage = damage;
                if (characterController is PlayerController) {
                    realDamage = Mathf.FloorToInt(damage / 2f);
                }
                characterController.ReceiveHit(position, realDamage, Hit.Type.Knockdown);
                Destroy(gameObject);
            }
            
        }
    }

    private bool IsAlignedWith(GameObject gameObject) {
        float yKnife = transform.position.y + 20; // lots of empty space due to unity weird sorting algos
        float yObject = gameObject.transform.position.y;
        return (yKnife > yObject - yHitBuffer) && (yKnife < yObject + yHitBuffer);
    }

}
