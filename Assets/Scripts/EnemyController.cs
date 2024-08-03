using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float flySpeed;
    [SerializeField] private Vector2 minMaxSecsBeforeHitting;
    [SerializeField] private PlayerController player;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackReach;
    [SerializeField] private float durationGrounded;
    [SerializeField] private CharacterSprite sprite;
    [SerializeField] private Animator animator;
    [SerializeField] private float gravity = 10f;


    enum State { Idle, Walk, PrepareAttack, Attack, Hurt, Flying, Falling, Grounded }

    private Vector2 position;
    private Vector2 velocity;
    private State state = State.Idle;
    private float timeSincePreparedToHit = float.NegativeInfinity;
    private float waitDurationBeforeHit = 0f;
    private float verticalMarginBetweenEnemyAndPlayer = 4;
    private float timeSinceGrounded = float.NegativeInfinity;
    private float zHeight = 0f;
    private float dzHeight = 0f;
    private bool wasPlayerInReach = false;

    void Start() {
        position = new Vector2(transform.position.x, transform.position.y);
        sprite.OnAttackAnimationComplete += Sprite_OnAttackAnimationComplete;
        sprite.OnInvincibilityEnd += Sprite_OnInvincibilityEnd;
        sprite.OnAttackFrame += Sprite_OnAttackFrame;
        player.RegisterEnemy(this);
    }

    public void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            wasPlayerInReach = false; // knocks player out a bit
            if (hitType == Hit.Type.PowerEject) {
                animator.SetBool("IsFlying", true);
                state = State.Flying;
                velocity = (damageOrigin.x < position.x ? Vector2.right : Vector2.left) * flySpeed;
            } else if (hitType == Hit.Type.Knockdown) {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                velocity = (damageOrigin.x < position.x ? Vector2.right : Vector2.left) * moveSpeed;
                dzHeight = 2f;
            } else {
                state = State.Hurt;
                animator.SetTrigger("Hurt");
            }
        }
    }

    public bool IsVulnerable(Vector2 damageOrigin) {
        return state == State.Idle ||
               state == State.Walk ||
               state == State.PrepareAttack;
    }

    private void Sprite_OnAttackFrame(object sender, System.EventArgs e) {
        if (IsPlayerWithinReach() && player.IsVulnerable(position)) {
            player.ReceiveHit(position);
        }
    }

    private void Sprite_OnInvincibilityEnd(object sender, EventArgs e) {
        state = State.Idle;
    }

    private void Sprite_OnAttackAnimationComplete(object sender, System.EventArgs e) {
        state = State.Idle; // no need for trigger, enable movement again
    }

    private void FixedUpdate()
    {
        HandleFlying();
        HandleFalling();
        HandleGrounded();
        HandleAttack();
        
        sprite.gameObject.transform.localPosition = Vector3.up * Mathf.RoundToInt(zHeight);

        if (CanMove()) {
            FacePlayer();
            Vector2 nextTargetDestination = GetNextMovementDirection();
            bool isPlayerTooFar = nextTargetDestination.magnitude > 0;
            animator.SetBool("IsWalking", isPlayerTooFar);
            if (isPlayerTooFar) {
                WalkTowards(nextTargetDestination);
                wasPlayerInReach = false;
            } else {
                state = State.PrepareAttack;
                timeSincePreparedToHit = Time.timeSinceLevelLoad;
                waitDurationBeforeHit = UnityEngine.Random.Range(minMaxSecsBeforeHitting.x, minMaxSecsBeforeHitting.y);
            }
        }
    }

    private void HandleFlying() {
        if (state == State.Flying) {
            position += velocity * Time.deltaTime;
            transform.position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), 0);
            Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
            zHeight = 5f;
            if ((transform.position.x < screenBoundaries.x + 8) ||
                (transform.position.x > screenBoundaries.y - 8)) {
                animator.SetBool("IsFlying", false);
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                dzHeight = 2f;
            }
        }
    }

    private void HandleFalling() {
        if (state == State.Falling) {
            dzHeight -= gravity * Time.deltaTime;
            zHeight += dzHeight;
            if (zHeight < 0) {
                state = State.Grounded;
                zHeight = 0f;
                timeSinceGrounded = Time.timeSinceLevelLoad;
                animator.SetBool("IsFalling", false);
            }
        }
    }

    private void HandleGrounded() {
        if ((state == State.Grounded) && (Time.timeSinceLevelLoad - timeSinceGrounded > durationGrounded)) {
            animator.SetTrigger("GetUp");
            state = State.Idle;
        }
    }

    private void HandleAttack() {
        if (state == State.PrepareAttack && 
            (Time.timeSinceLevelLoad - timeSincePreparedToHit > waitDurationBeforeHit)) {

            state = State.Attack;
            if (UnityEngine.Random.Range(0f, 1f) > 0.5f) {
                animator.SetTrigger("Punch");
            } else {
                animator.SetTrigger("PunchAlt");
            }
        }
    }

    private void WalkTowards(Vector2 targetDestination) {
        velocity = targetDestination * moveSpeed;
        position += velocity * Time.deltaTime;
        transform.position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), 0);
        state = State.Walk;
    }

    private bool IsPlayerWithinReach() {
        bool isYAligned = Mathf.Abs(player.transform.position.y - transform.position.y) < verticalMarginBetweenEnemyAndPlayer;
        bool isXAligned = Mathf.Abs(player.transform.position.x - transform.position.x) < attackReach + 1;
        return (isYAligned && isXAligned);
    }

    private Vector2 GetNextMovementDirection() {
        Vector2 target = Vector2.zero;

        if (IsPlayerWithinReach()) {
            return Vector2.zero; // no need to go any further
        }

        if (transform.position.x > player.transform.position.x) {
            target = new Vector2(player.transform.position.x + attackReach, player.transform.position.y);
        } else {
            target = new Vector2(player.transform.position.x - attackReach, player.transform.position.y);
        }
        return (target - position).normalized;
    }

    private bool CanMove() {
        return state == State.Idle ||
               state == State.Walk;
    }

    private void FacePlayer() {
        if (player != null) {
            sprite.GetComponent<SpriteRenderer>().flipX = player.transform.position.x < transform.position.x;
        }
    }
}
