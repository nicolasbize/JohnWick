using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseCharacterController {

    [SerializeField] private float jumpForce;
    [SerializeField] private float comboAttackMaxDuration; // s to perform combo

    public Pickable PickableItem { get; set; }

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
            Vector2 attackVector = damageOrigin.x < precisePosition.x ? Vector2.right : Vector2.left;
            ReceiveDamage(dmg);
            if (hitType == Hit.Type.Knockdown || (CurrentHP <= 0)) {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                preciseVelocity = attackVector * 50f;
                dzHeight = 1f;
                Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 1);
                GenerateSparkFX();
            } else {
                preciseVelocity = attackVector * (moveSpeed / 2f);
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
            (IsFacingLeft && damageOrigin.x < precisePosition.x) ||
            (!IsFacingLeft && damageOrigin.x > precisePosition.x))) {
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
                enemy.ReceiveHit(precisePosition, damage, hitType);
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

    protected override void Update() {
        bool wasFacingLeft = IsFacingLeft;
        HandleDropping();
        HandleJumpInput();
        // HandleBlockInput(); // blocking seems useless in this game, removing for now but keeping in the code.
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
        precisePosition = new Vector2(midX, 20);
        height = 50;
        SetTransformFromPrecisePosition();
        CurrentHP = MaxHP;
        UI.Instance.NotifyHeroHealthChange(this);
        foreach (EnemyController enemy in enemies) {
            if (enemy.state != State.WaitingForPlayer) {
                enemy.ReceiveHit(precisePosition, 0, Hit.Type.Knockdown);
            }
        }
    }

    private void RestrictScreenBoundaries() {
        Vector2 xBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        if (precisePosition.x < xBoundaries.x + 8) {
            precisePosition.x = xBoundaries.x + 8;
        } else if (precisePosition.x > xBoundaries.y - 8) {
            precisePosition.x = xBoundaries.y - 8;
        }
        SetTransformFromPrecisePosition();
    }

    private void HandleJumpInput() {
        if (CanJump() && Input.GetButton("Jump")) {
            dzHeight = jumpForce;
            grounded = false;
            animator.SetBool("IsJumping", true);
        }

        if (!grounded) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            if (height < 0f) {
                grounded = true;
                state = State.Idle;
                height = 0f;
                animator.SetBool("IsJumping", false);
            }
        }
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
            // first check if there is something to pick up
            if (PickableItem != null && (!HasKnife && PickableItem.Type == Pickable.PickableType.Knife)) {
                animator.SetTrigger("Pickup");
                if (PickableItem.Type == Pickable.PickableType.Knife) {
                    HasKnife = true;
                    UpdateKnifeGameObject();
                }
                PickableItem.PickupItem();
            } else {
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

                // check if a barrel was on the way and break it
                MaybeBreakBarrel();
            }
        }
    }

    private void MaybeBreakBarrel() {
        LayerMask barrelMask = LayerMask.GetMask("Barrel");
        Vector3 direction = IsFacingLeft ? Vector3.left : Vector3.right;
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * height, direction, attackReach, barrelMask);
        if (hit.collider != null && hit.collider.gameObject.GetComponent<Barrel>() != null) {
            Barrel barrel = hit.collider.gameObject.GetComponent<Barrel>();
            barrel.Break(precisePosition);
        }
    }

    private void HandleMoveInput() {
        if (CanMove()) {
            preciseVelocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * moveSpeed;
            TryMoveTo(precisePosition + preciseVelocity * Time.deltaTime);

            if (preciseVelocity != Vector2.zero) {
                state = State.Walking;
                if (preciseVelocity.x != 0f) {
                    characterSprite.flipX = preciseVelocity.x < 0;
                    knifeTransform.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
                    IsFacingLeft = characterSprite.flipX;
                }
            } else {
                state = State.Idle;
            }
            animator.SetBool("IsWalking", preciseVelocity != Vector2.zero);
        }
    }

}
