using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public abstract class BaseCharacterController : MonoBehaviour {
    public enum State { Idle, Walking, PreparingAttack, Attacking, Blocking, Hurt, Flying, Falling, Grounded, Dropping, WaitingForDoor, Dying, Dead, WaitingForPlayer }
    public enum InitialPosition { Behind, Garage, Roof, Street }

    public event EventHandler OnDirectionChange;
    public event EventHandler OnDying;
    public event EventHandler OnDeath;

    [field: SerializeField] public int MaxHP { get; protected set; }
    [field: SerializeField] public bool HasKnife { get; protected set; }
    [SerializeField] protected bool hasMultipleKnives;
    [SerializeField] protected float timeBetweenKnives;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackReach;
    [SerializeField] protected float durationLyingDead;
    [SerializeField] protected float durationGrounded;
    [SerializeField] protected float durationInvincibleAfterGettingUp;
    [SerializeField] protected bool isDisappearWhenDying;
    [SerializeField] protected SpriteRenderer characterSprite;
    [SerializeField] protected SpriteRenderer shadowSprite;
    [SerializeField] protected Transform knifeTransform;
    [SerializeField] protected float gravity = 10f;
    [SerializeField] protected float verticalMarginBetweenEnemyAndPlayer;
    [SerializeField] protected Knife knifePrefab;
    [SerializeField] protected Pickable pickableKnifePrefab;
    [SerializeField] protected Spark sparkPrefab;

    public int CurrentHP { get; protected set; }

    //protected Vector2 position;
    protected Vector2 preciseVelocity;
    protected float timeLastAttack = float.NegativeInfinity;
    protected float timeDyingStart = float.NegativeInfinity;
    protected float timeSinceGrounded = float.NegativeInfinity;
    protected float timeLastKnifeThrown = float.NegativeInfinity;
    protected Vector2 precisePosition = Vector2.zero; // x axis goes from -32 to end of level, y goes from -32 to 0
    protected float height = 0f;
    protected float dzHeight = 0f;
    protected bool grounded = true;
    public State state { get; protected set; } = State.Idle;
    protected Animator animator;
    protected InitialPosition initialPosition = InitialPosition.Street;

    protected virtual void Start() {
        CurrentHP = MaxHP;
        animator = GetComponent<Animator>();
        precisePosition = new Vector2(transform.position.x, transform.position.y);
        UpdateKnifeGameObject();
    }

    protected void UpdateKnifeGameObject() {
        if (HasKnife && knifeTransform != null) {
            knifeTransform.gameObject.SetActive(true);
        } else {
            knifeTransform.gameObject.SetActive(false);
        }
    }

    public bool IsFacingLeft { get; protected set; }

    public abstract void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal);
    public abstract bool IsVulnerable(Vector2 damageOrigin, bool canBlock = true);
    protected abstract void MaybeInductDamage();
    protected abstract void Update();

    public void OnAttackAnimationEndFrameEvent() {
        timeLastAttack = Time.timeSinceLevelLoad;
        state = State.Idle;
    }

    public void OnInvincibilityAnimationFrameEnd() {
        state = State.Idle;
    }

    public void OnAttackFrameEvent() {
        MaybeInductDamage();
    }

    protected void ThrowKnife() {
        Knife knife = Instantiate(knifePrefab);
        knife.transform.SetParent(transform.parent);
        knife.Direction = IsFacingLeft ? Vector2.left : Vector2.right;
        knife.transform.position = transform.position + (IsFacingLeft ? Vector3.left : Vector3.right) * 8;
        knife.Emitter = this;
        HasKnife = false;
        knifeTransform.gameObject.SetActive(false);
    }

    protected void SetTransformFromPrecisePosition() {
        transform.position = new Vector3(Mathf.FloorToInt(precisePosition.x), Mathf.FloorToInt(precisePosition.y + height), Mathf.FloorToInt(precisePosition.y));
        shadowSprite.transform.position = new Vector3(Mathf.FloorToInt(precisePosition.x), Mathf.FloorToInt(precisePosition.y), Mathf.FloorToInt(precisePosition.y));
    }

    protected void TryMoveTo(Vector2 newPosition) {
        // check on horiz and vert axis separately to prevent blocking on wall when going in diagonal
        if (CanMoveTo(new Vector2(precisePosition.x, newPosition.y))) {
            precisePosition = new Vector2(precisePosition.x, newPosition.y);
            SetTransformFromPrecisePosition();
        } else {
            preciseVelocity.y = 0;
        }

        if (CanMoveTo(new Vector2(newPosition.x, precisePosition.y))) {
            precisePosition = new Vector2(newPosition.x, precisePosition.y);
            SetTransformFromPrecisePosition();
        } else {
            preciseVelocity.x = 0;
        }
    }

    protected bool CanMoveTo(Vector2 destination) {
        if (this is PlayerController && destination.y > 30) return false; // not great but the following still allows to hit the walls :(
        if (state == State.Dying) return true;
        Vector3 targetedPosition = new Vector3(Mathf.FloorToInt(destination.x), Mathf.FloorToInt(destination.y), 0);
        Vector2 direction = (targetedPosition - transform.position).normalized;
        LayerMask worldMask = LayerMask.GetMask("World");
        LayerMask enemyMask = LayerMask.GetMask("Enemy");
        LayerMask mask;
        if (this is EnemyController) {
            mask = enemyMask; // only collide with other enemies
        } else {
            mask = worldMask;
        }
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * height, direction, 3f, mask);
        Debug.DrawRay(transform.position + Vector3.down * Mathf.CeilToInt(height), direction, Color.red);
        //if (hit.collider != null && hit.collider.gameObject != gameObject) {
        //    Debug.Log(hit.collider);
        //}
        return hit.collider == null || hit.collider.gameObject == gameObject;
    }

    protected void HandleDropping() {
        if (state == State.Dropping) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            precisePosition += preciseVelocity * Time.deltaTime;
            SetTransformFromPrecisePosition();
            if (height < 0) {
                state = State.Idle;
                height = 0f;
            }
        }
    }

    protected void HandleFalling() {
        if (state == State.Falling) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            precisePosition += preciseVelocity * Time.deltaTime;
            TryMoveTo(precisePosition);
            if (height <= 0) {
                state = State.Grounded;
                height = 0f;
                timeSinceGrounded = Time.timeSinceLevelLoad;
                animator.SetBool("IsFalling", false);
                Debug.Log("grounded");
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
                    characterSprite.color = new Color(1f, 1f, 1f, 1f - progress);
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

    protected bool CanAttack() {
        return state == State.Idle || state == State.Walking;
    }

    protected bool CanMove() {
        return state == State.Idle ||
               state == State.Walking ||
               !grounded;
    }

    protected bool CanJump() {
        return state != State.Attacking && state != State.Blocking && grounded;
    }

    protected bool CanBlock() {
        return state == State.Idle || state == State.Walking;
    }
}
