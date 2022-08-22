using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerRigidbodyMovement : MonoBehaviour
{
    public float walkSpeed = 4.0f;
    public float runSpeed = 8.0f;
    public float crouchSpeed = 2f;
    [SerializeField]
    private float jumpSpeed = 8.0f;
    [SerializeField]
    private float gravity = 20.0f;
    [SerializeField]
    private float antiBumpFactor = .75f;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    [HideInInspector]
    public Vector3 contactPoint;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public bool playerControl = false;

    public bool grounded = false;
    public Vector3 jump = Vector3.zero;
    Vector3 jumpedDir;

    private bool forceGravity;
    private float forceTime = 0;
    private float jumpPower;
    UnityEvent onReset = new UnityEvent();

    //¼öÁ¤Áß
    //PlayerInput playerInput;
    Vector3 movement;
    public float distToGround = 1.0f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    //public void SetPlayerComponents(PlayerInput input)
    //{
    //    playerInput = input;
    //}
    public void AddToReset(UnityAction call)
    {
        onReset.AddListener(call);
    }
    public void ResetPositionTo(Vector3 resetTo)
    {
        StartCoroutine(forcePosition());
        IEnumerator forcePosition()
        {
            //Reset position to 'resetTo'
            transform.position = resetTo;
            yield return new WaitForEndOfFrame();
        }
        onReset.Invoke();
    }

    public void Update()
    {
        //Vector3 newestTransform = m_lastPositions[m_newTransformIndex];
        //Vector3 olderTransform = m_lastPositions[OldTransformIndex()];

        //Vector3 adjust = Vector3.Lerp(olderTransform, newestTransform, InterpolationController.InterpolationFactor);
        //adjust -= transform.position;

        //rb.Move(adjust);
        
        if (forceTime > 0)
            forceTime -= Time.deltaTime;
    }

    public void FixedUpdate()
    {
        if (forceTime > 0)
        {
            if (forceGravity)
                moveDirection.y -= gravity * Time.deltaTime;
            rb.velocity = moveDirection;
            GroundCheck();
            //grounded = (rb.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        }
    }


    public void Move(Vector2 input, bool sprint, bool crouching)
    {
        if (forceTime > 0)
            return;

        float speed = (!sprint) ? walkSpeed : runSpeed;
        if (crouching) speed = crouchSpeed;

        if (grounded)
        {
            moveDirection = new Vector3(input.x, -antiBumpFactor, input.y);
            moveDirection = transform.TransformDirection(moveDirection) * speed;
            UpdateJump();
        }
        else
        {
            Vector3 adjust = new Vector3(input.x, 0, input.y);
            adjust = transform.TransformDirection(adjust);
            jumpedDir += adjust * Time.fixedDeltaTime * jumpPower * 2f;
            jumpedDir = Vector3.ClampMagnitude(jumpedDir, jumpPower);
            moveDirection.x = jumpedDir.x;
            moveDirection.z = jumpedDir.z;
        }

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;
        rb.velocity = moveDirection;
        // Move the controller, and set grounded true or false depending on whether we're standing on something
        GroundCheck();
        //grounded = (rb.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }

    public void Move(Vector3 direction, float speed, float appliedGravity)
    {
        if (forceTime > 0)
            return;

        Vector3 move = direction * speed;
        if (appliedGravity > 0)
        {
            moveDirection.x = move.x;
            moveDirection.y -= gravity * Time.deltaTime * appliedGravity;
            moveDirection.z = move.z;
        }
        else
            moveDirection = move;

        UpdateJump();
        GroundCheck();
        //grounded = (rb.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }

    public void Move(Vector3 direction, float speed, float appliedGravity, float setY)
    {
        if (forceTime > 0)
            return;

        Vector3 move = direction * speed;
        if (appliedGravity > 0)
        {
            moveDirection.x = move.x;
            if (setY != 0) moveDirection.y = setY * speed;
            moveDirection.y -= gravity * Time.deltaTime * appliedGravity;
            moveDirection.z = move.z;
        }
        else
            moveDirection = move;

        UpdateJump();
        GroundCheck();
        //grounded = (rb.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }


    void GroundCheck()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position,Vector3.down,out hit,distToGround + 0.1f))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    public void Jump(Vector3 dir, float mult)
    {
        jump = dir * mult;
    }

    public void UpdateJump()
    {
        if (jump != Vector3.zero)
        {
            Vector3 dir = (jump * jumpSpeed);
            if (dir.x != 0) moveDirection.x = dir.x;
            if (dir.y != 0) moveDirection.y = dir.y;
            if (dir.z != 0) moveDirection.z = dir.z;

            Vector3 move = moveDirection;
            jumpedDir = move; move.y = 0;
            jumpPower = Mathf.Min(move.magnitude, jumpSpeed);
            jumpPower = Mathf.Max(jumpPower, walkSpeed);
        }
        else
            jumpedDir = Vector3.zero;
        jump = Vector3.zero;
    }

    public void ForceMove(Vector3 direction, float speed, float time, bool applyGravity)
    {
        forceTime = time;
        forceGravity = applyGravity;
        moveDirection = direction * speed;
    }


}



