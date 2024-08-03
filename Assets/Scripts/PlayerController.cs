using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterSprite sprite;
    [SerializeField] private float comboAttackMaxDuration = 0.2f; // s to perform combo

    public enum State { Idle, Walking, Attacking, Hurt }

    private State state = State.Idle;
    private Vector2 speed;
    private Vector2 position;
    private List<string> comboAttackTriggers = new List<string>() {
        "Punch", "Punch", "Kick", "Roundhouse"
    };
    private int currentComboIndex = -1;
    private float timeLastAttack = float.NegativeInfinity;
    private float zHeight = 0f;
    private float dzHeight = 0f;
    private bool grounded = true;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite.OnAttackAnimationComplete += Sprite_OnAttackAnimationComplete;
        sprite.OnInvincibilityEnd += Sprite_OnInvincibilityEnd;
        position = new Vector2(transform.position.x, transform.position.y);
    }

    private void Sprite_OnInvincibilityEnd(object sender, EventArgs e) {
        state = State.Idle;
    }

    public void ReceiveHitFromEnemy() {
        if (IsVulnerable()) {
            state = State.Hurt;
            animator.SetTrigger("Hurt");
        }
    }

    public bool IsVulnerable() {
        return state != State.Hurt;
    }

    private void Sprite_OnAttackAnimationComplete(object sender, System.EventArgs e) {
        timeLastAttack = Time.timeSinceLevelLoad;
        state = State.Idle;
    }

    private void FixedUpdate() {
        if (CanJump() && Input.GetButton("Jump")) {
            dzHeight = jumpForce;
            grounded = false;
            animator.SetTrigger("Jump");
        }

        if (!grounded) {
            dzHeight -= gravity * Time.deltaTime;
            zHeight += dzHeight;
            if (zHeight < 0f) {
                grounded = true;
                state = State.Idle;
                zHeight = 0f;
                animator.SetTrigger("Land");
            }
        }

        sprite.gameObject.transform.localPosition = Vector3.up * Mathf.RoundToInt(zHeight);

        if (CanMove()) {
            speed = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * moveSpeed;
            position += speed * Time.deltaTime;
            transform.position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), 0);

            //rb.velocity = speed * Time.deltaTime;
            if (speed != Vector2.zero) {
                state = State.Walking;
                if (speed.x != 0f) {
                    sprite.GetComponent<SpriteRenderer>().flipX = speed.x < 0;
                }
            } else {
                state = State.Idle;
            }
            animator.SetBool("IsWalking", speed != Vector2.zero);
        } else {
            rb.velocity = Vector2.zero;
        }

        if (CanAttack() && Input.GetButtonDown("Attack")) {
            state = State.Attacking;
            if (!grounded) {
                animator.SetTrigger("AirKick");
            } else {
                if ((Time.timeSinceLevelLoad - timeLastAttack) < comboAttackMaxDuration) {
                    currentComboIndex = (currentComboIndex + 1) % (comboAttackTriggers.Count);
                } else {
                    currentComboIndex = 0;
                }
                animator.SetTrigger(comboAttackTriggers[currentComboIndex]);
            }
        }
    }

    public bool IsFlipped() {
        return sprite.GetComponent<SpriteRenderer>().flipX;
    }

    private bool CanAttack() {
        return state != State.Attacking;
    }

    private bool CanMove() {
        return (state != State.Attacking) || (!grounded);
    }

    private bool CanJump() {
        return state != State.Attacking && grounded;
    }

}
