using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour // Not just a movement script.
{
     public float Health, MaxHealth;
     [SerializeField]private HealthScript healthBar;



    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    

   [SerializeField] private Transform groundCheck;
    [SerializeField]private float groundDistance = 0.4f;
    [SerializeField]private LayerMask groundMask;

    private CharacterController controller;
    [SerializeField] private Transform cameraTransform;
    private Vector3 velocity;
    private bool isGrounded;
    private bool jumpRequested;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      healthBar.SetMaxHealth(MaxHealth);
      healthBar.SetHealth(Health); // sync the bar with the starting health
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
       
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            Debug.Log("Jump requested");
        }

        JumpAndGravity();

        // TEST: H = take damage, J = heal (remove when real damage/healing exists)
        if (Input.GetKeyDown(KeyCode.H))
        {
            ChangeHealth(-2f);
            Debug.Log("Damaged! Health = " + Health);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            ChangeHealth(2f);
            Debug.Log("Healed! Health = " + Health);
        }
      }

    private void GroundCheck()
    {
     /* Checks if the groundcheck object is colliding with anything
      on ground layer */
      if (groundCheck != null)
      {
        // Also trust the CharacterController, so floors on the wrong layer still count as ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask) || controller.isGrounded;
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
            //aligns to face camera direction//prevents player from running upwards or downwards
            camForward.y = 0f;
            camRight.y = 0f;
            //prevents player from running upwards or downwards
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
         if (jumpRequested && isGrounded)
        {
        //Calculates velocity required to reach jump height
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (jumpRequested)
        {
            jumpRequested = false;
        }
        
          // Apply downward velocity over time (Gravity acceleration formula: change in v = a * t)
        velocity.y += gravity * Time.deltaTime;
         controller.Move(velocity * Time.deltaTime);
    }
    // Adds healthChange to Health (use a negative number for damage)
    public void ChangeHealth(float healthChange)
    {
     Health += healthChange;
     Health = Mathf.Clamp(Health, 0, MaxHealth);

     healthBar.SetHealth(Health);
    }
}
 