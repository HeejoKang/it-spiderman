﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SuperCharacterController))]
public class VillainMachine : EnemyMachine {

    public GameObject ParticleDeathEffect;

    public AudioClip Footstep;
    public AudioClip FlattenedSound;
    public AudioClip Alerted;

    public float AttackSpeed = 3.0f;
    public float WanderSpeed = 1.0f;
    public float WanderTime = 5.0f;
    public float FieldOfView = 70.0f;
    public float SightDistance = 7.0f;
    public float MaintainSightDistance = 7.0f;
    public float ResumeWanderTime = 2.0f;
    public float KnockbackGravity = 25.0f;
    public float WanderDistance = 8.0f;

    private Vector3 initialPosition;

    public enum VillainStates
    {
        Wander,
        Alert,
        Attack,
        Damage,
        Knockback,
        Dying,
        Fall
    }

    protected override void Start()
    {
        base.Start();

        initialPosition = transform.position;

        currentState = VillainStates.Wander;
    }

    private float lastDirectionChangeTime;
    private Vector3 targetDirection;

    public override bool GetStruck(Vector3 direction, float force, float lift, float deathTimer = 0)
    {
        if ((VillainStates)currentState == VillainStates.Knockback)
        {
            return false;
        }

        moveDirection = direction.normalized * force + controller.up * lift;

        if (deathTimer != 0)
            Invoke("DestroyVillain", deathTimer);

        currentState = VillainStates.Knockback;

        return true;
    }

    public override void KillEnemy()
    {
        if ((VillainStates)currentState != VillainStates.Dying)
        {
            currentState = VillainStates.Dying;
        }
    }

