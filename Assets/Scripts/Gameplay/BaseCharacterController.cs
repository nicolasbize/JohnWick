using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
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
    [SerializeField] protected SpriteRenderer characterSprite;
    [SerializeField] protected Transform knifeTransform;
    [SerializeField] protected float gravity = 10f;
    [SerializeField] protected float verticalMarginBetweenEnemyAndPlayer;
    [SerializeField] protected Knife knifePrefab;

    public int CurrentHP { get; protected set; }

    protected Vector2 position; // floating point precise location, will get rounded up at the transform level
    protected Vector2 velocity;
    protected float timeLastAttack = float.NegativeInfinity;
    protected float timeDyingStart = float.NegativeInfinity;
    protected float timeSinceGrounded = float.NegativeInfinity;
    protected float timeLastKnifeThrown = float.NegativeInfinity;
    protected float zHeight = 0f;
    protected float dzHeight = 0f;
    protected bool grounded = true;
    protected State state = State.Idle;
    protected Animator animator;
    protected InitialPosition initialPosition = InitialPosition.Street;

    protected virtual void Start() {
        CurrentHP = MaxHP;
        animator = GetComponent<Animator>();
        position = new Vector2(transform.position.x, transform.position.y);
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
    protected abstract void FixedUpdate();

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
        knife.transform.position = transform.position + Vector3.down * 19 + (IsFacingLeft ? Vector3.left : Vector3.right) * 8;
        knife.Emitter = this;
        HasKnife = false;
        knifeTransform.gameObject.SetActive(false);
    }

    protected void TryMoveTo(Vector2 newPosition) {
        // check on horiz and vert axis separately to prevent blocking on wall when going in diagonal
        if (CanMoveTo(new Vector2(position.x, newPosition.y))) {
            position = new Vector2(position.x, newPosition.y);
            transform.position = new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0);
        } else {
            velocity.y = 0;
        }

        if (CanMoveTo(new Vector2(newPosition.x, position.y))) {
            position = new Vector2(newPosition.x, position.y);
            transform.position = new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0);
        } else {
            velocity.x = 0;
        }
    }

    protected bool CanMoveTo(Vector2 destination) {
        Vector3 targetedPosition = new Vector3(Mathf.FloorToInt(destination.x), Mathf.FloorToInt(destination.y), 0);
        Vector2 direction = (targetedPosition - transform.position).normalized;
        LayerMask worldMask = LayerMask.GetMask("World");
        LayerMask enemyMask = LayerMask.GetMask("Enemy");
        LayerMask mask;
        if (this is EnemyController) {
            mask = enemyMask; // only collide with other enemies
        } else {
            mask = worldMask | enemyMask;
        }
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 2f, mask);
        //if (hit.collider != null && hit.collider.gameObject != gameObject) {
        //    Debug.Log(hit.collider);
        //}
        return hit.collider == null || hit.collider.gameObject == gameObject;
    }


    protected void HandleFalling() {
        if (state == State.Falling) {
            dzHeight -= gravity * Time.deltaTime;
            zHeight += dzHeight;
            position += velocity * Time.deltaTime;
            transform.position = new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0);
            if (zHeight <= 0) {
                state = State.Grounded;
                zHeight = 0f;
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
            } else {
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

    protected bool CanAttack() {
        return state != State.Attacking;
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
