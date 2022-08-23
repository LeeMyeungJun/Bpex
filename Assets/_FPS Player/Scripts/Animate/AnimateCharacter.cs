using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCharacter : MonoBehaviour
{
    Player player;

    Animator ani;
    public float moveSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        ani = GetComponent<Animator>();
        if (GetComponentInParent<Player>())
            player = GetComponentInParent<Player>();
        player.AddToStatusChange(CharacterAnimChange);
    }

    // send input and other state parameters to the animator

    public void UpdateMoveAnim(Vector3 move, bool sprint,bool jumped, bool grounded)
    {
        move = transform.InverseTransformDirection(move);

        float speed = (!sprint) ? 0.5f : 1.0f;

        ani.SetFloat("x", move.x * speed, 0.1f, Time.deltaTime);
        ani.SetFloat("y", move.y * speed, 0.1f, Time.deltaTime);
        ani.SetFloat("z", move.z * speed, 0.1f, Time.deltaTime);

        ani.SetBool("IsGrounded", grounded);
        ani.SetBool("IsJumping", jumped);
    }

    public void CharacterAnimChange(PlayerMovementAdvanced.MovementState prevStatus, PlayerMovementAdvanced.MovementState newStatus)
    {
        ani.SetBool("IsWallRunning", false);
        ani.SetBool("IsCrouch", false);
        ani.SetBool("IsSliding", false);
        ani.SetBool("IsVault", false);
        ani.SetBool("IsLadder", false);
        ani.SetBool("IsLedge", false);
        ani.SetBool("IsLedgeUp", false);

        player.LockRot(false);

        switch (newStatus)
        {
            case PlayerMovementAdvanced.MovementState.walking:
                break;
            case PlayerMovementAdvanced.MovementState.sprinting:
                break;
            case PlayerMovementAdvanced.MovementState.wallrunning:
                ani.SetBool("IsWallRunning", true);
                ani.SetFloat("WallRunDir", player.getWallrunDir());
                break;
            case PlayerMovementAdvanced.MovementState.crouching:
                ani.SetBool("IsCrouch", true);
                break;
            case PlayerMovementAdvanced.MovementState.sliding:
                player.LockRot(true);
                ani.SetBool("IsSliding", true);
                break;
            case PlayerMovementAdvanced.MovementState.air:
                break;
        }
        //switch (newStatus)
        //{
        //    case Status.idle:
        //        break;
        //    case Status.walking:
        //        break;
        //    case Status.crouching:
        //        ani.SetBool("IsCrouch", true);
        //        break;
        //    case Status.sprinting:
        //        break;
        //    case Status.sliding:
        //        player.LockRot(true);
        //        ani.SetBool("IsSliding", true);
        //        break;
        //    case Status.climbingLadder:
        //        player.LockRot(true);
        //        ani.SetBool("IsLadder", true);
        //        break;
        //    case Status.wallRunning:
        //        ani.SetBool("IsWallRunning", true);
        //        ani.SetFloat("WallRunDir", player.getWallrunDir());
        //        break;
        //    case Status.vaulting:
        //        ani.SetBool("IsVault", true);
        //        break;
        //    case Status.grabbedLedge:
        //        player.LockRot(true);
        //        ani.SetBool("IsLedge", true);
        //        break;
        //    case Status.climbingLedge:
        //        player.LockRot(true);
        //        ani.SetBool("IsLedgeUp", true);
        //        break;
        //    case Status.surfaceSwimming:
        //        break;
        //    case Status.underwaterSwimming:
        //        break;
        //}

        //ani.CrossFade(, 0.25f);
    }
}
