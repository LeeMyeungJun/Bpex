using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Status { idle, walking, crouching, sprinting, sliding, climbingLadder, wallRunning, vaulting, grabbedLedge, climbingLedge, surfaceSwimming, underwaterSwimming }
public class StatusEvent : UnityEvent<Status, Func<IKData>> { }
public class CharacterAnimEvent : UnityEvent<Status, Status> { }
public class PlayerInfo
{
    public float rayDistance;
    public float radius;
    public float height;
    public float halfradius;
    public float halfheight;

    public PlayerInfo(float r, float h)
    {
        radius = r; height = h;
        halfradius = r / 2f; halfheight = h / 2f;
        rayDistance = halfheight + radius + .175f;
    }
}

public class IKData
{
    public Vector3 handPos;
    public Vector3 handEulerAngles;

    public Vector3 armElbowPos;
    public Vector3 armLocalPos;

    public IKData()
    {
        handPos = Vector3.zero;
        handEulerAngles = Vector3.zero;
        armElbowPos = Vector3.zero;
        armLocalPos = Vector3.zero;
    }

    public TransformData HandData()
    {
        TransformData data = new TransformData();
        data.position = handPos;
        data.eulerAngles = handEulerAngles;
        return data;
    }
}