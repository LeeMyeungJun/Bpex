using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementRigidbodyType : MonoBehaviour
{
    public Status changeTo;

    protected PlayerRigidbodyController player;
    protected PlayerRigidbodyMovement movement;
    protected PlayerInput playerInput;
    protected Status playerStatus;

    public virtual void Start()
    {
        player = GetComponent<PlayerRigidbodyController>();

        player.AddMovementType(this);
        player.AddToStatusChange(PlayerStatusChange);
    }

    public virtual void SetPlayerComponents(PlayerRigidbodyMovement move, PlayerInput input)
    {
        movement = move; playerInput = input;
    }

    public virtual void PlayerStatusChange(Status status, Func<IKData> call)
    {
        playerStatus = status;
    }

    public virtual void Movement()
    {
        //Movement info
    }

    public virtual void Check(bool canInteract)
    {
        //Check info
    }

    public virtual IKData IK()
    {
        IKData data = new IKData();
        return data;
    }
}
