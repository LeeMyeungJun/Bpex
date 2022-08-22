using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerRigidbodyMovement))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerRigidbodyController : MonoBehaviour
{
    public Status status;
    public LayerMask collisionLayer; //Default
    public float crouchHeight = 1.7f;
    public float unCrouchHeight = 1.98f;
    public PlayerInfo info;
    [SerializeField]
    private float sprintTime = 6f;
    [SerializeField]
    private float sprintReserve = 4f;
    [SerializeField]
    private float sprintMinimum = 2f;

    new CameraMovement camera;
    PlayerRigidbodyMovement movement;
    PlayerInput playerInput;
    AnimateLean animateLean;
    AnimateCameraLevel animateCamLevel;
    //AnimateCharacter animCharacter;

    bool canInteract;
    bool forceSprintReserve = false;

    float crouchCamAdjust;
    float stamina;

    public StatusEvent onStatusChange;
    public CharacterAnimEvent onAnimChange;
    List<MovementRigidbodyType> movements;
    WallrunMovementRb wallrun;
    //SurfaceSwimmingMovement swimming;


    //수정중
    float UnCrouchCenter = 0.13f;
    float CrouchCenter = 0.0f;
    CapsuleCollider capsuleCollider = null; 

    public void ChangeStatus(Status s)
    {
        if (status == s) return;

        if (onAnimChange != null)
            onAnimChange.Invoke(status, s);

        status = s;

        if (onStatusChange != null)
            onStatusChange.Invoke(status, null);



    }
    public void ChangeStatus(Status s, Func<IKData> call)
    {
        if (status == s) return;
        status = s;

        if (onAnimChange != null)
            onAnimChange.Invoke(status, s);

        if (onStatusChange != null)
            onStatusChange.Invoke(status, call);
    }

    public void AddToStatusChange(UnityAction<Status, Func<IKData>> action)
    {
        if (onStatusChange == null)
            onStatusChange = new StatusEvent();

        onStatusChange.AddListener(action);
    }
    //CharacterAnimation
    public void AddToStatusChange(UnityAction<Status, Status> action)
    {
        if (onAnimChange == null)
            onAnimChange = new CharacterAnimEvent();

        onAnimChange.AddListener(action);
    }

    public void AddMovementType(MovementRigidbodyType move)
    {
        if (movements == null) movements = new List<MovementRigidbodyType>();
        move.SetPlayerComponents(movement, playerInput);

        if ((move as WallrunMovementRb) != null) //If this move type is a Wallrunning
            wallrun = (move as WallrunMovementRb);
        //else if ((move as SurfaceSwimmingMovementRb) != null) //If this move type is a Surface Swimming
        //    swimming = (move as SurfaceSwimmingMovementRb);

        movements.Add(move);
    }

    //public SurfaceSwimmingMovement GetSwimmingMovement()
    //{
    //    return swimming;
    //}

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        movement = GetComponent<PlayerRigidbodyMovement>();
        capsuleCollider = GetComponent<CapsuleCollider>();


        camera = GetComponentInChildren<CameraMovement>();
        if (GetComponentInChildren<AnimateLean>())
            animateLean = GetComponentInChildren<AnimateLean>();
        if (GetComponentInChildren<AnimateCameraLevel>())
            animateCamLevel = GetComponentInChildren<AnimateCameraLevel>();
        //if (GetComponentInChildren<AnimateCharacter>())
        //    animCharacter = GetComponentInChildren<AnimateCharacter>();

    }
    private void Start()
    {
        movement.AddToReset(() => { status = Status.walking; });
        info = new PlayerInfo(0.5f, unCrouchHeight); // 캐릭터 raius 둘레랑 , 높이 키값 넣어줘야함
        crouchCamAdjust = (crouchHeight - info.height) / 2f;
        stamina = sprintTime;
        Uncrouch();

    }

    /******************************* UPDATE ******************************/
    void Update()
    {
        //Updates
        UpdateInteraction();
        UpdateMovingStatus();

        //Checks
        CheckCrouching();
        foreach (MovementRigidbodyType moveType in movements)
        {
            if (moveType.enabled)
                moveType.Check(canInteract);
        }

        //Misc
        UpdateLean();
        UpdateCamLevel();

        //animCharacter.UpdateMoveAnim(movement.moveDirection.normalized, isSprinting(), playerInput.Jump(), movement.grounded);

    }

    void UpdateInteraction()
    {
        if ((int)status >= 5)
            canInteract = false;
        else if (!canInteract)
        {
            if (movement.grounded || movement.moveDirection.y < 0)
                canInteract = true;
        }
    }

    void UpdateMovingStatus()
    {
        if (status == Status.sprinting && stamina > 0)
            stamina -= Time.deltaTime;
        else if (stamina < sprintTime)
            stamina += Time.deltaTime;

        if ((int)status <= 1 || isSprinting())
        {
            if (playerInput.input.magnitude > 0.02f)
                ChangeStatus((shouldSprint()) ? Status.sprinting : Status.walking);
            else
                ChangeStatus(Status.idle);
        }
    }

    public bool shouldSprint()
    {
        bool sprint = false;
        sprint = (playerInput.run && playerInput.input.y > 0);
        if (status != Status.sliding)
        {
            if (!isSprinting()) //If we want to sprint
            {
                if (forceSprintReserve && stamina < sprintReserve)
                    return false;
                else if (!forceSprintReserve && stamina < sprintMinimum)
                    return false;
            }
            if (stamina <= 0)
            {
                forceSprintReserve = true;
                return false;
            }
        }
        if (sprint)
            forceSprintReserve = false;
        return sprint;
    }

    void UpdateLean()
    {
        if (animateLean == null) return;
        Vector2 lean = Vector2.zero;
        if (status == Status.wallRunning)
            lean.x = getWallrunDir();
        if (status == Status.sliding)
            lean.y = -1;
        else if (status == Status.climbingLedge || status == Status.vaulting)
            lean.y = 1;
        animateLean.SetLean(lean);
    }

    void UpdateCamLevel()
    {
        if (animateCamLevel == null) return;

        float level = 0f;
        if (status == Status.crouching || status == Status.sliding || status == Status.vaulting || status == Status.climbingLedge || status == Status.underwaterSwimming)
            level = crouchCamAdjust;
        animateCamLevel.UpdateLevel(level);
    }

    public int getWallrunDir()
    {
        int wallDir = 0;
        if (wallrun != null)
            wallDir = wallrun.getWallDir();

        return wallDir;
    }

    public void LockRot(bool _lock)
    {
        camera.SetLockRot(_lock);
    }
    /*********************************************************************/


    /******************************** MOVE *******************************/
    void FixedUpdate()
    {
        foreach (MovementRigidbodyType moveType in movements)
        {
            if (status == moveType.changeTo)
            {
                moveType.Movement();
                return;
            }
        }

        DefaultMovement();
        //ani
        //animCharacter.UpdateMoveAnim(movement.moveDirection.normalized, isSprinting(), playerInput.Jump(), movement.grounded);

    }

    void DefaultMovement()
    {
        if (isSprinting() && isCrouching())
            Uncrouch();

        movement.Move(playerInput.input, isSprinting(), isCrouching());
        if (movement.grounded && playerInput.Jump())
        {
            if (status == Status.crouching)
            {
                if (!Uncrouch())
                    return;
            }

            movement.Jump(Vector3.up, 1f);
            playerInput.ResetJump();
        }
    }

    public bool isSprinting()
    {
        return (status == Status.sprinting && movement.grounded);
    }

    public bool isWalking()
    {
        if (status == Status.walking || status == Status.crouching)
            return (movement.rb.velocity.magnitude > 0f && movement.grounded);
        else
            return false;
    }
    public bool isCrouching()
    {
        return (status == Status.crouching);
    }

    void CheckCrouching()
    {
        if (!movement.grounded || (int)status > 2) return;

        if (playerInput.run)
        {
            Uncrouch();
            return;
        }

        if (playerInput.crouch)
        {
            if (status != Status.crouching)
                Crouch(true);
            else
                Uncrouch();
        }
    }

    public void Slide()
    {
        capsuleCollider.center = new Vector3(0, CrouchCenter, 0);
        capsuleCollider.height = crouchHeight;
    }


    public void Crouch(bool setStatus)
    {
        capsuleCollider.center = new Vector3(0, CrouchCenter, 0);
        capsuleCollider.height = crouchHeight;
        if (setStatus) ChangeStatus(Status.crouching);
    }

    public bool Uncrouch()
    {
        Vector3 bottom = transform.position - (Vector3.up * ((crouchHeight / 2) - info.radius));
        bool isBlocked = Physics.SphereCast(bottom, info.radius, Vector3.up, out var hit, info.height - info.radius, collisionLayer);
        if (isBlocked) return false; //If we have something above us, do nothing and return

        capsuleCollider.center = new Vector3(0, UnCrouchCenter, 0);
        capsuleCollider.height = unCrouchHeight;
        //movement.controller.height = info.height;
        ChangeStatus(Status.walking);
        return true;
    }

    public bool hasObjectInfront(float dis, LayerMask layer)
    {
        Vector3 top = transform.position + (transform.forward * 0.25f);
        Vector3 bottom = top - (transform.up * info.halfheight);

        return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.forward, dis, layer).Length >= 1);
    }

    public bool hasWallToSide(int dir, LayerMask layer)
    {
        //Check for ladder in front of player
        Vector3 top = transform.position + (transform.right * 0.25f * dir);
        Vector3 bottom = top - (transform.up * info.radius);
        top += (transform.up * info.radius);

        return (Physics.CapsuleCastAll(top, bottom, 0.25f, transform.right * dir, 0.05f, layer).Length >= 1);
    }
}
