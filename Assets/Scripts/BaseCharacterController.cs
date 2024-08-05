using System;
using UnityEngine;

public abstract class BaseCharacterController : MonoBehaviour
{
    public enum State { Idle, Walking, PreparingAttack, Attacking, Blocking, Hurt, Flying, Falling, Grounded }

    public event EventHandler OnHealthChange;
    public event EventHandler OnDirectionChange;
    public event EventHandler OnDeath;

    [field: SerializeField] public int MaxHP { get; private set; }
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackReach;
    [SerializeField] protected float durationLyingDead;
    [SerializeField] protected float durationGrounded;
    [SerializeField] protected SpriteRenderer characterSprite;
    [SerializeField] protected float gravity = 10f;
    [SerializeField] protected float verticalMarginBetweenEnemyAndPlayer;

    public int CurrentHP { get; protected set; }

    protected Vector2 position; // floating point precise location, will get rounded up at the transform level
    protected Vector2 velocity;
    protected float timeLastAttack = float.NegativeInfinity;
    protected float zHeight = 0f;
    protected float dzHeight = 0f;
    protected bool grounded = true;
    protected State state = State.Idle;
    protected Animator animator;

    private void Awake() {
        CurrentHP = MaxHP;
    }

    protected virtual void Start() {
        animator = GetComponent<Animator>();
        position = new Vector2(transform.position.x, transform.position.y);
    }

    public bool IsFacingLeft { get; protected set; }

    public abstract void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal);
    public abstract bool IsVulnerable(Vector2 damageOrigin);
    protected abstract void AttemptAttack();
    protected abstract void FixedUpdate();

    public void OnAttackAnimationEndFrameEvent() {
        timeLastAttack = Time.timeSinceLevelLoad;
        state = State.Idle;
    }

    public void OnInvincibilityAnimationFrameEnd() {
        state = State.Idle;
    }

    public void OnAttackFrameEvent() {
        AttemptAttack();
    }

    protected void NotifyChangeDirection() {
        OnDirectionChange?.Invoke(this, EventArgs.Empty);
    }

    protected void ReceiveDamage(int damage) {
        CurrentHP -= damage;
        // todo death
        OnHealthChange?.Invoke(this, EventArgs.Empty);
        if (CurrentHP <= 0) {
            OnDeath?.Invoke(this, EventArgs.Empty);
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
