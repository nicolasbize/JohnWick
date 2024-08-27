using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : BaseCharacterController {



    [SerializeField] private float jumpForce;
    
    public List<BaseCharacterController> Enemies { get; private set; }
    private bool isTransitioningLevel = false;
    private bool hasFinishedTransition = false;
    private bool hasCompletedLevel = false;
    private bool isAirKicking = false;
    private float timeSinceLanded = float.NegativeInfinity;
    private float durationBetweenJumps = 0.3f;
    private bool isJumpingLeft = false;
    private Dictionary<MeleePosition, EnemyController> availableMeleeSlots = new Dictionary<MeleePosition, EnemyController>() {
        { MeleePosition.TopLeft, null },
        { MeleePosition.TopRight, null },
        { MeleePosition.BottomLeft, null },
        { MeleePosition.BottomRight, null },
    };

    private List<Vector2> transitionDestinations = new List<Vector2>() {
        new Vector2(640, 3), new Vector2(670, 3)
    };
    private List<string> comboAttackTriggers = new List<string>() {
        "Punch", "Punch", "PunchAlt", "Kick", "Roundhouse"
    };
    private int currentComboIndex = 0;
    private int currentTransitionDestinationIndex = 0;

    public static PlayerController Instance;

    protected override void Awake() {
        base.Awake();
        Enemies = new List<BaseCharacterController>();
        Instance = this;
    }

    protected override void Start() {
        base.Start();
        World.Instance.OnLevelTransitionStart += OnLevelTransitionStart;
        UI.Instance.NotifyHeroHealthChange(this);
    }

    public void StartNewLevel() {
        PrecisePosition = new Vector3(30, 6, 6);
        preciseVelocity = Vector3.zero;
        CurrentHP = MaxHP;
        UI.Instance.NotifyHeroHealthChange(this);
        hasFinishedTransition = false;
        isTransitioningLevel = false;
        state = State.Idle;
        animator.SetBool("IsWalking", false);
        height = 0;
        SetTransformFromPrecisePosition();
        Enemies.Clear();
        hasCompletedLevel = false;
    }

    public void ReturnControlsToPlayer() {
        isTransitioningLevel = false;
    }

    private void OnLevelTransitionStart(object sender, EventArgs e) {
        currentTransitionDestinationIndex = 0;
        isTransitioningLevel = true;
    }

    public void RegisterEnemy(BaseCharacterController enemy) {
        if (!Enemies.Contains(enemy)) {
            Enemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(BaseCharacterController enemy) {
        if (Enemies.Contains(enemy)) {
            Enemies.Remove(enemy);
        }
    }

    public void FreeMeleeSlot(MeleePosition slot) {
        availableMeleeSlots[slot] = null;
    }

    public MeleePosition ReserveMeleeSlot(EnemyController enemy) {
        List<MeleePosition> availableSlots = new List<MeleePosition>();
        foreach (MeleePosition position in availableMeleeSlots.Keys) {
            if (position != MeleePosition.None && availableMeleeSlots[position] == null) {
                availableSlots.Add(position);
            }
        }
        if (availableSlots.Count > 0) {
            MeleePosition rndPosition = availableSlots[Random.Range(0, availableSlots.Count)];
            availableMeleeSlots[rndPosition] = enemy;
            return rndPosition;
        } else {
            return MeleePosition.None;
        }
    }

    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            Vector2 attackVector = damageOrigin.x < PrecisePosition.x ? Vector2.right : Vector2.left;
            ReceiveDamage(dmg);
            animator.SetBool("IsJumping", false);
            if (hitType == Hit.Type.Knockdown || (CurrentHP <= 0) || height > 0) {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                preciseVelocity = attackVector * 50f;
                dzHeight = 1f;
                Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 1);
                GenerateSparkFX();
                DropAllCarriedWeapons();
                SoundManager.Instance.Play(SoundManager.SoundType.HitAlt);
            } else {
                preciseVelocity = attackVector * (moveSpeed / 2f);
                state = State.Hurt;
                animator.SetTrigger("Hurt");
                SoundManager.Instance.Play(SoundManager.SoundType.Hit);
            }
            UI.Instance.NotifyHeroHealthChange(this);
            BreakCombo();
        }
    }

    public override bool IsVulnerable(Vector2 damageOrigin) {
        if (state == State.Hurt || state == State.Falling || state == State.Grounded || state == State.Dying || state == State.Dead) {
            return false;
        }
        if (Time.timeSinceLevelLoad - timeSinceGrounded < durationGrounded + durationInvincibleAfterGettingUp) {
            return false;
        }

        return true;
    }


    private void Sprite_OnInvincibilityEnd(object sender, EventArgs e) {
        state = State.Idle;
    }

    private void Sprite_OnAttackAnimationComplete(object sender, System.EventArgs e) {
        state = State.Idle;
    }

    protected override void MaybeInductDamage(bool muteMissSounds = false) {
        bool hasHitEnemy = false;
        // get list of vulnerable enemies within distance.
        foreach (BaseCharacterController enemy in Enemies) {
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
            bool isYAligned = Mathf.Abs(enemy.transform.position.z - transform.position.z) < verticalMarginBetweenEnemyAndPlayer;
            bool isXAligned = Mathf.Abs(enemy.transform.position.x - transform.position.x) < attackReach + 1;
            isAlignedWithPlayer = isYAligned && isXAligned;
            if (isAlignedWithPlayer && isInFrontOfPlayer && enemy.IsVulnerable(transform.position)) {
                bool isPowerAttack = currentComboIndex == comboAttackTriggers.Count - 1;
                Hit.Type hitType = isPowerAttack ? Hit.Type.PowerEject : Hit.Type.Normal;
                if (state == State.Jumping) { // jump kick always knocks down
                    hitType = Hit.Type.Knockdown;
                }
                int damage = isPowerAttack ? 4 : 2;
                enemy.ReceiveHit(PrecisePosition, damage, hitType);
                hasHitEnemy = true;
            }
        }

        // increment combo
        if (hasHitEnemy) {
            currentComboIndex = (currentComboIndex + 1) % comboAttackTriggers.Count;
        } else {
            // start combo motion over
            currentComboIndex = 0;
            if (!muteMissSounds) {
                SoundManager.Instance.Play(SoundManager.SoundType.MissJump);
            }
        }
    }

    private void BreakCombo() {
        currentComboIndex = 1; // don't start at zero since this is the first hit
        ComboIndicator.Instance.ResetCombo();
    }

    private bool isAttackPressed = false;
    private bool isJumpPressed = false;

    private void Update() {
        if (Input.GetButtonDown(InputHelper.BTN_JUMP) && (Time.timeSinceLevelLoad - timeSinceLanded > durationBetweenJumps)) {
            isJumpPressed = true;
        }
        if (Input.GetButtonDown(InputHelper.BTN_ATTACK)) {
            isAttackPressed = true;
        }
    }

    protected override void FixedUpdate() {
        if (hasCompletedLevel) return;
        if (isTransitioningLevel && !hasFinishedTransition) {
            HandleLevelTransition();
        } else {
            bool wasFacingLeft = IsFacingLeft;

            HandleJumpingWithInput();
            HandleWalkingWithInput();
            HandleAttackingWithInput();
            HandleAirKicks();
            HandleDropping();
            HandleFalling();
            HandleGrounded();
            HandleDying();

            if (IsFacingLeft != wasFacingLeft) {
                NotifyChangeDirection();
            }

            RestrictScreenBoundaries();

            FixIncorrectState();
        }
    }

    protected void FixIncorrectState() {
        // sometimes we can get in these weird anim states that don't correlate with character states.
        // call this method to fix it
        AnimatorClipInfo[] animatorInfo = this.animator.GetCurrentAnimatorClipInfo(0);
        if (animatorInfo.Length > 0) {
            string currentAnimation = animatorInfo[0].clip.name;
            if (currentAnimation == "Fall" && (state != State.Falling && state != State.Grounded)) {
                Debug.Log("fixed incorrect Falling state, was " + state);
                state = State.Falling;
            }
        }
    }

    public void Respawn() {
        state = State.Jumping;
        DropAllCarriedWeapons();
        animator.SetBool("IsJumping", true);
        capsuleCollider.enabled = true;
        Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        float midX = Mathf.FloorToInt((screenBoundaries.y - screenBoundaries.x) / 2f);
        PrecisePosition = new Vector2(midX, 20);
        height = 50;
        SetTransformFromPrecisePosition();
        CurrentHP = MaxHP;
        UI.Instance.NotifyHeroHealthChange(this);
        foreach (BaseCharacterController enemy in Enemies) {
            if (enemy.state != State.WaitingForPlayer) {
                enemy.ReceiveHit(PrecisePosition, 0, Hit.Type.Knockdown);
            }
        }
    }

    private void RestrictScreenBoundaries() {
        float buffer = 10f;
        Vector2 xBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        if (PrecisePosition.x < xBoundaries.x + buffer) {
            PrecisePosition = new Vector2(xBoundaries.x + buffer, PrecisePosition.y);
        } else if (PrecisePosition.x > xBoundaries.y - buffer) {
            PrecisePosition = new Vector2(xBoundaries.y - buffer, PrecisePosition.y);
        }
        SetTransformFromPrecisePosition();
    }

    private void HandleAirKicks() {
        if (state == State.Jumping && isAirKicking) {
            MaybeInductDamage(true);
        }
    }

    private void HandleJumpingWithInput() {
        if (CanJump() && isJumpPressed && (Time.timeSinceLevelLoad - timeSinceLanded > durationBetweenJumps)) {
            dzHeight = jumpForce * Time.deltaTime;
            state = State.Jumping;
            animator.SetBool("IsJumping", true);
            SoundManager.Instance.Play(SoundManager.SoundType.MissJump);
            isJumpPressed = false;
            isJumpingLeft = IsFacingLeft;
        }

        if (state == State.Jumping) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            if (height < 0f) {
                state = State.Idle;
                height = 0f;
                animator.SetBool("IsJumping", false);
                isAirKicking = false;
                timeSinceLanded = Time.timeSinceLevelLoad;
            }
        }
    }

    private void HandleAttackingWithInput() {
        if (CanAttack() && isAttackPressed) {
            isAttackPressed = false;
            // first check if there is something to pick up
            Pickable pickable = PickUpItemFromGround();
            if (pickable != null) {
                animator.SetTrigger("Pickup");
                if (pickable.Type == Pickable.PickableType.Knife) {
                    HasKnife = true;
                    UpdateKnifeGameObject();
                } else if (pickable.Type == Pickable.PickableType.Gun) {
                    HasGun = true;
                    bulletsLeft = 3;
                    UpdateGunGameObject();
                } else if (pickable.Type == Pickable.PickableType.Food) {
                    CurrentHP = MaxHP;
                    UI.Instance.NotifyHeroHealthChange(this);
                    SoundManager.Instance.Play(SoundManager.SoundType.EatFood);
                }
                pickable.PickupItem();
            } else {
                statePriorToAttacking = state;
                if (state == State.Jumping) {
                    animator.SetTrigger("AirKick");
                    isAirKicking = true;
                } else {
                    if (HasKnife) {
                        animator.SetTrigger("PunchAlt");
                        ThrowKnife();
                        // don't perform an attack if throwing the knife already
                    } else if (HasGun) {
                        animator.SetTrigger("Punch");
                        if (bulletsLeft > 0) {
                            bulletsLeft -= 1;
                            ShootGun();
                        } else {
                            ThrowGun();
                        }
                    } else {
                        animator.SetTrigger(comboAttackTriggers[currentComboIndex]);
                        state = State.Attacking;
                    }
                }

                // check if a barrel was on the way and break it
                MaybeBreakBarrel();
            }
        }
    }

    private Pickable PickUpItemFromGround() {
        LayerMask mask = LayerMask.GetMask("Pickable");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down * 2, mask);
        if (hit.collider != null && hit.collider.GetComponent<Pickable>() != null) {
            Pickable pickable = hit.collider.GetComponent<Pickable>();
            if (pickable.IsPickable) {
                if (pickable.Type == Pickable.PickableType.Food) return pickable;
                if (HasKnife || HasGun) return null;
                if (HasKnife && pickable.Type == Pickable.PickableType.Knife) return null;
                if (HasGun && pickable.Type == Pickable.PickableType.Gun) return null;
                return pickable;
            }
        }
        return null;
    }

    private void MaybeBreakBarrel() {
        LayerMask barrelMask = LayerMask.GetMask("Breakable");
        Vector3 direction = IsFacingLeft ? Vector3.left : Vector3.right;
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * height, direction, attackReach, barrelMask);
        if (hit.collider != null && hit.collider.gameObject.GetComponent<Breakable>() != null) {
            Breakable barrel = hit.collider.gameObject.GetComponent<Breakable>();
            barrel.Break(PrecisePosition);
            SoundManager.Instance.Play(SoundManager.SoundType.HitAlt);
        }
    }

    protected override bool CanAttack() {
        return state == State.Idle || state == State.Walking || state == State.Jumping;
    }

    private void HandleWalkingWithInput() {
        if (CanMove()) {
            preciseVelocity = new Vector2(Input.GetAxisRaw(InputHelper.AXIS_HORIZONTAL), Input.GetAxisRaw(InputHelper.AXIS_VERTICAL)).normalized * moveSpeed;

            // prevent changing direction when jumping
            if (state == State.Jumping) {
                if (isJumpingLeft && preciseVelocity.x > 0) {
                    preciseVelocity = new Vector2(0, preciseVelocity.y);
                } else if (!isJumpingLeft && preciseVelocity.x < 0) {
                    preciseVelocity = new Vector2(0, preciseVelocity.y);
                }
            }
            TryMoveTo(PrecisePosition + preciseVelocity * Time.deltaTime);

            if (preciseVelocity != Vector2.zero) {
                if (state != State.Jumping) { 
                    state = State.Walking;
                }
                if (preciseVelocity.x != 0f) {
                    characterSprite.flipX = preciseVelocity.x < 0;
                    knifeTransform.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
                    gunTransform.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
                    IsFacingLeft = characterSprite.flipX;
                }
                animator.SetBool("IsWalking", true);
            } else if (state != State.Jumping) {
                state = State.Idle;
                animator.SetBool("IsWalking", false);
            }

        }
    }

    private void HandleLevelTransition() {
        animator.SetBool("IsFalling", false);
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsWalking", true);
        height = 0;
        SetTransformFromPrecisePosition();
        characterSprite.flipX = false;
        Vector2 distanceToNextPoint = transitionDestinations[currentTransitionDestinationIndex] - PrecisePosition;
        if (distanceToNextPoint.magnitude < 1) {
            if (currentTransitionDestinationIndex < transitionDestinations.Count - 1) {
                currentTransitionDestinationIndex += 1;
            } else {
                if (!hasCompletedLevel) {
                    World.Instance.CompleteLevel();
                    hasCompletedLevel = true;
                }
                isTransitioningLevel = false;
                hasFinishedTransition = true; // wait for World to reload player in next stage
            }
        } else {
            preciseVelocity = distanceToNextPoint.normalized * moveSpeed;
            TryMoveTo(PrecisePosition + preciseVelocity * Time.deltaTime, true);
        }
    }

}
