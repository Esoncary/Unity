
using System;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController controller;
    Animator animator;
    //移动
    public float moveSpeed = 250;
    float m_move;
    //跳跃相关
    bool isJump;
    //蹲下相关
    bool isCrouch;

    //爬相关
    bool isClimb;
    float dirction = 0;

    void Awake()
    {
        controller = this.GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        controller.OnJumpEvent.AddListener((v) =>
        {
            // print($"触发{v}");
            //音效
            if (v)
            {
                //起跳
            }
            else
            {
                //落地
            }

            //动画
            animator.SetBool("Jump", v);
        });
        controller.OnMoveEvent.AddListener(() =>
        {
            //动画
            animator.SetFloat("Speed", Math.Abs(m_move));
        });
        controller.OnCrouchEvent.AddListener((v) =>
        {
            //动画
            print(isCrouch);
            animator.SetBool("Crouch", v);
        });
        controller.OnClimbEvent.AddListener((v) =>
        {
            //动画
            animator.SetBool("Climb", v);
        });
        controller.OnFallEvent.AddListener((v) =>
        {
            //动画
            animator.SetBool("Fall", v);
        });
    }



    void Update()
    {
        //移动
        float inputX = Input.GetAxisRaw("Horizontal");
        m_move = inputX * moveSpeed;


        //按下空格跳跃
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJump = true;
        }

        //按下control蹲下 放开站起来
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouch = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouch = false;
        }

        //爬
        if (Input.GetKeyDown(KeyCode.C))
            isClimb = true;
        else if (Input.GetKeyUp(KeyCode.C))
            isClimb = false;
        dirction = Input.GetAxisRaw("Vertical");
    }
    void FixedUpdate()
    {
        //移动
        controller.Move(m_move * Time.fixedDeltaTime);
        //跳跃
        controller.Jump(isJump);
        isJump = false;
        //蹲下
        controller.Crouch(isCrouch);
        //爬
        controller.Climb(isClimb, dirction);
    }
}
