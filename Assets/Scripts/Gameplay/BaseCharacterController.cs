using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public abstract class BaseCharacterController : MonoBehaviour {
    public enum State { Idle, Walking, PreparingAttack, Attacking, Blocking, Hurt, Flying, Falling, Grounded, Dropping, WaitingForDoor, Dying, Dead }

    public event EventHandler OnDirectionChange;
    public event EventHandler OnDying;
    public event EventHandler OnDeath;

    [field: SerializeField] public int MaxHP { get; protected set; }
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
    protected float timeDyingStart = float.NegativeInfinity;
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
        Vector3 targetedPosition = new Vector3(destination.x, destination.y, 0);
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

    protected void NotifyChangeDirection() {
        OnDirectionChange?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void ReceiveDamage(int damage) {
        CurrentHP -= damage;
        if (CurrentHP < 0) {
            CurrentHP = 0;
        }
    }

    protected void NotifyDeath() {
        if (CurrentHP <= 0) {
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void NotifyDying() {
        OnDying?.Invoke(this, EventArgs.Empty);
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
