using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    Rigidbody2D m_Rigidbody2D;
    //转向相关
    bool m_FacingRight = true;

    //移动相关
    Vector2 m_Velocity = Vector2.zero;
    [Range(0, 0.3f)]
    [SerializeField]
    float m_MovementSmoothing = .5f;//移动过度的速度

    //跳跃相关
    [SerializeField]
    float m_JumpForce = 400f;//跳跃时施加的力
    bool m_Grounded;//是否站在地面

    //地面检测
    [SerializeField] Transform m_GroundCheck;//检测点
    float k_GroundCheckRadius = .3f;//检测半径
    [SerializeField] LayerMask m_GroundLayer;//地面Layer
    //头顶检测
    [SerializeField] Transform m_CellingCheck;//检测点
    const float k_CellingCheckRadius = .2f;//检测半径
    //左右检测
    [SerializeField] Transform m_LeftCheck;//检测点
    [SerializeField] Transform m_RightCheck;//检测点
    [SerializeField] float k_RoundCheckRadius = .2f;//检测半径
    //下蹲相关
    bool m_wasCrouching = false;
    [Range(0, 1)]
    [SerializeField]
    private float m_CrouchSpeed = .36f;//下蹲后移动的速度为原来的多少
    [SerializeField] private Collider2D m_CrouchDisableCollider;//下蹲禁止的碰撞体

    //爬
    [SerializeField] float m_ClimbForce = 200f;
    bool onWall;

    //是否允许空中控制
    [SerializeField] private bool m_AirControl = false;

    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;//落地事件

    [System.Serializable]
    //蹲下事件
    public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent OnCrouchEvent;
    //爬墙事件
    public BoolEvent OnClimbEvent;
    //在空中事件
    // public BoolEvent OnAirEvent;

    private void Awake()
    {
        m_Rigidbody2D = this.GetComponent<Rigidbody2D>();


        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
        if (OnClimbEvent == null)
            OnClimbEvent = new BoolEvent();
    }

    private void FixedUpdate()
    {
        //检测是否从天上落在地上
        bool wasGround = m_Grounded;
        m_Grounded = false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundCheckRadius, m_GroundLayer);
        foreach (Collider2D col in hits)
        {
            if (col.gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGround && m_Rigidbody2D.velocity.y <= 0f)
                {
                    print("123");
                    OnLandEvent.Invoke();
                }
                break;
            }
        }
    }

    /// <summary>
    /// 移动函数
    /// </summary>
    /// <param name="move"></param>
    /// <param name="crouch">是否下蹲</param>
    /// <param name="jump">是否跳跃</param>
    public void Move(float move, bool crouch, bool jump, bool climb)
    {
        //如果不是蹲下的 检查头顶是不是有物体顶住了
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CellingCheck.position, k_CellingCheckRadius, m_GroundLayer))
            {
                crouch = true;
                OnCrouchEvent.Invoke(true);
            }
        }
        if (m_Grounded || m_AirControl)
        {
            if (crouch)
            {
                //之前不是蹲下 现在蹲下了
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }
                //改移速
                move *= m_CrouchSpeed;
                //禁用碰撞体
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;
                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }
        }
        //移动
        Vector2 targetVelocity = new Vector2(move, m_Rigidbody2D.velocity.y);
        m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        //角色始终朝向按键方向
        if (move < 0 && m_FacingRight || move > 0 && !m_FacingRight)
            Flip();

        //跳跃
        if (jump && m_Grounded && !crouch)
        {
            Invoke("JumpDelay", 0.5f);//不加延迟的话 检测地面会出问题
            m_Rigidbody2D.AddForce(new Vector2(0, m_JumpForce));
        }

        //爬
        onWall = climb && (Physics2D.OverlapCircle(m_LeftCheck.position, k_RoundCheckRadius, m_GroundLayer) ||
                 Physics2D.OverlapCircle(m_RightCheck.position, k_RoundCheckRadius, m_GroundLayer));
        if (onWall)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_ClimbForce);
            OnClimbEvent.Invoke(true);
        }
        else
        {
            OnClimbEvent.Invoke(false);
        }
    }
    public void JumpDelay()
    {
        m_Grounded = false;
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