    private void DestroyVillain()
    {
        if (server != null)
            server.PatronDeath();

        if (!isGold)
        {
            Instantiate(ParticleDeathEffect, transform.position, Quaternion.identity);

            if (canDropObjectOnDeath)
                Instantiate(ObjectDroppedOnDeath, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(GoldParticleDeath, transform.position, Quaternion.identity);
            Instantiate(ObjectDroppedOnDeath, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private bool Attack(float pushbackSpeed)
    {
        bool tookDamage = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position + controller.up * controller.radius, controller.radius, playerLayer);

        foreach (var col in colliders)
        {
            tookDamage = col.GetComponent<PlayerMachine>().GroundDamageLight(1, transform.position, pushbackSpeed, false);
        }

        return tookDamage;
    }

    IEnumerator Footsteps()
    {
        float speed = 0.15f;

        float lastStepTime = Time.time - speed;

        while (true)
        {
            if (SuperMath.Timer(lastStepTime, speed))
            {
                GetComponent<AudioSource>().PlayOneShot(Footstep, 0.5f);
                lastStepTime = Time.time;
            }

            yield return 0;
        }
    }

    void Fall_EnterState()
    {
        controller.DisableClamping();
        controller.DisableSlopeLimit();
    }

    void Fall_SuperUpdate()
    {
        moveDirection -= controller.up * KnockbackGravity * Time.deltaTime;

        if (IsGrounded(0.15f, false))
        {
            currentState = VillainStates.Wander;
        }
    }

    void Wander_EnterState()
    {
        controller.EnableClamping();
        controller.EnableSlopeLimit();

        targetDirection = lookDirection;

        anim.CrossFade("walking_inPlace", 0.25f);
    }

	void Wander_SuperUpdate() 
    {
        if (!IsGrounded(0.5f, true))
        {
            currentState = VillainStates.Fall;
            return;
        }

        if (Attack(4.0f))
        {
            currentState = VillainStates.Alert;
            return;
        }

        Vector3 direction = target.position - transform.position;

        direction = Math3d.ProjectVectorOnPlane(controller.up, direction);

        float distance = Vector3.Distance(target.position, transform.position);

        if (Vector3.Angle(direction, lookDirection) < FieldOfView && distance < SightDistance)
        {
            currentState = VillainStates.Alert;
            return;
        }

        if (Vector3.Distance(transform.position, initialPosition) > WanderDistance)
        {
            targetDirection = Math3d.ProjectVectorOnPlane(controller.up, (initialPosition - transform.position).normalized);
        }

        if (Time.time > lastDirectionChangeTime + WanderTime + Random.Range(0f, 2f))
        {
            targetDirection = Quaternion.AngleAxis(Random.Range(30, 70) * Mathf.Sign(Random.Range(-1, 1)), controller.up) * targetDirection;
            lastDirectionChangeTime = Time.time;
        }

        float accleration = moveSpeed > WanderSpeed ? 10.0f : 1.0f;

        moveSpeed = Mathf.MoveTowards(moveSpeed, WanderSpeed, accleration * Time.deltaTime);

        lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, 90 * Mathf.Deg2Rad * Time.deltaTime, 0);

        moveDirection = lookDirection * moveSpeed;
        anim.Play("walking_inPlace");
    }

    void Alert_EnterState()
    {
        moveDirection = Vector3.zero;

        GetComponent<AudioSource>().PlayOneShot(Alerted);

        anim.Play("jump");
    }

    void Alert_SuperUpdate()
    {
        if (!IsGrounded(0.5f, true))
        {
            currentState = VillainStates.Fall;
            return;
        }

        Attack(4.0f);

        Vector3 direction = target.position - transform.position;

        direction = Math3d.ProjectVectorOnPlane(controller.up, direction);

        lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, 180 * Mathf.Deg2Rad * Time.deltaTime, 0);

        if (Time.time > anim["jump"].length + timeEnteredState)
        {
            currentState = VillainStates.Attack;
        }
    }

    float lastSeenTime;

    void Attack_EnterState()
    {
        StartCoroutine(Footsteps());

        lastSeenTime = Time.time;

        anim.CrossFade("running_inPlace", 0.1f);
    }

    void Attack_SuperUpdate()
    {
        if (!IsGrounded(0.5f, true))
        {
            currentState = VillainStates.Fall;
            return;
        }

        if (Attack(8.0f))
        {
            //anim.Play("uppercut");
            currentState = VillainStates.Alert;
            return;
        }

        Vector3 direction = target.position - transform.position;

        direction = Math3d.ProjectVectorOnPlane(controller.up, direction);

        float distance = Vector3.Distance(target.position, transform.position);

        if (distance > MaintainSightDistance)
        {
            currentState = VillainStates.Wander;
            return;
        }

        if (Vector3.Angle(direction, lookDirection) < FieldOfView)
        {
            lastSeenTime = Time.time;
        }

        if (SuperMath.Timer(lastSeenTime, ResumeWanderTime))
        {
            currentState = VillainStates.Wander;
            return;
        }

        lookDirection = Vector3.RotateTowards(lookDirection, direction, 70 * Mathf.Deg2Rad * Time.deltaTime, 0);

        moveSpeed = Mathf.MoveTowards(moveSpeed, AttackSpeed, 5.0f * Time.deltaTime);

        moveDirection = lookDirection * moveSpeed;
        anim.Play("running_inPlace");
    }

    void Attack_ExitState()
    {
        StopAllCoroutines();
    }

    float currentScale;

    void Dying_EnterState()
    {
        Alive = false;
        moveDirection = Vector3.zero;
        anim.Play("falling_back");
        //currentScale = AnimatedMesh.localScale.y;

        foreach (AnimationState a in anim)
        {
            if (a.enabled)
            {
                //a.speed = 0;
                break;
            }
        }

        anim.Play("falling_back");
        GetComponent<AudioSource>().PlayOneShot(FlattenedSound);
    }

    void Dying_SuperUpdate()
    {
        //currentScale = Mathf.MoveTowards(currentScale, 0.2f, 6.0f * Time.deltaTime);

        //AnimatedMesh.localScale = new Vector3(AnimatedMesh.localScale.x, currentScale, AnimatedMesh.localScale.z);

        anim.Play("falling_back");
        if (Time.time > timeEnteredState + 1.6f)
        {
            DestroyVillain();
        }
    }

    void Knockback_EnterState()
    {
        controller.DisableClamping();
        controller.DisableSlopeLimit();

        foreach (AnimationState a in anim)
        {
            if (a.enabled)
            {
                //a.speed = 0;
                break;
            }
        }
    }

    void Knockback_SuperUpdate()
    {
        moveDirection -= controller.up * KnockbackGravity * Time.deltaTime;

        anim.Play("falling_back");
        if (Vector3.Angle(moveDirection, controller.up) > 90 && controller.collisionData.Count > 0)
        {
            DestroyVillain();
        }
    }
}
