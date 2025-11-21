using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    [Header("Move / Jump")]
    [SerializeField] float runSpeed = 12f;
    [SerializeField] float jumpHeight = 0.5f;
    [SerializeField] LayerMask groundMask;

    [Header("Acceleration (微惯性)")]
    [SerializeField] float groundAccel = 60f;
    [SerializeField] float groundDecel = 50f;
    [SerializeField] float airAccel    = 35f;
    [SerializeField] float stopThreshold = 0.05f;

    [Header("Crouch (视觉压扁)")]
    [SerializeField] Transform visual;
    [SerializeField] Vector2 crouchScale = new Vector2(1.2f, 0.7f);
    [SerializeField] float crouchLerp = 12f;

    [Header("Ladder")]
    [SerializeField] float climbSpeed = 3.5f;     // ↑↓ 速度
    [SerializeField] float ladderRunSpeedMult = 0.8f; // 梯子上的左右速度系数（可调）

    [Header("Ground Probe")]
    [SerializeField] float probeRadius = 0.06f;
    [SerializeField] float probeOffset = 0.04f;
    [SerializeField] float probeDepth  = 0.10f;

    Rigidbody2D rb; CircleCollider2D cc;
    Vector3 normalScale, normalLocalPos;
    float gAbs, baseGravity;
    float moveX, climbY;
    bool wantJump, wantCrouch, inLadder;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CircleCollider2D>();
        normalScale    = visual ? visual.localScale    : Vector3.one;
        normalLocalPos = visual ? visual.localPosition : Vector3.zero;

        baseGravity = rb.gravityScale;
        gAbs = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        rb.freezeRotation = true;
    }

    // ==== Input (Unity Events) ====
    public void OnMove  (InputAction.CallbackContext ctx){ moveX = ctx.ReadValue<float>(); }
    public void OnClimb (InputAction.CallbackContext ctx){ climbY = ctx.ReadValue<float>(); }
    public void OnJump  (InputAction.CallbackContext ctx){ if (ctx.performed) wantJump = true; }
    public void OnCrouch(InputAction.CallbackContext ctx){ wantCrouch = ctx.ReadValue<float>() > 0.5f; }

    void Update(){
        if (visual) visual.rotation = Quaternion.identity;

        // 梯子上不做压扁；地面状态才压扁并贴地
        if (visual){
            Vector3 targetScale = normalScale;
            Vector3 targetPos   = normalLocalPos;
            if (!inLadder && wantCrouch){
                targetScale = new Vector3(
                    normalScale.x * crouchScale.x,
                    normalScale.y * crouchScale.y,
                    normalScale.z
                );
                if (IsGrounded()){
                    float r = cc ? cc.radius : 0.5f;
                    float drop = r * (1f - crouchScale.y);
                    Vector3 localDown = transform.InverseTransformVector(Vector3.down * drop);
                    targetPos = normalLocalPos + localDown;
                }
            }
            visual.localScale    = Vector3.Lerp(visual.localScale,    targetScale, crouchLerp * Time.deltaTime);
            visual.localPosition = Vector3.Lerp(visual.localPosition, targetPos,   crouchLerp * Time.deltaTime);
        }

        // 跳（不在梯子时才允许）
        if (!inLadder && IsGrounded() && wantJump){
            float vy = Mathf.Sqrt(Mathf.Max(0.01f, 2f * gAbs * jumpHeight));
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vy);
        }
        wantJump = false;
    }

    void FixedUpdate(){
        if (inLadder){
            // ✅ 梯子模式：允许左右 + 上下；关重力，但不锁水平
            rb.gravityScale = 0f;

            // 左右微惯性（用地面加速度，速度乘一个系数）
            float targetX = moveX * runSpeed * ladderRunSpeedMult;
            float vx = rb.linearVelocity.x;
            float delta = Mathf.Clamp(targetX - vx, -groundAccel * Time.fixedDeltaTime, groundAccel * Time.fixedDeltaTime);
            vx += delta;
            if (Mathf.Abs(moveX) < 0.01f && Mathf.Abs(vx) < stopThreshold) vx = 0f;

            // 垂直匀速爬；没按键就停在梯子上
            float vy = climbY * climbSpeed;

            rb.linearVelocity = new Vector2(vx, vy);
            return;
        }

        // 常规模式
        rb.gravityScale = baseGravity;

        float targetRun = wantCrouch ? 0f : moveX * runSpeed;
        bool onGround = IsGrounded();
        bool hasInput = Mathf.Abs(moveX) > 0.01f;
        float a = onGround ? (hasInput ? groundAccel : groundDecel) : airAccel;

        float curX = rb.linearVelocity.x;
        float step = Mathf.Clamp(targetRun - curX, -a * Time.fixedDeltaTime, a * Time.fixedDeltaTime);
        curX += step;
        if (!hasInput && Mathf.Abs(curX) < stopThreshold) curX = 0f;

        rb.linearVelocity = new Vector2(curX, rb.linearVelocity.y);
    }

    // ==== Ground check (三点探地) ====
    bool IsGrounded(){
        if (!cc) return false;
        float r = cc.radius;
        Vector2 center = (Vector2)transform.position + Vector2.down * (r * 0.98f);
        Vector2 left   = center + Vector2.left  * probeOffset;
        Vector2 right  = center + Vector2.right * probeOffset;
        return HitDown(center) || HitDown(left) || HitDown(right);
    }
    bool HitDown(Vector2 from){
        return Physics2D.CircleCast(from, probeRadius, Vector2.down, probeDepth, groundMask).collider != null;
    }

    // ==== Ladder trigger ====
    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Ladder")){
            inLadder = true;
            // 不清零水平，让它自然延续；只把竖直停住
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }
    void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("Ladder")){
            inLadder = false;
            rb.gravityScale = baseGravity;
        }
    }
}
