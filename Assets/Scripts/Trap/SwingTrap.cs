using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingTrap : MonoBehaviour
{
    public int speed = 0;
    public int angle = 0;

    private float lerpTime = 0;

    void Update()
    {
        lerpTime += Time.deltaTime * speed;
        transform.rotation = Quaternion.Lerp(Quaternion.Euler(Vector3.right * angle),
                            Quaternion.Euler(Vector3.left * angle), GetLerpTParam());
    }

    float GetLerpTParam()
    {
        return (Mathf.Sin(lerpTime) + 1) * 0.5f;
    }
}
