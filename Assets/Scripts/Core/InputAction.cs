using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WOT.Core;

public class InputAction : Singleton<InputAction>
{
    public bool isPCControls = false;
    
    // Inputs 
    // _movement
    public Vector2 _moveAction;
    public Vector2 _mouseAction;
    public bool meleAttack = false;
    
    public bool skill_1 = false;
    public bool skill_2 = false;
    public bool skill_3 = false;

    public bool lockOnTarget = false;
    public bool isShiftClicked = false;
    public bool _jump = false;

    public Joystick moveJoystick;
    public Joystick mouseJoystick;


    private void Start()
    {


        if (isPCControls)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void Update()
    {
        if (isPCControls)
            PCInput();
        else
            UIInputs();
    }


    public void PCInput()
    {
        _moveAction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _mouseAction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.K))
        {
            meleAttack = true;
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.K))
        {
            meleAttack = false;
        }

        if (Input.GetMouseButtonDown(2))
        {
            lockOnTarget = true;
            //EventManager.Instance.OnMeleAttackTrigger();
        }
        else if (Input.GetMouseButtonUp(2))
        {
            lockOnTarget = false;
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            skill_1 = true;
            //EventManager.Instance.OnSkillAttackTrigger(1);
        }
        else if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            skill_1 = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            skill_2 = true;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            skill_2 = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            skill_3 = true;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            skill_3 = false;
        }


        if (Input.GetKeyDown(KeyCode.T))
        {

        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isShiftClicked = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isShiftClicked = false;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jump = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            _jump = false;
        }
    }


    public void UIInputs()
    {
        _moveAction = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
        _mouseAction = new Vector2(mouseJoystick.Horizontal, mouseJoystick.Vertical);
        UIManager.Instance.meleAttack.onClick.AddListener(() => { meleAttack = true; });
        UIManager.Instance.skill1Attack.onClick.AddListener(() => { skill_1 = true; });
        UIManager.Instance.skill2Attack.onClick.AddListener(() => { skill_2 = true; });
        UIManager.Instance.skill3Attack.onClick.AddListener(() => { skill_3 = true; });
    }





}
