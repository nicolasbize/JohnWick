using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButcherController : BaseCharacterController, IBoss {
    public enum AttackType { NormalAttack, FlyKick, SummerSalt }

    [SerializeField] private Vector2 minMaxSecsBeforeHitting;
    [SerializeField] private float flyKickPower;
    [SerializeField] private float summersaltDuration;
    [SerializeField] private Breakable breakableBar;
    [SerializeField] private AudioClip gruntSound;
    [field: SerializeField] public EnemySO EnemySO { get; private set; }

    private bool isDoingEntrance = true;
    public bool HasStartedEngaging { get; private set; } = false;
    private int hitsReceivedBeforeAttacking = 0;
    private AttackType nextAttackType = AttackType.NormalAttack;
    private float timeSinceStartSummersalt = float.NegativeInfinity;
    private float timeSincePreparedToHit = float.NegativeInfinity;
    private float waitDurationBeforeHit = 0f;
    private bool isInHittingStance = false;
    private PlayerController player;
    private bool isFlyKicking = false;

    protected override void Start() {
        base.Start();
        CurrentHP = MaxHP;
        player = PlayerController.Instance;
        player.RegisterEnemy(this);
        state = State.WaitingForPlayer;
        state = State.Idle;
    }

    public void Activate() {
        // summersalt / destroy bar
        isDoingEntrance = false;
        breakableBar.Break(PrecisePosition);
        HasStartedEngaging = true;
        StartSummersalt();
    }

    public override bool IsVulnerable(Vector2 damageOrigin, bool canBlock = true) {
        if (isFlyKicking) return false;
        if (!HasStartedEngaging) return false;
        if (CurrentHP <= 0) return false;
        if (state == State.PreparingAttack && nextAttackType == AttackType.SummerSalt) return false;
        if (state == State.Summersalting || state == State.Hurt || state == State.Falling || state == State.Grounded) return false;
        return true;
    }

    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            ComboIndicator.Instance.IncreaseCombo();
            ReceiveDamage(Mathf.Max(dmg - 2, 0)); // 2 armor
            Vector2 attackVector = damageOrigin.x < PrecisePosition.x ? Vector2.right : Vector2.left;
            if (CurrentHP > 0) {
                state = State.Hurt;
                animator.SetTrigger("Hurt");
                audioSource.PlayOneShot(hitSound);
                hitsReceivedBeforeAttacking += 1;
            } else {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                preciseVelocity = attackVector * (moveSpeed / 4f);
                dzHeight = 2f;
                Camera.main.GetComponent<CameraFollow>().Shake(0.35f, 2);
                GenerateSparkFX();
                audioSource.PlayOneShot(gruntSound);
            }
            isInHittingStance = false;

        }
    }

    protected override void DoWorkAfterHurtComplete() {
        if (hitsReceivedBeforeAttacking >= 3) {
            StartSummersalt();
        }
    }

    private void StartSummersalt() {
        nextAttackType = AttackType.SummerSalt;
        isInHittingStance = true;
        state = State.PreparingAttack;
        timeSincePreparedToHit = Time.timeSinceLevelLoad;
        waitDurationBeforeHit = 1f;
        animator.SetBool("IsSummersalting", true);
        animator.SetTrigger("StartSummersalt");
    }


    protected override void ReceiveDamage(int damage) {
        base.ReceiveDamage(damage);
        UI.Instance.NotifyEnemyHealthChange(this, EnemySO.enemyType);
    }

    protected override void FixedUpdate() {
        if (isDoingEntrance) {
            HandleEntrance();
        } else if (!HasStartedEngaging) {
            // not sure we need to do anything here?
        } else {
            HandleMoving();
            HandleSummersalting();
            HandleAttack();
            HandleFlying();
            HandleDying();
            HandleFalling();
            HandleGrounded();
        }
        FixIncorrectState();
    }

    private void FixIncorrectState() {
        AnimatorClipInfo[] animatorInfo = this.animator.GetCurrentAnimatorClipInfo(0);
        if (animatorInfo.Length > 0) {
            string currentAnimation = animatorInfo[0].clip.name;
            if (currentAnimation == "JumpKick" && (state != State.Flying && state != State.Jumping)) {
                Debug.Log("fixed incorrect Flying state, was " + state);
                state = State.Flying;
            }
            if (currentAnimation == "Fall" && (state != State.Falling)) {
                Debug.Log("fixed incorrect Falling state, was " + state);
                state = State.Falling;
            }
        }
    }

    protected override void MaybeInductDamage(bool muteMissSounds = false) {
        if (IsPlayerWithinReach() && player.IsVulnerable(PrecisePosition)) {
            player.ReceiveHit(PrecisePosition, 3, Hit.Type.Knockdown);
        }
    }

    private void HandleEntrance() {

    }

    private void HandleMoving() {

        if (CanMove()) {
            FacePlayer();
            if (nextAttackType == AttackType.NormalAttack) { // go towards the player
                Vector2 nextTargetDestination = GetDirectionTowardsPlayer();
                bool isPlayerTooFar = nextTargetDestination.magnitude > 0;
                animator.SetBool("IsWalking", isPlayerTooFar);
                if (isPlayerTooFar) {
                    WalkTowards(nextTargetDestination);
                    isInHittingStance = false;
                } else if (!isPlayerTooFar && state != State.PreparingAttack) {
                    isInHittingStance = true;
                    state = State.PreparingAttack;
                    timeSincePreparedToHit = Time.timeSinceLevelLoad;
                    waitDurationBeforeHit = Random.Range(minMaxSecsBeforeHitting.x, minMaxSecsBeforeHitting.y);
                }
            } else if (nextAttackType == AttackType.FlyKick) {
                Vector2 nextTargetDestination = GetDirectionTowardsFlyKick();
                Vector2 direction = nextTargetDestination - PrecisePosition;
                if (direction.magnitude < 2) {
                    isInHittingStance = true;
                    state = State.PreparingAttack;
                    timeSincePreparedToHit = Time.timeSinceLevelLoad;
                    waitDurationBeforeHit = Random.Range(minMaxSecsBeforeHitting.x, minMaxSecsBeforeHitting.y);
                    animator.SetBool("IsWalking", false);
                } else {
                    WalkTowards(direction.normalized);
                    animator.SetBool("IsWalking", true);
                }
            }
        }
    }

    private void HandleSummersalting() {
        if (state == State.Summersalting) {
            if (Time.timeSinceLevelLoad - timeSinceStartSummersalt > summersaltDuration) {
                animator.SetBool("IsSummersalting", false);
                HasStartedEngaging = true;
                hitsReceivedBeforeAttacking = 0;
                state = State.Idle;
                PickNextRandomAttack();
            } else {
                Vector2 targetDestination = (player.transform.position - transform.position).normalized;
                preciseVelocity = targetDestination * moveSpeed * 1.5f;
                TryMoveTo(PrecisePosition + preciseVelocity * Time.deltaTime);
                DamagePlayerOnPath();
            }
        }
    }

    private void PickNextRandomAttack() {
        nextAttackType = Random.Range(0f, 1f) > 0.5f ? AttackType.NormalAttack : AttackType.FlyKick;
    }

    private Vector2 GetDirectionTowardsFlyKick() {
        float buffer = 6f;
        Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        // find closest screen boundary
        float destX = screenBoundaries.x + buffer;
        if (Mathf.Abs(PrecisePosition.x - screenBoundaries.x) > Mathf.Abs(PrecisePosition.x - screenBoundaries.y)) {
            destX = screenBoundaries.y - buffer;
        }
        return new Vector2(destX, player.transform.position.y);
    }

    private void HandleAttack() {
        if (state == State.PreparingAttack &&
            (Time.timeSinceLevelLoad - timeSincePreparedToHit > waitDurationBeforeHit)) {

            if (nextAttackType == AttackType.NormalAttack) {
                state = State.Attacking;
                animator.SetTrigger("Punch");
                PickNextRandomAttack();
            } else if (nextAttackType == AttackType.FlyKick) {
                animator.SetBool("IsFlyKicking", true);
                state = State.Flying;
                audioSource.PlayOneShot(missSound);
                preciseVelocity = (IsFacingLeft ? Vector2.left : Vector2.right) * flyKickPower;
            } else if (nextAttackType == AttackType.SummerSalt) {
                state = State.Summersalting;
                timeSinceStartSummersalt = Time.timeSinceLevelLoad;
            }
            isInHittingStance = false;
        }
    }

    private void WalkTowards(Vector2 targetDestination) {
        preciseVelocity = targetDestination * moveSpeed;
        TryMoveTo(PrecisePosition + preciseVelocity * Time.deltaTime);
        if (preciseVelocity != Vector2.zero) {
            state = State.Walking;
        } else {
            state = State.Idle;
        }
    }

    private void HandleFlying() {
        if (state == State.Flying) {
            PrecisePosition += preciseVelocity * Time.deltaTime;
            SetTransformFromPrecisePosition();
            Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
            if ((IsFacingLeft && transform.position.x < screenBoundaries.x + 8) ||
                (!IsFacingLeft && transform.position.x > screenBoundaries.y - 8)) {
                animator.SetBool("IsFlyKicking", false);
                state = State.Idle;
                Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 2);
                nextAttackType = AttackType.NormalAttack;
                audioSource.PlayOneShot(hitAltSound);
                nextAttackType = AttackType.NormalAttack;
            }

            DamagePlayerOnPath();
        }
    }

    private void DamagePlayerOnPath() {
        // check if we're enountering the player
        float margin = verticalMarginBetweenEnemyAndPlayer;
        Rect rect = new Rect(PrecisePosition.x - margin, PrecisePosition.y - margin, margin * 2, margin * 2);
        if (rect.Contains(player.PrecisePosition)) {
            MaybeInductDamage();
        }
    }

    private Vector2 GetDirectionTowardsPlayer() {
        Vector2 target = Vector2.zero;

        if (IsPlayerWithinReach()) {
            return Vector2.zero; // no need to go any further
        }

        if (transform.position.x > player.transform.position.x) {
            target = new Vector2(player.transform.position.x + attackReach, player.transform.position.z);
        } else {
            target = new Vector2(player.transform.position.x - attackReach, player.transform.position.z);
        }
        return (target - PrecisePosition).normalized;
    }

    private bool IsPlayerWithinReach() {
        bool isYAligned = Mathf.Abs(player.transform.position.z - transform.position.z) < verticalMarginBetweenEnemyAndPlayer;
        bool isXAligned = Mathf.Abs(player.transform.position.x - transform.position.x) < attackReach + 1;
        return (isYAligned && isXAligned);
    }

    private void FacePlayer() {
        if (player != null) {
            characterSprite.flipX = player.transform.position.x < transform.position.x;
            IsFacingLeft = characterSprite.flipX;
        }
    }
}
