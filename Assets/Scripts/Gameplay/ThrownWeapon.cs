using UnityEngine;

public class ThrownWeapon : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float zHitBuffer;
    [SerializeField] private float height;
    [field:SerializeField] public Vector2 Direction { get; set; }
    [field:SerializeField] public BaseCharacterController Emitter { get; set; }
    [SerializeField] private SpriteRenderer weaponSprite;

    private Vector2 position;

    void Start()
    {
        position = new Vector2(transform.position.x, transform.position.y);
        height = 8;
    }

    void Update()
    {
        weaponSprite.flipX = Direction.x < 0;
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
        float zKnife = transform.position.z;
        float zObject = gameObject.transform.position.z;
        return (zKnife > zObject - zHitBuffer) && (zKnife < zObject + zHitBuffer);
    }

}
