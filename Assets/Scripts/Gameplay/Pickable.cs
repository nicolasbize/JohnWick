using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Pickable : MonoBehaviour
{
    public enum PickableType { Knife, Gun, Food }

    [field: SerializeField] public bool IsFalling { get; set; }
    [field: SerializeField] public bool IsPickable { get; private set; }
    [field: SerializeField] public PickableType Type { get; private set; }
    [SerializeField] protected float gravity = 5f;
    private Animator animator;
    private float height = 0f;
    private float dzHeight = 0f;
    private Vector2 precisePosition;
    void Start()
    {
        animator = GetComponent<Animator>();
        IsPickable = !IsFalling;
        if (IsFalling) {
            animator.SetTrigger("Fall");
            height = 10f;
        }
        precisePosition = transform.position;
        transform.position = new Vector3(transform.position.x, transform.position.z + height, 0);
    }

    private void FixedUpdate() {
        if (height > 0) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            transform.position = new Vector3(Mathf.FloorToInt(precisePosition.x), Mathf.FloorToInt(precisePosition.y + height), Mathf.FloorToInt(precisePosition.y));
            if (height < 0) {
                IsPickable = true;
                height = 0f;
            }
        }
    }

    public void OnHitGround() {
        IsPickable = true;
    }

    public void PickupItem() {
        Destroy(gameObject);
    }
}
