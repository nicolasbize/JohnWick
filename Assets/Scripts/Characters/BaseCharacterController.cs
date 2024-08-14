using System;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public abstract class BaseCharacterController : MonoBehaviour {
    public enum State { Idle, Walking, PreparingAttack, Attacking, Blocking, Hurt, Flying, Falling, Grounded, Dropping, WaitingForDoor, Dying, Dead, WaitingForPlayer, Jumping, Summersalting }
    public enum InitialPosition { Behind, Garage, Roof, Street }

    public event EventHandler OnDirectionChange;
    public event EventHandler OnDying;
    public event EventHandler OnDeath;
    public event EventHandler OnFinishDropping;

    [field: SerializeField] public int MaxHP { get; protected set; }
    [field: SerializeField] public bool HasKnife { get; protected set; }
    [field: SerializeField] public bool HasGun { get; protected set; }

    [SerializeField] protected bool hasMultipleKnives;
    [SerializeField] protected float timeBetweenKnives;
    [SerializeField] protected float timeBetweenGunShot;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackReach;
    [SerializeField] protected float durationLyingDead;
    [SerializeField] protected float durationGrounded;
    [SerializeField] protected float durationInvincibleAfterGettingUp;
    [SerializeField] protected bool isDisappearWhenDying;
    [SerializeField] protected SpriteRenderer characterSprite;
    [SerializeField] protected SpriteRenderer shadowSprite;
    [SerializeField] protected Transform knifeTransform;
    [SerializeField] protected Transform gunTransform;
    [SerializeField] protected float gravity = 10f;
    [SerializeField] protected float verticalMarginBetweenEnemyAndPlayer;
    [SerializeField] protected ThrownWeapon thrownKnifePrefab; // knife that characters can throw
    [SerializeField] protected ThrownWeapon thrownGunPrefab; // gun that characters can throw
    [SerializeField] protected BulletShot bulletPrefab; // bullets that are shot
    [SerializeField] protected Pickable pickableKnifePrefab; // knife that the character can drop
    [SerializeField] protected Pickable pickableGunPrefab; // gun that the character can drop
    [SerializeField] protected Spark sparkPrefab;
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip hitAltSound;
    [SerializeField] protected AudioClip missSound;
    [SerializeField] protected AudioClip eatFoodSound;
    [SerializeField] protected AudioClip gunshotSound;


    public int CurrentHP { get; protected set; }
    public State state { get; protected set; } = State.Idle;

    protected Vector2 preciseVelocity;
    protected float timeLastAttack = float.NegativeInfinity;
    protected float timeDyingStart = float.NegativeInfinity;
    protected float timeSinceGrounded = float.NegativeInfinity;
    protected float timeLastKnifeThrown = float.NegativeInfinity;
    protected float timeLastGunShot = float.NegativeInfinity;
    protected int bulletsLeft = 3;
    public Vector2 PrecisePosition { get; protected set; } = Vector2.zero; // x axis goes from -32 to end of level, y goes from -32 to 0
    protected float height = 0f;
    protected float dzHeight = 0f;
    //protected bool grounded = true;
    protected Animator animator;
    protected InitialPosition initialPosition = InitialPosition.Street;
    protected AudioSource audioSource;
    protected State statePriorToAttacking = State.Idle;

    protected virtual void Awake() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start() {
        CurrentHP = MaxHP;
        PrecisePosition = new Vector2(transform.position.x, transform.position.y);
        UpdateKnifeGameObject();
        UpdateGunGameObject();
    }

    protected void UpdateKnifeGameObject() {
        if (knifeTransform == null) return;
        knifeTransform.gameObject.SetActive(HasKnife);
    }

    protected void UpdateGunGameObject() {
        if (gunTransform == null) return;
        gunTransform.gameObject.SetActive(HasGun);
    }

    public bool IsFacingLeft { get; protected set; }

    public abstract void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal);
    public abstract bool IsVulnerable(Vector2 damageOrigin, bool canBlock = true);
    protected abstract void MaybeInductDamage(bool muteMissSounds = false);
    protected abstract void FixedUpdate();

    public void OnAttackAnimationEndFrameEvent() {
        timeLastAttack = Time.timeSinceLevelLoad;
        state = statePriorToAttacking;
    }

    public void OnInvincibilityAnimationFrameEnd() {
        state = State.Idle;
        DoWorkAfterHurtComplete();
    }

    protected virtual void DoWorkAfterHurtComplete() { }

    public void OnAttackFrameEvent() {
        MaybeInductDamage();
    }

    protected void ThrowKnife() {
        ThrownWeapon knife = Instantiate(thrownKnifePrefab, transform.parent);
        knife.Direction = IsFacingLeft ? Vector2.left : Vector2.right;
        knife.transform.position = transform.position + (IsFacingLeft ? Vector3.left : Vector3.right) * 8;
        knife.Emitter = this;
        DropAllCarriedWeapons();
    }

    protected void ThrowGun() {
        ThrownWeapon knife = Instantiate(thrownGunPrefab, transform.parent);
        knife.Direction = IsFacingLeft ? Vector2.left : Vector2.right;
        knife.transform.position = transform.position + (IsFacingLeft ? Vector3.left : Vector3.right) * 8;
        knife.Emitter = this;
        DropAllCarriedWeapons();
    }

    protected void DropAllCarriedWeapons() {
        HasKnife = false;
        HasGun = false;
        knifeTransform.gameObject.SetActive(false);
        gunTransform.gameObject.SetActive(false);
    }

    protected void ShootGun() {
        float hMargin = (this is PlayerController) ? 20 : 24;
        float vMargin = (this is PlayerController) ? 15 : 17;
        BulletShot shot = Instantiate(bulletPrefab, transform.parent);
        shot.transform.position = transform.position + (IsFacingLeft ? Vector3.left : Vector3.right) * hMargin; // size of gun
        Vector3 startBulletTrail = shot.transform.position + Vector3.up * vMargin; // 17 height, compensate on Y axis
        Vector3 endBulletTrail = Vector3.zero;
        List<BaseCharacterController> possibleTargets = new List<BaseCharacterController>(GameObject.FindObjectsOfType<BaseCharacterController>());
        Vector2 screenBoundaries = Camera.main.GetComponent<CameraFollow>().GetScreenXBoundaries();
        float shotLength = 0;
        if (IsFacingLeft) {
            shotLength = startBulletTrail.x - screenBoundaries.x;
        } else {
            shotLength = screenBoundaries.y - startBulletTrail.x;
        }
        BaseCharacterController target = GetShotTarget(shot.transform.position, IsFacingLeft ? Vector3.left : Vector3.right, 1, shotLength, possibleTargets);
        if (target == null) {
            endBulletTrail = new Vector3(IsFacingLeft ? screenBoundaries.x : screenBoundaries.y, startBulletTrail.y, startBulletTrail.z);
        } else {
            endBulletTrail = new Vector3(target.transform.position.x, startBulletTrail.y, startBulletTrail.z);
        }
        shot.SetUp(startBulletTrail, endBulletTrail);

        // deal damage
        if (target != null) {
            int damage = 6;
            if (this is PlayerController) { damage *= 2; }
            target.ReceiveHit(PrecisePosition, damage, Hit.Type.Knockdown);
        }
        audioSource.PlayOneShot(gunshotSound);
    }

    private BaseCharacterController GetShotTarget(Vector3 startGroundPosition, Vector3 direction, int currentLength, float lengthLimit, List<BaseCharacterController> possibleTargets) {
        if (currentLength >= lengthLimit) return null;
        float buffer = 3;
        bool headingLeft = direction == Vector3.left;
        float rectXStart = headingLeft ? (startGroundPosition.x - currentLength) : startGroundPosition.x;
        float rectYStart = startGroundPosition.z - buffer;
        float rectWidth = currentLength;
        float rectHeight = buffer * 2;
        Rect rect = new Rect(rectXStart, rectYStart, rectWidth, rectHeight);
        foreach(BaseCharacterController target in possibleTargets) {
            if (target.IsVulnerable(startGroundPosition) && rect.Contains(new Vector2(target.transform.position.x, target.transform.position.z))) {
                return target;
            }
        }
        return GetShotTarget(startGroundPosition, direction, currentLength + 1, lengthLimit, possibleTargets);
    }

    protected void SetTransformFromPrecisePosition() {
        transform.position = new Vector3(Mathf.FloorToInt(PrecisePosition.x), Mathf.FloorToInt(PrecisePosition.y + height), Mathf.FloorToInt(PrecisePosition.y));
        shadowSprite.transform.position = new Vector3(Mathf.FloorToInt(PrecisePosition.x), Mathf.FloorToInt(PrecisePosition.y), Mathf.FloorToInt(PrecisePosition.y));
    }

    protected void TryMoveTo(Vector2 newPosition, bool ignoreWalls = false) {
        // check on horiz and vert axis separately to prevent blocking on wall when going in diagonal
        if (CanMoveTo(new Vector2(PrecisePosition.x, newPosition.y), ignoreWalls)) {
            PrecisePosition = new Vector2(PrecisePosition.x, newPosition.y);
            SetTransformFromPrecisePosition();
        } else {
            preciseVelocity.y = 0;
        }

        if (CanMoveTo(new Vector2(newPosition.x, PrecisePosition.y), ignoreWalls)) {
            PrecisePosition = new Vector2(newPosition.x, PrecisePosition.y);
            SetTransformFromPrecisePosition();
        } else {
            preciseVelocity.x = 0;
        }
    }

    protected bool CanMoveTo(Vector2 destination, bool ignoreWalls = false) {
        if (!ignoreWalls) {
            // hardcoded limits because I can't get proper sliding/collision in unity :(
            // this is pretty horrible
            if (this is PlayerController && destination.y > 30) return false;
            if (this is PlayerController && destination.y < 2) return false;
            if (this is PlayerController && World.Instance.CurrentLevelIndex == 0 && destination.x > 294 && destination.y > 28 - (destination.x - 295)) return false;
            if (this is PlayerController && World.Instance.CurrentLevelIndex == 1) { //todo: swap this back to 1 when levels are both loaded
                // check if the boss is registered, might be super slow let's see.
                ButcherController boss = (ButcherController)((PlayerController)this).Enemies.Find(e => e is ButcherController);
                if (boss != null && !boss.HasStartedEngaging) {
                    Rect barRect = new Rect(240f, 15f, 160, 60);
                    if (barRect.Contains(destination)) return false;
                }
            }
        }

        if (this is PlayerController) {
            Vector3 targetedPosition = new Vector3(Mathf.FloorToInt(destination.x), Mathf.FloorToInt(destination.y), 0);
            Vector2 direction = (targetedPosition - transform.position).normalized;
            LayerMask mask = LayerMask.GetMask("Breakable");
            RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * height, direction, 3f, mask);
            if (hit.collider != null) return false;
        }
        
        return true;


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

    protected void HandleDropping() {
        if (state == State.Dropping) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            PrecisePosition += preciseVelocity * Time.deltaTime;
            SetTransformFromPrecisePosition();
            if (height < 0) {
                state = State.Idle;
                height = 0f;
                OnFinishDropping?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected void HandleFalling() {
        if (state == State.Falling) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            Vector2 newAttemptedPosition = PrecisePosition + preciseVelocity * Time.deltaTime;

            TryMoveTo(newAttemptedPosition);
            if (height <= 0) {
                state = State.Grounded;
                height = 0f;
                timeSinceGrounded = Time.timeSinceLevelLoad;
                animator.SetBool("IsFalling", false);
            }
        }
    }

    protected void HandleGrounded() {
        if (state == State.Grounded) {
            if (Time.timeSinceLevelLoad - timeSinceGrounded > durationGrounded) {
                if (CurrentHP > 0) {
                    animator.SetTrigger("GetUp");
                    state = State.Idle;
                } else {
                    state = State.Dying;
                    timeDyingStart = Time.timeSinceLevelLoad;
                    OnDying?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    protected void HandleDying() {
        if (state == State.Dying) {
            float progress = (Time.timeSinceLevelLoad - timeDyingStart) / durationLyingDead;
            if (progress >= 1) {
                state = State.Dead;
                if (CurrentHP <= 0) {
                    OnDeath?.Invoke(this, EventArgs.Empty);
                }
                HandleExtraWorkAfterDeath();
            } else if (isDisappearWhenDying) {
                // oscillate five times
                bool isHidden = Mathf.RoundToInt(progress * 10f) % 2 == 1;
                if (isHidden) {
                    characterSprite.enabled = false;
                } else {
                    characterSprite.enabled = true;
                    //characterSprite.color = new Color(1f, 1f, 1f, 1f - progress); // fade to transparent when dying, not too NES like though
                }
            }
        }
    }

    protected virtual void HandleExtraWorkAfterDeath() { }

    protected void NotifyChangeDirection() {
        OnDirectionChange?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void ReceiveDamage(int damage) {
        CurrentHP -= damage;
        if (CurrentHP < 0) {
            CurrentHP = 0;
        }
    }

    protected void GenerateSparkFX() {
        Spark spark = Instantiate<Spark>(sparkPrefab, transform.parent);
        spark.transform.position = new Vector3(transform.position.x, transform.position.y + 12, 0);
    }

    protected virtual bool CanAttack() {
        return state == State.Idle || state == State.Walking || state == State.Jumping;
    }

    protected bool CanMove() {
        return state == State.Idle ||
               state == State.Walking ||
               state == State.Jumping;
    }

    protected bool CanJump() {
        return state == State.Idle || state == State.Walking;
    }

    protected bool CanBlock() {
        return state == State.Idle || state == State.Walking;
    }
}
