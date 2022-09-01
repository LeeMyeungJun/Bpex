using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public int rotSpeed = 0;
    public Vector3 root = Vector3.zero;
    void Start()
    {
        root = root.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
        transform.Rotate(root * rotSpeed * Time.deltaTime);

    }
}
