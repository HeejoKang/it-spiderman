﻿using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    public bool MouseCamera;

    [SerializeField]
    Transform PlayerCamera;

    public bool DebugGui;

    public PlayerInputSet Current { get; private set; }

    public InputManager input;

    void Start() {
        // check for camera on starrtup
        if (PlayerCamera == null) {
            Debug.LogError("[PlayerInput]: Main camera not found");
        }
    }

    void Update() {
        // ignore too little joystick movement
        Vector3 analogMoveInput = input.MovementInputDeadZoned();

        Vector3 moveInput = Vector3.zero;

        if (analogMoveInput.x != 0) {
            moveInput += PlayerCamera.right * analogMoveInput.x;
        }

        if (analogMoveInput.y != 0) {
            moveInput += PlayerCamera.forward * analogMoveInput.y;
        }

        moveInput = Math3d.ProjectVectorOnPlane(transform.up, moveInput).normalized;

        float moveMagnitude = input.MovementMagnitudeDeadZoned();
        Vector2 cameraInput = input.CameraInputDeadZoned();


        bool jump = input.Jump();
        bool jumpDown = input.JumpDown();
        bool strike = input.Strike();
        bool strikeDown = input.StrikeDown();
        bool kick = input.Kick();
        bool kickDown = input.KickDown();

        Current = new PlayerInputSet() {
            MoveInput = moveInput,
            MoveMagnitude = moveMagnitude,
            CameraInput = cameraInput,
            Jump = jump,
            JumpDown = jumpDown,
            Strike = strike,
            StrikeDown = strikeDown,
            Kick = kick,
            KickDown = kickDown
        };
    }

    void OnGUI()
    {
        if (DebugGui)
        {
            GUI.BeginGroup(new Rect(220, 10, 160, 180));

            GUI.Box(new Rect(0, 0, 150, 180), "Player Input");
            GUI.TextField(new Rect(10, 30, 130, 20), string.Format("Move Input X: {0}", Input.GetAxis("Horizontal").ToString("F3")));
            GUI.TextField(new Rect(10, 60, 130, 20), string.Format("Move Input Y: {0}", Input.GetAxis("Vertical").ToString("F3")));
            GUI.TextField(new Rect(10, 90, 130, 20), string.Format("Move Magn: {0}", Current.MoveMagnitude.ToString("F3")));
            GUI.TextField(new Rect(10, 120, 130, 20), string.Format("Cam Input X: {0}", Input.GetAxis("AxisTwoHorizontal").ToString("F3")));
            GUI.TextField(new Rect(10, 150, 130, 20), string.Format("Cam Input Y: {0}", Input.GetAxis("AxisTwoVertical").ToString("F3")));
            GUI.EndGroup();
        }
    }
}

public struct PlayerInputSet {
    public Vector3 MoveInput;
    public Vector2 CameraInput;
    public float MoveMagnitude;
    public bool Jump;
    public bool JumpDown;
    public bool Strike;
    public bool StrikeDown;
    public bool Kick;
    public bool KickDown;
}

