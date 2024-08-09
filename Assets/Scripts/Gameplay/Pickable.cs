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
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        IsPickable = !IsFalling;
        if (IsFalling) {
            animator.SetTrigger("Fall");
        }
    }

    public void OnHitGround() {
        IsPickable = true;
        Debug.Log("ispickable");
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (IsPickable && collision != null && collision.gameObject.GetComponent<PlayerController>() != null) {
            collision.gameObject.GetComponent<PlayerController>().PickableItem = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {

        if (IsPickable && collision != null && collision.gameObject.GetComponent<PlayerController>() != null) {
            collision.gameObject.GetComponent<PlayerController>().PickableItem = null;
        }
    }

    public void PickupItem() {
        Destroy(gameObject);
    }
}
