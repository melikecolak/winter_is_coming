using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float drag = 6f;

    [Header("References")]
    public Transform orientation;

    private Rigidbody rb;
    private Animator animator;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    private void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        
        animator = GetComponentInChildren<Animator>();
        rb.linearDamping = drag;
    }

    private void Update()
    {
        
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool isMoving = horizontalInput != 0 || verticalInput != 0;
        animator.SetBool("isRunning", isMoving);
    }

    private void FixedUpdate()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

   
        if (horizontalInput == 0 && verticalInput == 0)
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }
}