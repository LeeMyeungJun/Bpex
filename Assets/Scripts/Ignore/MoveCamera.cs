using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    //public Transform cameraPosition;

    //private void Update()
    //{
    //    transform.position = cameraPosition.position;
    //}

    //private void OnDrawGizmos()
    //{
    //    if(cameraPosition != null)
    //    {
    //        transform.position = cameraPosition.position;
    //    }
    //}

    public float adjustSpeed = 15f;
    Vector3 offset;

    private void Start()
    {
        offset = transform.localPosition;
    }

    public void UpdateLevel(float level)
    {
        Vector3 camLevel = offset;
        camLevel.y += level;
        transform.localPosition = Vector3.Lerp(transform.localPosition, camLevel, Time.deltaTime * adjustSpeed);
    }
}
