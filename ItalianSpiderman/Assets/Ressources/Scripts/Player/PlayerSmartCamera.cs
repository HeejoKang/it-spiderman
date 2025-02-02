﻿using UnityEngine;
using System.Collections;

public class PlayerSmartCamera : MonoBehaviour {

    public bool DebugGui;

    [SerializeField]
    Transform target;

    [SerializeField]
    float trackerHeight = 2.0f;

    [SerializeField]
    float cameraHeight = 1.0f;

    [SerializeField]
    float maxDistance = 12.0f;

    [SerializeField]
    float minDistance = 3.0f;

    [SerializeField]
    float xSensitivity = 45.0f;

    [SerializeField]
    float ySensitivity = 45.0f;

    private PlayerMachine Player;
    private SuperCharacterController controller;
    private PlayerInput input;

    private Vector3 lastGroundedPosition;

    private float currentDistance;
    private float currentRotation;

    private Vector3 trackerPlanar;
    private Vector3 trackerVertical;
    private Vector3 verticalTrackerVelocity;

	// Use this for initialization
	void Start () {
        currentDistance = (maxDistance + minDistance) * 0.5f;

        Player = target.GetComponent<PlayerMachine>();
        controller = target.GetComponent<SuperCharacterController>();
        input = target.GetComponent<PlayerInput>();
	}

	// Update is called once per frame
	void LateUpdate () {

        currentDistance = Mathf.Clamp(currentDistance + input.Current.CameraInput.y * ySensitivity, minDistance, maxDistance);
        currentRotation -= input.Current.CameraInput.x * xSensitivity;

        float t = Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
        float maxCamDistance = Mathf.Lerp(1.5f, 7, t);

        bool airborn = PlayerAirborn();

        Vector3 verticalTrackerTarget;

        if (airborn)
        {
            Vector3 lastGroundAltitude = Math3d.ProjectPointOnLine(target.position, Vector3.up, lastGroundedPosition);
            Vector3 currentGroundAltitude = Math3d.ProjectPointOnLine(target.position, Vector3.up, controller.currentGround.Hit.point);

            bool above = PointAbovePlane(Vector3.up, lastGroundAltitude, currentGroundAltitude);

            if (above)
            {
                lastGroundAltitude = currentGroundAltitude;
            }

            float distance = Vector3.Distance(lastGroundAltitude, target.position);

            if (distance > maxCamDistance)
            {
                verticalTrackerTarget = (distance - maxCamDistance) * Vector3.up;
            }
            else
            {
                verticalTrackerTarget = Vector3.zero;
            }

            verticalTrackerTarget += lastGroundAltitude;
        }
        else
        {
            verticalTrackerTarget = Math3d.ProjectPointOnLine(target.position, Vector3.up, lastGroundedPosition);
            lastGroundedPosition = target.position;
        }

        verticalTrackerTarget = Math3d.ProjectPointOnLine(Vector3.zero, Vector3.up, verticalTrackerTarget);

        trackerVertical = Math3d.ProjectPointOnLine(verticalTrackerTarget, Vector3.up, trackerVertical);
        trackerVertical = Vector3.SmoothDamp(trackerVertical, verticalTrackerTarget, ref verticalTrackerVelocity, 0.1f);

        trackerPlanar = Math3d.ProjectPointOnPlane(Vector3.up, Vector3.zero, target.position);

        Vector3 tracker = trackerVertical + trackerPlanar;

        tracker += Vector3.up * trackerHeight;

        transform.rotation = Quaternion.AngleAxis(currentRotation, Vector3.up);

        Vector3 targetPosition = tracker - transform.forward * currentDistance + Vector3.up * cameraHeight;

        transform.position = targetPosition;
        transform.rotation = Quaternion.LookRotation(tracker - transform.position);

        if (DebugGui)
        {
            DebugDraw.DrawMarker(tracker, 1.0f, Color.cyan, 0, false);
        }
	}

    private bool PointAbovePlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
    {
        Vector3 direction = point - planePoint;
        return Vector3.Angle(direction, planeNormal) < 90;
    }

    private bool PlayerAirborn()
    {
        if ((PlayerMachine.PlayerStates)Player.currentState == PlayerMachine.PlayerStates.Jump ||
            (PlayerMachine.PlayerStates)Player.currentState == PlayerMachine.PlayerStates.Fall ||
            (PlayerMachine.PlayerStates)Player.currentState == PlayerMachine.PlayerStates.AirKnockback ||
            (PlayerMachine.PlayerStates)Player.currentState == PlayerMachine.PlayerStates.Dive ||
            (PlayerMachine.PlayerStates)Player.currentState == PlayerMachine.PlayerStates.GroundPound ||
            (PlayerMachine.PlayerStates)Player.currentState == PlayerMachine.PlayerStates.GroundPoundPrepare ||
            (PlayerMachine.PlayerStates)Player.currentState == PlayerMachine.PlayerStates.SFlip)
        {
            return true;
        }

        return false;
    }
}
