using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
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
        transform.position = new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0f);

    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision != null && collision.gameObject.GetComponent<BaseCharacterController>() != null && 
            collision.gameObject != Emitter.gameObject) {
            BaseCharacterController characterController = collision.GetComponent<BaseCharacterController>();
            if (characterController.IsVulnerable(position)) {
                characterController.ReceiveHit(position, damage, Hit.Type.Knockdown);
            }
            Destroy(gameObject);
        }
    }

}
