using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseCharacterController {

    [SerializeField] private float jumpForce;
    [SerializeField] private float comboAttackMaxDuration; // s to perform combo

    public static PlayerController Instance;

    private void Awake() {
        Instance = this;
    }

    private List<string> comboAttackTriggers = new List<string>() {
        "Punch", "Punch", "PunchAlt", "Kick", "Roundhouse"
    };
    private int currentComboIndex = 0;
    private List<EnemyController> enemies = new List<EnemyController>();

    public void RegisterEnemy(EnemyController enemy) {
        enemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyController enemy) {
        enemies.Remove(enemy);
    }


    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            Vector2 attackVector = damageOrigin.x < position.x ? Vector2.right : Vector2.left;
            ReceiveDamage(dmg);
            if (hitType == Hit.Type.Knockdown || (CurrentHP <= 0)) {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                velocity = attackVector * moveSpeed * 1.5f;
                dzHeight = 1f;
            } else {
                velocity = attackVector * moveSpeed;
                state = State.Hurt;
                animator.SetTrigger("Hurt");
            }
            UI.Instance.NotifyHeroHealthChange(this);
            BreakCombo();
        }
    }

    public override bool IsVulnerable(Vector2 damageOrigin, bool canBlock = true) {
        if (state == State.Hurt || state == State.Falling || state == State.Grounded || state == State.Dying || state == State.Dead) {
            return false;
        }
        if (Time.timeSinceLevelLoad - timeSinceGrounded < durationGrounded + durationInvincibleAfterGettingUp) {
            return false;
        }

        if (canBlock && state == State.Blocking && (
            (IsFacingLeft && damageOrigin.x < position.x) ||
            (!IsFacingLeft && damageOrigin.x > position.x))) {
            return false;
        }
        return true;
    }


    private void Sprite_OnInvincibilityEnd(object sender, EventArgs e) {
        state = State.Idle;
    }

    private void Sprite_OnAttackAnimationComplete(object sender, System.EventArgs e) {
        timeLastAttack = Time.timeSinceLevelLoad;
        state = State.Idle;
    }

    protected override void MaybeInductDamage() {
        bool hasHitEnemy = false;
        // get list of vulnerable enemies within distance.
        foreach (EnemyController enemy in enemies) {
            // they need to be facing the direction of the hit
            bool isInFrontOfPlayer = false;
            if (IsFacingLeft && enemy.transform.position.x < transform.position.x) {
                isInFrontOfPlayer = true;
            }
            if (!IsFacingLeft && enemy.transform.position.x > transform.position.x) {
                isInFrontOfPlayer = true;
            }

            // they need to be within distance in the right axis
            bool isAlignedWithPlayer = false;
            bool isYAligned = Mathf.Abs(enemy.transform.position.y - transform.position.y) < verticalMarginBetweenEnemyAndPlayer;
            bool isXAligned = Mathf.Abs(enemy.transform.position.x - transform.position.x) < attackReach + 1;
            isAlignedWithPlayer = isYAligned && isXAligned;
            if (isAlignedWithPlayer && isInFrontOfPlayer && enemy.IsVulnerable(transform.position)) {
                bool isPowerAttack = currentComboIndex == comboAttackTriggers.Count - 1;
                Hit.Type hitType = isPowerAttack ? Hit.Type.PowerEject : Hit.Type.Normal;
                if (!grounded) {
                    hitType = Hit.Type.Knockdown;
                }
                int damage = isPowerAttack ? 2 : 4;
                enemy.ReceiveHit(position, damage, hitType);
                hasHitEnemy = true;
            }
        }
        
        // increment combo
        if (hasHitEnemy) {
            if (Time.timeSinceLevelLoad - timeLastAttack < comboAttackMaxDuration) {
                currentComboIndex = (currentComboIndex + 1) % comboAttackTriggers.Count;
                ComboIndicator.Instance.IncreaseCombo();
            } else {
                BreakCombo();
            }
            timeLastAttack = Time.timeSinceLevelLoad;
        } else {
            // don't reset combo but start over
            currentComboIndex = 0;
        }
    }

    private void BreakCombo() {
        currentComboIndex = 1; // don't start at zero since this is the first hit
        ComboIndicator.Instance.ResetCombo();
    }

    protected override void FixedUpdate() {
        bool wasFacingLeft = IsFacingLeft;
        HandleDropping();
        HandleJumpInput();
        HandleBlockInput();
        HandleMoveInput();
        HandleAttackInput();
        HandleFalling();
        HandleGrounded();
        HandleDying();

        if (IsFacingLeft != wasFacingLeft) {
            NotifyChangeDirection();
        }
        RestrictScreenBoundaries();

        if (Time.timeSinceLevelLoad - timeLastAttack > comboAttackMaxDuration) {
            BreakCombo();
        }
    }

    public void Respawn() {
        state = State.Idle;
        grounded = false;
        animator.SetBool("IsJumping", true);
        Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        float midX = Mathf.FloorToInt((screenBoundaries.y - screenBoundaries.x) / 2f);
        position = new Vector2(midX, -10f);
        zHeight = 50;
        CurrentHP = MaxHP;
        UI.Instance.NotifyHeroHealthChange(this);
        foreach (EnemyController enemy in enemies) {
            if (enemy.state != State.WaitingForPlayer) {
                enemy.ReceiveHit(position, 0, Hit.Type.Knockdown);
            }
        }
    }

    private void RestrictScreenBoundaries() {
        Vector2 xBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        if (position.x <  xBoundaries.x) {
            position.x = xBoundaries.x;
        } else if (position.x > xBoundaries.y) {
            position.x = xBoundaries.y;
        }
        transform.position = new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0);
    }

    private void HandleJumpInput() {
        if (CanJump() && Input.GetButton("Jump")) {
            dzHeight = jumpForce;
            grounded = false;
            animator.SetBool("IsJumping", true);
        }

        if (!grounded) {
            dzHeight -= gravity * Time.deltaTime;
            zHeight += dzHeight;
            if (zHeight < 0f) {
                grounded = true;
                state = State.Idle;
                zHeight = 0f;
                animator.SetBool("IsJumping", false);
            }
        }

        characterSprite.gameObject.transform.localPosition = Vector3.up * Mathf.FloorToInt(zHeight);
    }

    private void HandleBlockInput() {
        if (CanBlock() && Input.GetButton("Block")) {
            state = State.Blocking;
        }
        if (state == State.Blocking && !Input.GetButton("Block")) {
            state = State.Idle;
        }
        animator.SetBool("IsBlocking", state == State.Blocking);
    }

    private void HandleAttackInput() {
        if (CanAttack() && Input.GetButtonDown("Attack")) {
            state = State.Attacking;
            if (!grounded) {
                currentComboIndex = 0;
                animator.SetTrigger("AirKick");
            } else {
                if (HasKnife) {
                    animator.SetTrigger("PunchAlt");
                    ThrowKnife();
                } else {
                    animator.SetTrigger(comboAttackTriggers[currentComboIndex]);
                }
            }
        }
    }

    private void HandleMoveInput() {
        if (CanMove()) {
            velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * moveSpeed;
            TryMoveTo(position + velocity * Time.deltaTime);

            if (velocity != Vector2.zero) {
                state = State.Walking;
                if (velocity.x != 0f) {
                    characterSprite.flipX = velocity.x < 0;
                    knifeTransform.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
                    IsFacingLeft = characterSprite.flipX;
                }
            } else {
                state = State.Idle;
            }
            animator.SetBool("IsWalking", velocity != Vector2.zero);
        }
    }

}
