using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController2D controller;

    //移动相关
    float m_move;
    public float moveSpeed = 500;
    //蹲下相关
    bool isCrouch;
    //跳跃相关
    bool isJump;
    //爬相关
    bool isClimb;
    //动画相关
    Animator animator;
    // Start is called before the first frame update
    void Awake()
    {
        controller = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();

        controller.OnLandEvent.AddListener(() =>
        {
            animator.SetBool("Jump", false);
        });
        controller.OnCrouchEvent.AddListener((v) =>
        {
            animator.SetBool("Crouch", v);
        });
        controller.OnClimbEvent.AddListener((v) =>
        {
            animator.SetBool("Climb", v);
        });
        // controller.OnAirEvent.AddListener((v) =>
        // {
        //     animator.SetBool("OnAir", v);
        // });

    }
    // Update is called once per frame
    void Update()
    {
        //移动
        float inputX = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Math.Abs(inputX));
        m_move = inputX * moveSpeed;
        //按下空格跳跃
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJump = true;
            // if (!isCrouch)
            animator.SetBool("Jump", isJump);
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


    }
    void FixedUpdate()
    {
        controller.Move(m_move * Time.fixedDeltaTime, isCrouch, isJump, isClimb);
        isJump = false;
    }
}
