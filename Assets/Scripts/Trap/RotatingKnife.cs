using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingKnife : MonoBehaviour
{
    public int rotSpeed = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
    }
}
