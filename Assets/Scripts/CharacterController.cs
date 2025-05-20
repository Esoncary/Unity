using UnityEngine;
using UnityEngine.Events;

public class CharacterController : MonoBehaviour
{
    //移动相关
    [SerializeField] Rigidbody2D m_RigidBody2D;//刚体
    [Range(0, 1)][SerializeField] float m_MovementSmoothing = 0.2f;//移动系数 越小 趋近的越快
    [SerializeField] Vector2 m_Velocity = Vector2.zero;//自身的速度

    const float m_DefaultMoveSpeed = 1;
    float m_MoveSpeed = m_DefaultMoveSpeed;
    float m_move;

    //转向
    bool m_FacingRight = true;

    //跳跃相关
    [SerializeField]
    float m_JumpForce = 400f;//跳跃时施加的力
    bool m_Grounded;//是否站在地面
    float previousVelY;

    //下蹲相关
    bool m_crouch = false;
    [Range(0, 1)]
    [SerializeField]
    private float m_CrouchSpeed = .36f;//下蹲后移动的速度为原来的多少
    [SerializeField] private Collider2D m_CrouchDisableCollider;//下蹲禁止的碰撞体

    //爬
    [SerializeField] float m_ClimbForce = 2f;
    bool onWall;
    float gravityScale = 2;
    bool m_climb = false;
    [SerializeField] float m_JumpClimbForce = 400f;

    //地面检测
    [SerializeField] Transform m_GroundCheck;//检测点
    float k_GroundCheckRadius = .3f;//检测半径
    [SerializeField] LayerMask m_GroundLayer;//地面Layer

    //左右检测
    [SerializeField] Transform m_LeftCheck;//检测点
    [SerializeField] Transform m_RightCheck;//检测点
    [SerializeField]
    float k_RoundCheckRadius = .2f;//检测半径

    //事件
    [Header("Events")]
    [Space]
    public BoolEvent OnJumpEvent;//跳跃事件
    public UnityEvent OnMoveEvent;//移动事件
    public BoolEvent OnCrouchEvent;//蹲下事件
    public BoolEvent OnClimbEvent;//爬事件
    public BoolEvent OnFallEvent;//落下事件
    // public
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }



    void Awake()
    {
        m_RigidBody2D = GetComponent<Rigidbody2D>();

        if (OnJumpEvent == null)
            OnJumpEvent = new BoolEvent();
        if (OnMoveEvent == null)
            OnMoveEvent = new UnityEvent();
        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
        if (OnClimbEvent == null)
            OnClimbEvent = new BoolEvent();
        if (OnFallEvent == null)
            OnFallEvent = new BoolEvent();
    }
    void FixedUpdate()
    {
        //一直判断是不是在地上
        bool wasGround = m_Grounded;
        m_Grounded = CheckIsGround();
        if (!wasGround && m_Grounded)
        {
            // OnJumpEvent.Invoke(false);
            OnFallEvent.Invoke(false);
        }

        // 物理更新后获取当前速度
        // Unity 内部会在 FixedUpdate 里先更新刚体 velocity，再调用你的 FixedUpdate
        float currentVelY = m_RigidBody2D.velocity.y;

        // 从上升（positive）到下降（negative）时，说明过了顶点
        if (previousVelY >= 0f && currentVelY <= 0f && !m_Grounded)
        {
            // 这里触发“开始下落”（到达最高点）
            // print("触发");
            OnFallEvent.Invoke(true);
            OnJumpEvent.Invoke(false);
        }
        previousVelY = currentVelY;

    }

    //移动
    public void Move(float move)
    {
        m_move = move;
        Vector2 targetVelocity = new Vector2(move * m_MoveSpeed, m_RigidBody2D.velocity.y);
        m_RigidBody2D.velocity = Vector2.SmoothDamp(m_RigidBody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        //角色始终朝向按键方向
        if (move < 0 && m_FacingRight || move > 0 && !m_FacingRight)
            Flip();
        //移动事件
        OnMoveEvent.Invoke();
    }
    //跳跃
    public void Jump(bool jump)
    {
        //攀墙跳
        if (m_climb && jump)
        {
            m_RigidBody2D.AddForce(new Vector2(0, m_JumpClimbForce));
            // OnJumpEvent.Invoke(true);//起跳事件
            OnClimbEvent.Invoke(false);
        }
        //地面起跳
        if (jump && m_Grounded)
        {
            m_RigidBody2D.AddForce(new Vector2(0, m_JumpForce));
            // Invoke("DelayJump", 0.1f);
            OnJumpEvent.Invoke(true);//起跳事件
        }
    }
    public void DelayJump()
    {
        m_Grounded = false;
    }
    //蹲下
    public void Crouch(bool crouch)
    {
        m_crouch = crouch;
        if (crouch && !m_climb)
        {
            //改移速
            m_MoveSpeed = m_CrouchSpeed;
            //禁用碰撞体
            if (m_CrouchDisableCollider != null)
                m_CrouchDisableCollider.enabled = false;
        }
        else
        {
            m_MoveSpeed = m_DefaultMoveSpeed;
            if (m_CrouchDisableCollider != null)
                m_CrouchDisableCollider.enabled = true;

        }
        OnCrouchEvent.Invoke(crouch);
    }

    //爬
    public void Climb(bool climb, float dirction)
    {
        m_climb = climb;
        // print(dirction);
        onWall = climb && (Physics2D.OverlapCircle(m_LeftCheck.position, k_RoundCheckRadius, m_GroundLayer) ||
                 Physics2D.OverlapCircle(m_RightCheck.position, k_RoundCheckRadius, m_GroundLayer));
        if (onWall)
        {
            m_RigidBody2D.velocity = new Vector2(m_RigidBody2D.velocity.x, m_ClimbForce * dirction);
            //重力为0
            m_RigidBody2D.gravityScale = 0;

        }
        else
        {
            //重力重设
            m_RigidBody2D.gravityScale = gravityScale;
        }
        OnClimbEvent.Invoke(onWall);

    }
    //判断是不是在地面上
    public bool CheckIsGround()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundCheckRadius, m_GroundLayer);
        foreach (Collider2D col in hits)
        {
            if (col.gameObject != gameObject)
            {
                // OnFallEvent.Invoke(false);
                return true;
            }
        }
        return false;
    }
    //转向
    private void Flip()
    {
        m_FacingRight = !m_FacingRight;

        Vector2 theScale = transform.localScale;
        //把Scale.x * -1 就能使角色转向
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}