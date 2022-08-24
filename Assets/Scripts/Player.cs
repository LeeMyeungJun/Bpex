using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RigCharacterAnimEvent : UnityEvent<PlayerMovementAdvanced.MovementState, PlayerMovementAdvanced.MovementState> { }
[RequireComponent(typeof(PlayerMovementAdvanced))]
public class Player : MonoBehaviour
{
    AnimateCharacter animCharacter;
    PlayerMovementAdvanced movement;
    public RigCharacterAnimEvent onAnimChange;
    new CameraMovement camera;
    WallRunningAdvanced wallrun;
    private void Start()
    {
        if (GetComponentInChildren<AnimateCharacter>())
            animCharacter = GetComponentInChildren<AnimateCharacter>();

        camera = GetComponentInChildren<CameraMovement>();
        camera.SetLockRot(false);

        if (GetComponentInChildren<WallRunningAdvanced>())
            wallrun = GetComponentInChildren<WallRunningAdvanced>();

        movement = GetComponentInChildren<PlayerMovementAdvanced>();
    }
    public void ChangeStatus(PlayerMovementAdvanced.MovementState prevState, PlayerMovementAdvanced.MovementState newState)
    {
        if (prevState == newState) return;

        if (onAnimChange != null)
            onAnimChange.Invoke(prevState, newState);
    }
    public void AddToStatusChange(UnityAction<PlayerMovementAdvanced.MovementState, PlayerMovementAdvanced.MovementState> action)
    {
        if (onAnimChange == null)
            onAnimChange = new RigCharacterAnimEvent();

        onAnimChange.AddListener(action);
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
    private void Update()
    {
        animCharacter.UpdateMoveAnim(movement.moveDirection.normalized, movement.isSprinting(), movement.isAir(), movement.grounded);
    }

}

public class ColliderInfo
{
    public Vector3 center;
    public float height;
    public ColliderInfo(Vector3 c, float h)
    {
        center = c;
        height = h;
    }
}