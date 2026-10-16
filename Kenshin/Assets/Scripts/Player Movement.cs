using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 7f;
    private float rotationSpeed = 12f;
    private float jumpHeight = 2f;
    private float gravity = -9.81f;

   [SerializeField] private Transform groundCheck;
    [SerializeField]private float groundDistance = 0.4f;
    [SerializeField]private LayerMask groundMask;

    private CharacterController controller;
   [SerializeField] private Transform cameraTransform;
    private Vector3 velocity;
    private bool isGrounded;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        Movement();
        JumpAndGravity();

    }

    private void GroundCheck()
    {
     /* Checks if the groundcheck object is colliding with anything
      on ground layer */
      if (groundCheck != null)
      {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
      }
      else
      {
        // precaution if noah forgets
       isGrounded = controller.isGrounded;
      }
      // makes jumping less buggy
      if (isGrounded && velocity.y < 0)
      {
        velocity.y = -2f;
      }
    }
    private void Movement()
    {
        // Raw = WASD
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
         Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

         if (inputDirection.magnitude >= 0.1f && cameraTransform != null)
         {
            //aligns to face camera direction
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            //prevents player from running upwards or downwards
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
           // Calculate the finalized direction vector
            Vector3 moveDirection = (camForward * vertical) + (camRight * horizontal);

            // Execute lateral movement
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
            //Rotates character into the direction  their walking
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

         }
    }
    private void JumpAndGravity()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
        //Calculates velocity required to reach jump height
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
          // Apply downward velocity over time (Gravity acceleration formula: change in v = a * t)
        velocity.y += gravity * Time.deltaTime;
         controller.Move(velocity * Time.deltaTime);
    }
}
 