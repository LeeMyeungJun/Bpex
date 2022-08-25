using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderAdvanced : MonoBehaviour
{

    [Header("References")]
    private PlayerMovementAdvanced pm;
    private Rigidbody rb;


    [SerializeField]
    private LayerMask ladderLayer;
    Vector3 ladderNormal = Vector3.zero;
    Vector3 lastTouch = Vector3.zero;
    public float LadderClimbSpeed;
    public float halfheight;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
    }


    private void Update()
    {
        Check();
        //StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.climbingLadder)
            LadderMovement();
    }

    private void Check()
    {
        //Check for ladder all across player (so they cannot use the side)
        //bool right = Physics.Raycast(transform.position + (transform.right * .5f), transform.forward, 1.0f + 0.125f, ladderLayer);
        //bool left = Physics.Raycast(transform.position - (transform.right * .5f), transform.forward, 1.0f + 0.125f, ladderLayer);

        //if (Physics.Raycast(transform.position, transform.forward, out var hit, 1.0f + 0.125f, ladderLayer) && right && left)
        if (Physics.Raycast(transform.position, transform.forward, out var hit, 1.0f + 0.125f, ladderLayer) )
        {
            if (hit.normal != hit.transform.forward) return;

            ladderNormal = -hit.normal;
            if (pm.hasObjectInfront(0.05f, ladderLayer) && Input.GetAxis("Vertical") > 0.02f)
                pm.climbingLadder = true;
        }
        else
        {
            pm.climbingLadder = false;
        }
    }
    private void LadderMovement()
    {
        Vector2 i = Vector2.zero;
        i.x = Input.GetAxis("Horizontal");
        i.y = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(i.x, i.y, 0);
        Vector3 move = Vector3.Cross(Vector3.up, ladderNormal).normalized;
        move *= input.x;
        move.y = input.y * LadderClimbSpeed;

        bool goToGround = (move.y < -0.02f && pm.grounded);

        if (Input.GetKey(pm.jumpKey))
        {
            pm.Jump((-ladderNormal + Vector3.up).normalized, 5f);
            pm.climbingLadder = false;
            //player.ChangeStatus(Status.walking);
        }

        if (!pm.hasObjectInfront(0.05f, ladderLayer) || goToGround)
        {
            pm.climbingLadder = false;
            Vector3 pushUp = ladderNormal;
            pushUp.y = 0.25f;
            rb.velocity = pushUp * LadderClimbSpeed;
        }
        else
            rb.velocity = move * LadderClimbSpeed;
    }

}
