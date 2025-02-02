﻿using UnityEngine;
using System.Collections;

public partial class PlayerMachine : SuperStateMachine {

    public bool DebugGui;

    public float InitialRotation = 0.0f;

    public PlayerVerySmartCamera SmartCamera;

    public Transform AnimatedMesh;
    public AdditiveTransform ChestBone;
    public ParticleSystem RunSmokeEffect;
    public GameObject GroundPoundSmokeEffect;
    public GameObject WallKickSmokeEffect;
    public GameObject GroundFallSmokeEffect;
    public GameObject WallHitStarEffect;
    public GameObject EnemyHitEffect;
    public GameObject TakeDamageEffect;
    public GameObject UpgradeEffect;
    public Transform ScaleAnimator;

    public LayerMask EnemyLayerMask;

    public float runSpeed = 7.0f;
    public float turnSpeed = 10.0f;
    public float maxSlideSpeed = 26.0f;
    public float jumpDamageHeight = 20.0f;
    public float acquireGroundDistance = 0.05f;
    public float maintainGroundDistance = 0.5f;

    private Animation anim;

    private PlayerInput input;
    private PlayerStatus status;
    private PlayerSound sound;
    private SuperCharacterController controller;
    private ShaderSwapper transparencyShaderSwapper;
    private MaterialSwapper goldMaterialSwapper;
    private TransparencyFade transparencyFade;

    private Vector3 moveDirection;
    private Vector3 lookDirection;
    private Vector3 artUpDirection;

    private JumpProfile currentJumpProfile;

    private float moveSpeed;
    private float verticalMoveSpeed;
    private float lastLandTime;

    private float chestBendAngle;
    public float chestTwistAngle;

    private bool isTakingFallDamage;
    private bool goldPlayer;

    /// <summary>
    /// Jump profiles
    /// </summary>

    private JumpProfile jumpStandard = new JumpProfile {
        CanDive = true,
        CanKick = true,
        CanControlHeight = true,
        JumpHeight = 2.0f,
        Animation = "jump2"
    };

    private JumpProfile jumpDouble = new JumpProfile {
        CanDive = true,
        CanKick = true,
        CanControlHeight = true,
        JumpHeight = 3.6f,
        Animation = "jump",
        FallAnimation = "falling_idle"
    };

    private JumpProfile jumpTriple = new JumpProfile {
        CanDive = true,
        JumpHeight = 6.0f,
        Animation = "forward_flip"
    };

    private JumpProfile jumpBackFlip = new JumpProfile {
        JumpHeight = 5.0f,
        InitialForwardVelocity = -7.0f,
        Animation = "backflip"
    };

    private JumpProfile jumpStrike = new JumpProfile {
        JumpHeight = 1.0f,
        Animation = "right_hook",
        Gravity = 45.0f
    };

    private JumpProfile jumpFastStrike = new JumpProfile {
        JumpHeight = 1.0f,
        Animation = "flying_punch",
        Gravity = 45.0f
    };

    private JumpProfile jumpKick = new JumpProfile {
        JumpHeight = 1.0f,
        Animation = "roundhouse_kick",
        Gravity = 45.0f
    };

    private JumpProfile jumpFastKick = new JumpProfile {
        JumpHeight = 1.0f,
        Animation = "flying_bicycle_kick",
        Gravity = 45.0f
    };



    private JumpProfile jumpSideFlip = new JumpProfile {
        CanDive = true,
        JumpHeight = 5.0f,
        Animation = "forward_flip"
    };

    private JumpProfile jumpLong = new JumpProfile {
        JumpHeight = 2.7f,
        InitialForwardVelocity = 4.0f,
        Gravity = 17.0f,
        Animation = "falling",
        CrossFadeTime = 0.1f
    };

    private JumpProfile jumpWall = new JumpProfile {
        CanDive = true,
        CanControlHeight = true,
        JumpHeight = 5.0f,
        InitialForwardVelocity = 7.4f,
        Animation = "jump",
        FallAnimation = "falling_idle"
    };

    private JumpProfile fallProfile = new JumpProfile {};
}
