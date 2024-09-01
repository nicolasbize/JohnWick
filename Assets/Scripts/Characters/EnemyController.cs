using System;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class EnemyController : BaseCharacterController {
    
    public enum Type { Biker, Goon, Punk, StreetBoss, Thug, LoneWolf, Ruffian, BarBoss}

    [SerializeField] private float flySpeed;
    [SerializeField] private int pointsScored;
    [SerializeField] private Vector2 minMaxSecsBeforeHitting;
    [SerializeField] private GarageDoor garageDoor;
    [field:SerializeField] public EnemySO EnemySO { get; private set; }
    [field: SerializeField] public bool IsActivatedForCheckpoint { get; private set; }

    private float timeSincePreparedToHit = float.NegativeInfinity;
    private float waitDurationBeforeHit = 0f;
    private bool isInHittingStance = false;
    private Vector3 originalPosition;
    private PlayerController player;
    private MeleePosition reservedPosition;

    protected override void Start() {
        base.Start();
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
        if (IsActivatedForCheckpoint) return;
        if (initialPosition == InitialPosition.Street && state == State.WaitingForPlayer) {
            state = State.Idle; // only activate if waiting, might be on the ground due to knife strike
            IsActivatedForCheckpoint = true;
        } else if (initialPosition == InitialPosition.Behind) {
            state = State.Idle;
            PrecisePosition = new Vector2(originalPosition.x, originalPosition.y);
            transform.position = originalPosition;
            IsActivatedForCheckpoint = true;
        } else if (initialPosition == InitialPosition.Roof) {
            height = 50;
            PrecisePosition = new Vector2(originalPosition.x, originalPosition.y - height);
            SetTransformFromPrecisePosition();
            state = State.Dropping;
            IsActivatedForCheckpoint = true;
        } else if (initialPosition == InitialPosition.Garage) {
            if (garageDoor != null && !garageDoor.IsOpened) {
                // only change state if the door isn't opened already
                state = State.WaitingForDoor;
                IsActivatedForCheckpoint = true;
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
        UI.Instance.AddScore(pointsScored);
        Destroy(gameObject);
    }

    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            ReceiveDamage(dmg);
            ComboIndicator.Instance.IncreaseCombo();
            Vector2 attackVector = damageOrigin.x < PrecisePosition.x ? Vector2.right : Vector2.left;
            isInHittingStance = false; // knocks player out a bit
            if (HasKnife) {
                HasKnife = false;
                hasMultipleKnives = false;
                UpdateKnifeGameObject();
                DropPickable(pickableKnifePrefab);
            }
            if (HasGun) {
                HasGun = false;
                UpdateGunGameObject();
                DropPickable(pickableGunPrefab);
            }
            if (hitType == Hit.Type.PowerEject) {
                animator.SetBool("IsFlying", true);
                state = State.Flying;
                preciseVelocity = attackVector * flySpeed;
                GenerateSparkFX();
                SoundManager.Instance.Play(SoundManager.SoundType.HitAlt);
            } else if (hitType == Hit.Type.Knockdown || (CurrentHP <= 0)) {
                animator.SetBool("IsFalling", true);
                state = State.Falling;
                preciseVelocity = attackVector * moveSpeed * 3;
                dzHeight = 2f;
                Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 1);
                GenerateSparkFX();
                SoundManager.Instance.Play(SoundManager.SoundType.HitAlt);
            } else {
                preciseVelocity = attackVector * moveSpeed * 2;
                state = State.Hurt;
                animator.SetTrigger("Hurt");
                SoundManager.Instance.Play(SoundManager.SoundType.Hit);
            }
        }
    }

    private void DropPickable(Pickable prefab) {
        Pickable pickable = Instantiate(prefab);
        pickable.IsFalling = true;
        pickable.transform.SetParent(transform.parent);
        pickable.transform.position = transform.position + Vector3.down * 4;
    }

    public override bool IsVulnerable(Vector2 damageOrigin) {
        return state == State.Idle ||
               state == State.Walking ||
               state == State.PreparingAttack ||
               // the following allows to throw knife upon seeing htem before they are activated
               (state == State.WaitingForPlayer && initialPosition == InitialPosition.Street);
    }

    protected override void MaybeInductDamage(bool muteMissSounds = false) {
        if (HasKnife && player.IsVulnerable(PrecisePosition)) {
            ThrowKnife();
            timeLastKnifeThrown = Time.timeSinceLevelLoad;
        } else if (HasGun && player.IsVulnerable(PrecisePosition)) {
            ShootGun();
            timeLastGunShot = Time.timeSinceLevelLoad;
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
            if (HasKnife || HasGun) {
                Vector2 nextTargetDestination = GetRangeWeaponPosition();
                Vector2 direction = nextTargetDestination - PrecisePosition;
                if (direction.magnitude < 2) {
                    // don't go full machine gun here
                    if (HasGun && (Time.timeSinceLevelLoad - timeLastGunShot < timeBetweenGunShot)) return;
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
                if (reservedPosition == MeleePosition.None) {
                    reservedPosition = player.ReserveMeleeSlot(this);
                }
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

            // check if we're hitting other enemies along the way
            LayerMask mask = LayerMask.GetMask("Enemy");
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(mask);
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            Physics2D.Raycast(transform.position + Vector3.up * 2, Vector3.down * 2, contactFilter, hits);
            if (hits.Count > 0) {
                foreach (RaycastHit2D hit in hits) {
                    EnemyController other = hit.collider.GetComponent<EnemyController>();
                    if (other != null && other != this) {
                        if (other.IsVulnerable(PrecisePosition)) {
                            other.ReceiveHit(PrecisePosition, 2, Hit.Type.Knockdown);
                        }
                    }
                }

            }
            
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
            } else if (HasGun) {
                animator.SetTrigger("ShootGun");
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
        Vector2 xBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        float fasterInPositionSpeed = moveSpeed;
        float buffer = 16f;
        if (PrecisePosition.x < (xBoundaries.x + buffer) || PrecisePosition.x > (xBoundaries.y - buffer)) {
            fasterInPositionSpeed *= 3; // get them in the screen faster
        }
        preciseVelocity = targetDestination * fasterInPositionSpeed;
        TryMoveTo(PrecisePosition + preciseVelocity * Time.deltaTime);
        if (preciseVelocity == Vector2.zero) { // something is blocking the path, try to go down instead
            preciseVelocity = Vector2.down * moveSpeed;
            TryMoveTo(PrecisePosition + preciseVelocity * Time.deltaTime);
        }
        if (preciseVelocity != Vector2.zero) {
            state = State.Walking;
        } else {
            state = State.Idle;
        }
    }

    private bool IsPlayerWithinReach() {
        bool isYAligned = Mathf.Abs(player.transform.position.z - transform.position.z) < verticalMarginBetweenEnemyAndPlayer;
        bool isXAligned = Mathf.Abs(player.transform.position.x - transform.position.x) < attackReach + 1;
        bool isXTooClose = Mathf.Abs(player.transform.position.x - transform.position.x) < 4f;
        return (isYAligned && isXAligned && !isXTooClose);
    }

    private Vector2 GetRangeWeaponPosition() {
        float buffer = 20f;
        Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        // find closest screen boundary
        float destX = screenBoundaries.x + buffer;
        if (Mathf.Abs(PrecisePosition.x - screenBoundaries.x) > Mathf.Abs(PrecisePosition.x - screenBoundaries.y)) {
            destX = screenBoundaries.y - buffer;
        }
        return new Vector2(destX, player.transform.position.z);
    }

    

    private Vector2 GetNextMovementDirection() {
        Vector2 target = Vector2.zero;

        if (IsPlayerWithinReach()) {
            return Vector2.zero; // no need to go any further
        }
        switch (reservedPosition) {
            case MeleePosition.TopLeft:
                target = new Vector2(player.transform.position.x - attackReach, player.transform.position.z + verticalMarginBetweenEnemyAndPlayer / 2);
                break;
            case MeleePosition.TopRight:
                target = new Vector2(player.transform.position.x + attackReach, player.transform.position.z + verticalMarginBetweenEnemyAndPlayer / 2);
                break;
            case MeleePosition.BottomLeft:
                target = new Vector2(player.transform.position.x - attackReach, player.transform.position.z - verticalMarginBetweenEnemyAndPlayer / 2);
                break;
            case MeleePosition.BottomRight:
                target = new Vector2(player.transform.position.x + attackReach, player.transform.position.z - verticalMarginBetweenEnemyAndPlayer / 2);
                break;
            default: // no assigned slots, stay a bit further away
                if (transform.position.x > player.transform.position.x) {
                    target = new Vector2(player.transform.position.x + 3 * attackReach, player.transform.position.z);
                } else {
                    target = new Vector2(player.transform.position.x - 3 * attackReach, player.transform.position.z);
                }
                break;
        }
        return (target - PrecisePosition).normalized;
    }

    private void FacePlayer() {
        if (player != null) {
            characterSprite.flipX = player.transform.position.x < transform.position.x;
            if (HasKnife) {
                knifeTransform.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
            }
            if (HasGun) {
                gunTransform.GetComponent<SpriteRenderer>().flipX = characterSprite.flipX;
            }
            IsFacingLeft = characterSprite.flipX;
        }
    }
}
