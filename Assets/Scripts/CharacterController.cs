using System;
using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    public enum State { Idle, Walking, PreparingAttack, Attacking, Blocking, Hurt, Flying, Falling, Grounded }

    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackReach;
    [SerializeField] protected float durationGrounded;
    [SerializeField] protected CharacterSprite sprite;
    [SerializeField] protected Animator animator;
    [SerializeField] protected float gravity = 10f;
    [SerializeField] protected float verticalMarginBetweenEnemyAndPlayer;

    protected Vector2 position; // floating point precise location, will get rounded up at the transform level
    protected Vector2 velocity;
    protected float timeLastAttack = float.NegativeInfinity;
    protected float zHeight = 0f;
    protected float dzHeight = 0f;
    protected bool grounded = true;
    protected State state = State.Idle;

    protected virtual void Start() {
        sprite.OnAttackAnimationComplete += Sprite_OnAttackAnimationComplete;
        sprite.OnInvincibilityEnd += Sprite_OnInvincibilityEnd;
        sprite.OnAttackFrame += Sprite_OnAttackFrame;
        position = new Vector2(transform.position.x, transform.position.y);
    }

    public bool IsFacingLeft() {
        return sprite.GetComponent<SpriteRenderer>().flipX;
    }

    public abstract void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal);
    public abstract bool IsVulnerable(Vector2 damageOrigin);
    protected abstract void AttemptAttack();
    protected abstract void FixedUpdate();

    private void Sprite_OnInvincibilityEnd(object sender, EventArgs e) {
        state = State.Idle;
    }

    private void Sprite_OnAttackAnimationComplete(object sender, System.EventArgs e) {
        timeLastAttack = Time.timeSinceLevelLoad;
        state = State.Idle;
    }

    private void Sprite_OnAttackFrame(object sender, EventArgs e) {
        AttemptAttack();
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
