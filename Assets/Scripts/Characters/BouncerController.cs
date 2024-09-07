using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BouncerController : BaseCharacterController, IBoss {

    public enum AttackType { SuperPunch, NormalAttack }

    [SerializeField] private float minTimeToAttackAfterBlock;
    [SerializeField] private Vector2 minMaxSecsBeforeHitting;
    [SerializeField] private float superPunchPower;

    private float timeSinceLanded = float.NegativeInfinity;
    private float durationLanding = 1f;
    private bool isDoingInitialDrop = true;
    private bool hasStartedEngaging = false;
    [field: SerializeField] public EnemySO EnemySO { get; private set; }
    private int hitsReceivedBeforeBlocking = 0;
    private AttackType nextAttackType = AttackType.NormalAttack;
    private float timeSinceStartBlock = float.NegativeInfinity;
    private bool isBlocking = false; // can be on top of walk / idle states

    private float timeSincePreparedToHit = float.NegativeInfinity;
    private float waitDurationBeforeHit = 0f;
    private bool isInHittingStance = false;
    private PlayerController player;

    protected override void Start() {
        base.Start();
        CurrentHP = MaxHP;
        player = PlayerController.Instance;
        player.RegisterEnemy(this);
        state = State.WaitingForPlayer;
        OnFinishDropping += OnFinishInitialDrop;
    }

    public void Activate() {
        height = 54;
        PrecisePosition = transform.position + Vector3.down * height;
        SetTransformFromPrecisePosition();
        state = State.Dropping;
        animator.SetBool("IsDropping", true);
    }

    private void OnFinishInitialDrop(object sender, EventArgs e) {
        animator.SetBool("IsDropping", false);
        Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 3);
        player.ReceiveHit(PrecisePosition, 0, Hit.Type.Knockdown);
        timeSinceLanded = Time.timeSinceLevelLoad;
        isDoingInitialDrop = false;
        UI.Instance.SetBossMode(this, EnemySO.enemyType);
    }

    public override bool IsVulnerable(Vector2 damageOrigin) {
        if (!hasStartedEngaging) return false;
        if (isBlocking) return false;
        if (state == State.Flying) return false;
        if (CurrentHP <= 0) return false;
        if (state == State.Hurt || state == State.Falling || state == State.Grounded) return false;
        return true;
    }

    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            ComboIndicator.Instance.IncreaseCombo();
            if (hitsReceivedBeforeBlocking > 1) {
                isBlocking = true;
                animator.SetBool("IsBlocking", true);
                nextAttackType = Random.Range(0, 1) > 0.5f ? AttackType.NormalAttack : AttackType.SuperPunch;
                timeSinceStartBlock = Time.timeSinceLevelLoad;
                SoundManager.Instance.Play(SoundManager.SoundType.HitAlt);
            } else {
                ReceiveDamage(Math.Max(dmg - 1, 0));
                Vector2 attackVector = damageOrigin.x < PrecisePosition.x ? Vector2.right : Vector2.left;
                if (CurrentHP > 0) {
                    state = State.Hurt;
                    animator.SetTrigger("Hurt");
                    SoundManager.Instance.Play(SoundManager.SoundType.Hit);
                    hitsReceivedBeforeBlocking += 1;
                } else {
                    animator.SetBool("IsFalling", true);
                    state = State.Falling;
                    preciseVelocity = attackVector * (moveSpeed / 4f);
                    dzHeight = 2f;
                    Camera.main.GetComponent<CameraFollow>().Shake(0.35f, 2);
                    GenerateSparkFX();
                    SoundManager.Instance.Play(SoundManager.SoundType.Grunt);
                }
            }
            isInHittingStance = false;
        }
    }
    protected override void ReceiveDamage(int damage) {
        base.ReceiveDamage(damage);
        UI.Instance.NotifyEnemyHealthChange(this, EnemySO.enemyType);
    }

    protected override void FixedUpdate() {
        if (isDoingInitialDrop) {
            HandleDropping();
        } else if (!hasStartedEngaging) {
            if (Time.timeSinceLevelLoad - timeSinceLanded > durationLanding) {
                animator.SetTrigger("GetUp");
                hasStartedEngaging = true;
            }
        } else {
            HandleMoving();
            HandleAttack();
            HandleFlying();
            HandleDying();
            HandleFalling();
            HandleGrounded();
        }
    }

    protected override void MaybeInductDamage(bool muteMissSounds = false) {
        if (IsPlayerWithinReach() && player.IsVulnerable(PrecisePosition)) {
            player.ReceiveHit(PrecisePosition, 3, Hit.Type.Knockdown);
        }

    }

    private void HandleMoving() {

        if (CurrentHP > 0 && CanMove()) {
            FacePlayer();
            if (nextAttackType == AttackType.NormalAttack) { // go towards the player
                Vector2 nextTargetDestination = GetDirectionTowardsPlayer();
                bool isPlayerTooFar = nextTargetDestination.magnitude > 0;
                animator.SetBool("IsWalking", isPlayerTooFar);
                if (isPlayerTooFar) {
                    WalkTowards(nextTargetDestination);
                    isInHittingStance = false;
                } else if (!isPlayerTooFar && !isInHittingStance) {
                    isInHittingStance = true;
                    state = State.PreparingAttack;
                    timeSincePreparedToHit = Time.timeSinceLevelLoad;
                    waitDurationBeforeHit = Random.Range(minMaxSecsBeforeHitting.x, minMaxSecsBeforeHitting.y);
                }
            } else if (nextAttackType == AttackType.SuperPunch) {
                Vector2 nextTargetDestination = GetDirectionTowardsSuperPunch();
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

    private Vector2 GetDirectionTowardsSuperPunch() {
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
        if (CurrentHP > 0 && state == State.PreparingAttack &&
            (Time.timeSinceLevelLoad - timeSincePreparedToHit > waitDurationBeforeHit)) {
            
            isBlocking = false;
            hitsReceivedBeforeBlocking = 0;
            if (nextAttackType == AttackType.NormalAttack) {
                animator.SetBool("IsBlocking", false);
                state = State.Attacking;
                if (Random.Range(0f, 1f) > 0.5f) {
                    animator.SetTrigger("Punch");
                } else {
                    animator.SetTrigger("Kick");
                }
            } else {
                animator.SetBool("IsBlocking", false);
                animator.SetBool("IsSpecialAttack", true);
                state = State.Flying;
                SoundManager.Instance.Play(SoundManager.SoundType.MissJump);
                preciseVelocity = (IsFacingLeft ? Vector2.left : Vector2.right) * superPunchPower;
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
            if ((transform.position.x < screenBoundaries.x + 8) ||
                (transform.position.x > screenBoundaries.y - 16)) {
                animator.SetBool("IsSpecialAttack", false);
                state = State.Idle;
                Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 2);
                nextAttackType = AttackType.NormalAttack;
                SoundManager.Instance.Play(SoundManager.SoundType.HitAlt);
            }

            // check if we're enountering the player
            float margin = verticalMarginBetweenEnemyAndPlayer;
            Rect rect = new Rect(PrecisePosition.x - margin, PrecisePosition.y - margin, margin * 2, margin * 2);
            if (rect.Contains(player.PrecisePosition)) {
                MaybeInductDamage();
            }
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

