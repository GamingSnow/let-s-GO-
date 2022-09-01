using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMotion : MonoBehaviour
{
    private const float LANE_DISTANCE = 3.0f;
    private const float TURN_SPEED = 0.005f;

    //
    private bool isRunning = false;

    private CharacterController controller;
    private float jumpForce = 7.0f;
    private float gravity = 12.0f;
    private float verticalVelocity;
   
    private int desiredLane = 1; //0 = Left, 1 = middle, 2 = right

    //speed midifier
    private float originalSpeed = 7.0f;
    private float speed;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 5.5f;
    private float speedIncreaseAmount = 0.1f;

    // Start is called before the first frame update
    private void Start()
    {
        speed = originalSpeed;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isRunning)
            return;

        if(Time.time - speedIncreaseLastTick > speedIncreaseTime)
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }
        // gather the inputs on  which lane we should be
        if (mobileInput.Instance.SwipeLeft)
            MoveLane(false);
        if (mobileInput.Instance.SwipeRight)
            MoveLane(true);

        // calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * Vector3.forward;

        if (desiredLane == 0)
            targetPosition += Vector3.left * LANE_DISTANCE;
        else if (desiredLane == 2)
            targetPosition += Vector3.right * LANE_DISTANCE;

        //let's calculate our move delta
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;

        bool isGrounded = IsGrounded();

        //calculate y
        if (isGrounded) //if grounded
        {
            verticalVelocity = -0.1f;
            if (mobileInput.Instance.SwipeUp)
            {
                //jump
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity -= (gravity * Time.deltaTime);

            // fast falling mechanic
            if (mobileInput.Instance.SwipeDown)
            {
                verticalVelocity = -jumpForce;
            }
        }
        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        controller.Move(moveVector * Time.deltaTime);
        //rotate the sphere to where it is going
        Vector3 dir = controller.velocity;
        dir.y = 0;
        transform.forward = Vector3.Lerp(transform.forward, dir, TURN_SPEED);
    }
    private void MoveLane(bool goingRight)
    {
        desiredLane += (goingRight) ? 1 : -1;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);
    }

    private bool IsGrounded()
    {
        Ray groundRay = new Ray(new Vector3(controller.bounds.center.x, (controller.bounds.center.y - controller.bounds.extents.y) + 0.2f, controller.bounds.center.z), Vector3.down);
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.cyan, 0.1f);

        return (Physics.Raycast(groundRay, 0.2f + 0.1f));
        
    }

    public void StartRunning()
    {
        isRunning = true;
    }

    private void Crash()
    {
        isRunning = false;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                Crash();
            break;
        }
    }
}
