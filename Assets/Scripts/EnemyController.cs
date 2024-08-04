using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : BaseCharacterController {
    
    public enum EnemyType { Biker, Goon, Punk, StreetBoss, Thug}

    [SerializeField] private float flySpeed;
    [SerializeField] private Vector2 minMaxSecsBeforeHitting;
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyType enemyType;
    

    private float timeSincePreparedToHit = float.NegativeInfinity;
    private float waitDurationBeforeHit = 0f;
    private float timeSinceGrounded = float.NegativeInfinity;
    private bool isInHittingStance = false;

    protected override void Start() {
        base.Start();
        player.RegisterEnemy(this);
    }

    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            Vector2 attackVector = damageOrigin.x < position.x ? Vector2.right : Vector2.left;
            isInHittingStance = false; // knocks player out a bit
            if (hitType == Hit.Type.PowerEject) {
                animator.SetBool("IsFlying", true);
                state = State.Flying;
                velocity = attackVector * flySpeed;
            } else if (hitType == Hit.Type.Knockdown || (CurrentHP <= 0)) {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                velocity = attackVector * moveSpeed;
                dzHeight = 2f;
            } else {
                state = State.Hurt;
                animator.SetTrigger("Hurt");
            }
            ReceiveDamage(dmg);
        }
    }

    public override bool IsVulnerable(Vector2 damageOrigin) {
        return state == State.Idle ||
               state == State.Walking ||
               state == State.PreparingAttack;
    }

    protected override void AttemptAttack() {
        if (IsPlayerWithinReach() && player.IsVulnerable(position)) {
            player.ReceiveHit(position);
        }
        isInHittingStance = false; // take a breather
    }

    protected override void FixedUpdate() {
        HandleFlying();
        HandleFalling();
        HandleGrounded();
        HandleAttack();
        
        characterSprite.gameObject.transform.localPosition = Vector3.up * Mathf.RoundToInt(zHeight);

        if (CanMove()) {
            FacePlayer();
            Vector2 nextTargetDestination = GetNextMovementDirection();
            bool isPlayerTooFar = nextTargetDestination.magnitude > 0;
            animator.SetBool("IsWalking", isPlayerTooFar);
            if (isPlayerTooFar) {
                WalkTowards(nextTargetDestination);
                isInHittingStance = false;
            } else if (!isPlayerTooFar && !isInHittingStance) {
                isInHittingStance = true;
                state = State.PreparingAttack;
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
        if (state == State.Grounded) {
            if (CurrentHP > 0 && (Time.timeSinceLevelLoad - timeSinceGrounded > durationGrounded)) {
                animator.SetTrigger("GetUp");
                state = State.Idle;
            } else if (CurrentHP <= 0 && (Time.timeSinceLevelLoad - timeSinceGrounded > durationGrounded)) {
                player.UnregisterEnemy(this);
                Destroy(gameObject);
            }
        }
    }

    private void HandleAttack() {
        if (state == State.PreparingAttack && 
            (Time.timeSinceLevelLoad - timeSincePreparedToHit > waitDurationBeforeHit)) {

            state = State.Attacking;
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
        state = State.Walking;
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

    private void FacePlayer() {
        if (player != null) {
            characterSprite.flipX = player.transform.position.x < transform.position.x;
            IsFacingLeft = characterSprite.flipX;
        }
    }
}
