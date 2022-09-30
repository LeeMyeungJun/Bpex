using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    // Start is called before the first frame update
    bool destroy = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (destroy)
        {
           // yield return new WaitForSeconds(3);

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Player")
        {

           Invoke("Destroy",2);
        }
    }

    void Destroy()
    {
        gameObject.SetActive(false);

        Invoke("Spawn", 2);
    }

    void Spawn()
    {
        gameObject.SetActive(true);
    }
}
