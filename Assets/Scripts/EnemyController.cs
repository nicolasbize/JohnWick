using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float hitReach;
    [SerializeField] private Vector2 minMaxSecsBeforeHitting;
    [SerializeField] private PlayerController player;
    [SerializeField] private CharacterSprite sprite;
    [SerializeField] private Animator animator;
    

    enum State { Idle, Walk, PrepareAttack, Attack }

    private Rigidbody2D rb;
    private Vector2 position;
    private Vector2 speed;
    private State state = State.Idle;
    private float timeSincePreparedToHit = float.NegativeInfinity;
    private float waitDurationBeforeHit = 0f;
    private float verticalMarginBetweenEnemyAndPlayer = 4;

    void Start() {
        position = new Vector2(transform.position.x, transform.position.y);
        sprite.OnAttackAnimationComplete += Sprite_OnAttackAnimationComplete;
        sprite.OnAttackFrame += Sprite_OnAttackFrame;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Sprite_OnAttackFrame(object sender, System.EventArgs e) {
        if (IsPlayerWithinReach() && player.IsVulnerable()) {
            player.ReceiveHitFromEnemy();
        }
    }

    private void Sprite_OnAttackAnimationComplete(object sender, System.EventArgs e) {
        state = State.Idle; // no need for trigger, enable movement again
    }

    void FixedUpdate()
    {
        if (CanMove()) {
            FacePlayer();
            Vector2 nextTargetDestination = GetNextMovementDirection();
            bool isPlayerTooFar = nextTargetDestination.magnitude > 0;
            animator.SetBool("IsWalking", isPlayerTooFar);
            if (isPlayerTooFar) {
                WalkTowards(nextTargetDestination);
            } else {
                if (state != State.Attack && state != State.PrepareAttack) {
                    StopWalking();
                    PrepareAttack();
                } else if (IsReadyToAttack()) {
                    Attack();
                }
            }
        }
    }

    private void Attack() {
        state = State.Attack;
        if (Random.Range(0f, 1f) > 0.5f) {
            animator.SetTrigger("Punch");
        } else {
            animator.SetTrigger("PunchAlt");
        }
    }

    private bool IsReadyToAttack() {
        return state == State.PrepareAttack &&
            (Time.timeSinceLevelLoad - timeSincePreparedToHit > waitDurationBeforeHit);
    }

    private void WalkTowards(Vector2 targetDestination) {
        speed = targetDestination * moveSpeed;
        position += speed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), 0);
        state = State.Walk;
    }

    private void StopWalking() {
        state = State.Idle;
    }

    private void PrepareAttack() {
        if (state != State.PrepareAttack) {
            state = State.PrepareAttack;
            waitDurationBeforeHit = Random.Range(minMaxSecsBeforeHitting.x, minMaxSecsBeforeHitting.y);
            timeSincePreparedToHit = Time.timeSinceLevelLoad;
        }
    }

    private bool IsPlayerWithinReach() {
        bool isYAligned = Mathf.Abs(player.transform.position.y - transform.position.y) < verticalMarginBetweenEnemyAndPlayer;
        bool isXAligned = Mathf.Abs(player.transform.position.x - transform.position.x) < hitReach + 1;
        return (isYAligned && isXAligned);
    }

    private Vector2 GetNextMovementDirection() {
        Vector2 target = Vector2.zero;

        if (IsPlayerWithinReach()) {
            return Vector2.zero; // no need to go any further
        }

        if (transform.position.x > player.transform.position.x) {
            target = new Vector2(player.transform.position.x + hitReach, player.transform.position.y);
        } else {
            target = new Vector2(player.transform.position.x - hitReach, player.transform.position.y);
        }
        return (target - position).normalized;
    }

    private bool CanMove() {
        return state != State.Attack;
    }

    private void FacePlayer() {
        if (player != null) {
            sprite.GetComponent<SpriteRenderer>().flipX = player.transform.position.x < transform.position.x;
        }
    }
}
