using System;
using UnityEngine;

public class BouncerController : BaseCharacterController, IBoss {

    [SerializeField] private PlayerController player;

    private float timeSinceLanded = float.NegativeInfinity;
    private float durationLanding = 1f;
    private bool isDoingInitialDrop = true;
    private bool hasStartedEngaging = false;
    [field: SerializeField] public EnemySO EnemySO { get; private set; }

    protected override void Start() {
        base.Start();
        CurrentHP = MaxHP;
        player.RegisterEnemy(this);
        state = State.WaitingForPlayer;
        OnFinishDropping += OnFinishInitialDrop;
    }

    public void Activate() {
        height = 54;
        precisePosition = transform.position + Vector3.down * height;
        SetTransformFromPrecisePosition();
        state = State.Dropping;
        animator.SetBool("IsDropping", true);
    }

    private void OnFinishInitialDrop(object sender, EventArgs e) {
        animator.SetBool("IsDropping", false);
        Camera.main.GetComponent<CameraFollow>().Shake(0.05f, 3);
        player.ReceiveHit(precisePosition, 0, Hit.Type.Knockdown);
        timeSinceLanded = Time.timeSinceLevelLoad;
        isDoingInitialDrop = false;
        UI.Instance.SetBossMode(this, EnemySO.enemyType);
        Debug.Log("landed");
    }

    public override bool IsVulnerable(Vector2 damageOrigin, bool canBlock = true) {
        if (!hasStartedEngaging) return false;
        return true;
    }

    public override void ReceiveHit(Vector2 damageOrigin, int dmg = 0, Hit.Type hitType = Hit.Type.Normal) {
        if (IsVulnerable(damageOrigin)) {
            ReceiveDamage(dmg);
            state = State.Hurt;
            animator.SetTrigger("Hurt");
            audioSource.PlayOneShot(hitSound);
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
                Debug.Log("let's go");
            }
        }
    }

    protected override void MaybeInductDamage() {
        throw new NotImplementedException();
    }
}

