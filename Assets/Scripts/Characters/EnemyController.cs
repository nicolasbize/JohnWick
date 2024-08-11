using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class EnemyController : BaseCharacterController {
    
    public enum Type { Biker, Goon, Punk, StreetBoss, Thug}

    [SerializeField] private float flySpeed;
    [SerializeField] private Vector2 minMaxSecsBeforeHitting;
    //[SerializeField] private PlayerController player;
    [SerializeField] private GarageDoor garageDoor;
    [field:SerializeField] public EnemySO EnemySO { get; private set; }

    private float timeSincePreparedToHit = float.NegativeInfinity;
    private float waitDurationBeforeHit = 0f;
    private bool isInHittingStance = false;
    private Vector3 originalPosition;
    private PlayerController player;

    protected override void Start() {
        base.Start();
        MaxHP = EnemySO.maxHealth;
        CurrentHP = MaxHP;
        player = PlayerController.Instance;
        player.RegisterEnemy(this);
    }

    public void InitializeFromCheckpoint(Checkpoint checkpoint) {
        state = State.WaitingForPlayer;
        animator.SetBool("IsWalking", false);
        if (!CheckForGarageInitialPosition()) {
            if (!CheckForRoofInitialPosition()) {
                CheckForBehindPosition(checkpoint);
            }
        }
    }

    public void ActivateFromCheckpoint() {
        if (initialPosition == InitialPosition.Street && state == State.WaitingForPlayer) {
            state = State.Idle; // only activate if waiting, might be on the ground due to knife strike
        } else if (initialPosition == InitialPosition.Behind) {
            state = State.Idle;
            PrecisePosition = new Vector2(originalPosition.x, originalPosition.y);
            transform.position = originalPosition;
        } else if (initialPosition == InitialPosition.Roof) {
            height = 50;
            PrecisePosition = new Vector2(originalPosition.x, originalPosition.y - height);
            SetTransformFromPrecisePosition();
            state = State.Dropping;
        } else if (initialPosition == InitialPosition.Garage) {
            if (garageDoor != null && !garageDoor.IsOpened) {
                // only change state if the door isn't opened already
                state = State.WaitingForDoor;
            }
        }
    }

    private void CheckForBehindPosition(Checkpoint checkpoint) {
        if (transform.position.x < checkpoint.CameraLockTargetX && transform.position.y < 32) {
            initialPosition = InitialPosition.Behind;
            originalPosition = new Vector2(transform.position.x, transform.position.y);
            transform.position = new Vector3(0, 200, 0);
        }
    }

    private bool CheckForGarageInitialPosition() {
        if (garageDoor != null && !garageDoor.IsOpened) {
            garageDoor.OnDoorOpened += GarageDoor_OnDoorOpened;
            initialPosition = InitialPosition.Garage;
            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>()) { // cover character and weapons
                renderer.sortingLayerName = "BackgroundFurniture";
                renderer.sortingOrder = 1;
            }
            return true;
        }
        return false;
    }

    private bool CheckForRoofInitialPosition() {
        if (transform.position.y > 32) {
            originalPosition = new Vector2(transform.position.x, transform.position.y);
            initialPosition = InitialPosition.Roof;
            return true;
        }
        return false;
    }

    private void GarageDoor_OnDoorOpened(object sender, EventArgs e) {
        state = State.Idle;
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>()) {
            renderer.sortingLayerName = "Characters";
            renderer.sortingOrder = 0;
        }
    }

    protected override void HandleExtraWorkAfterDeath() {
        if (player != null) {
            player.UnregisterEnemy(this);
        }
        Destroy(gameObject);
    }

    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            ReceiveDamage(dmg);
            Vector2 attackVector = damageOrigin.x < PrecisePosition.x ? Vector2.right : Vector2.left;
            isInHittingStance = false; // knocks player out a bit
            if (HasKnife) {
                HasKnife = false;
                hasMultipleKnives = false;
                UpdateKnifeGameObject();
                Pickable pickable = Instantiate(pickableKnifePrefab);
                pickable.IsFalling = true;
                pickable.transform.SetParent(transform.parent);
                pickable.transform.position = transform.position + Vector3.down * 4;
            }
            if (hitType == Hit.Type.PowerEject) {
                animator.SetBool("IsFlying", true);
                state = State.Flying;
                preciseVelocity = attackVector * flySpeed;
                GenerateSparkFX();
                audioSource.PlayOneShot(hitAltSound);
            } else if (hitType == Hit.Type.Knockdown || (CurrentHP <= 0)) {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                preciseVelocity = attackVector * moveSpeed * 3;
                dzHeight = 2f;
                Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 1);
                GenerateSparkFX();
                audioSource.PlayOneShot(hitAltSound);
            } else {
                preciseVelocity = attackVector * moveSpeed * 2;
                state = State.Hurt;
                animator.SetTrigger("Hurt");
                audioSource.PlayOneShot(hitSound);
            }
        }
    }

    public override bool IsVulnerable(Vector2 damageOrigin, bool canBlock = true) {
        return state == State.Idle ||
               state == State.Walking ||
               state == State.PreparingAttack ||
               // the following allows to throw knife upon seeing htem before they are activated
               (state == State.WaitingForPlayer && initialPosition == InitialPosition.Street);
    }

    protected override void MaybeInductDamage() {
        if (HasKnife && player.IsVulnerable(PrecisePosition)) {
            ThrowKnife();
            timeLastKnifeThrown = Time.timeSinceLevelLoad;
        } else {
            if (IsPlayerWithinReach() && player.IsVulnerable(PrecisePosition)) {
                player.ReceiveHit(PrecisePosition, 1);
            }
        }
        isInHittingStance = false; // take a breather
    }

    protected override void FixedUpdate() {
        HandleDropping(); // for spawns
        HandleGarageDoorHidding(); // for spawns
        HandleHurt();
        HandleDying();
        HandleFlying();
        HandleFalling();
        HandleGrounded();

        if (state != State.WaitingForPlayer) {
            HandleMoving();
            HandleAttack();
            CheckForKnifeRespawn();
        }
    }


    protected override void ReceiveDamage(int damage) {
        base.ReceiveDamage(damage);
        UI.Instance.NotifyEnemyHealthChange(this, EnemySO.enemyType);
    }

    private void HandleMoving() {
        if (CanMove()) {
            FacePlayer();
            if (HasKnife) {
                Vector2 nextTargetDestination = GetKnifeThrowingPosition();
                Vector2 direction = nextTargetDestination - PrecisePosition;
                if (direction.magnitude < 2) {
                    isInHittingStance = true;
                    state = State.PreparingAttack;
                    timeSincePreparedToHit = Time.timeSinceLevelLoad;
                    waitDurationBeforeHit = UnityEngine.Random.Range(minMaxSecsBeforeHitting.x, minMaxSecsBeforeHitting.y);
                    animator.SetBool("IsWalking", false);
                } else {
                    WalkTowards(direction.normalized);
                    animator.SetBool("IsWalking", true);
                }
            } else {
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
    }

    private void HandleHurt() {
        if (state == State.Hurt) {
            // carry momentum
            PrecisePosition += preciseVelocity * Time.deltaTime;
            SetTransformFromPrecisePosition();
        }
    }

    private void HandleFlying() {
        if (state == State.Flying) {
            PrecisePosition += preciseVelocity * Time.deltaTime;
            SetTransformFromPrecisePosition();
            Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
            height = 5f;
            if ((transform.position.x < screenBoundaries.x + 8) ||
                (transform.position.x > screenBoundaries.y - 8)) {
                animator.SetBool("IsFlying", false);
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                dzHeight = 2f;
                preciseVelocity = preciseVelocity.normalized * -10; // bounce 
                Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 1);
            }
        }
    }

    private void HandleGarageDoorHidding() {
        if (state == State.WaitingForDoor) {
            if (garageDoor != null && !garageDoor.IsOpened && !garageDoor.IsOpening) {
                garageDoor.Open();
            }
        }
    }

    private void HandleAttack() {
        if (state == State.PreparingAttack && 
            (Time.timeSinceLevelLoad - timeSincePreparedToHit > waitDurationBeforeHit)) {

            state = State.Attacking;
            if (HasKnife) {
                animator.SetTrigger("ThrowKnife");
            } else {
                if (UnityEngine.Random.Range(0f, 1f) > 0.5f) {
                    animator.SetTrigger("Punch");
                } else {
                    animator.SetTrigger("PunchAlt");
                }
            }
        }
    }

    private void CheckForKnifeRespawn() {
        if (hasMultipleKnives && !HasKnife && (Time.timeSinceLevelLoad - timeLastKnifeThrown > timeBetweenKnives)) {
            HasKnife = true;
            UpdateKnifeGameObject();
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

    private bool IsPlayerWithinReach() {
        bool isYAligned = Mathf.Abs(player.transform.position.y - transform.position.y) < verticalMarginBetweenEnemyAndPlayer;
        bool isXAligned = Mathf.Abs(player.transform.position.x - transform.position.x) < attackReach + 1;
        return (isYAligned && isXAligned);
    }

    private Vector2 GetKnifeThrowingPosition() {
        float buffer = 6f;
        Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        // find closest screen boundary
        float destX = screenBoundaries.x + buffer;
        if (Mathf.Abs(PrecisePosition.x - screenBoundaries.x) > Mathf.Abs(PrecisePosition.x - screenBoundaries.y)) {
            destX = screenBoundaries.y - buffer;
        }
        return new Vector2(destX, player.transform.position.y);
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
        return (target - PrecisePosition).normalized;
    }

    private void FacePlayer() {
        if (player != null) {
            characterSprite.flipX = player.transform.position.x < transform.position.x;
            if (HasKnife) {
                knifeTransform.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
            }
            IsFacingLeft = characterSprite.flipX;
        }
    }
}
