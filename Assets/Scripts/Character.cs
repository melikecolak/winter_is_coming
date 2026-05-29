 using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [Header("Hız")]
    public float playerSpeed = 4f;
    public float sprintSpeed = 6f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 1.2f;
    public float gravityMultiplier = 2.5f;
    [Range(0f, 1f)] public float airControl = 0.5f;

    [Header("Dönüş")]
    public float rotationSpeed = 10f;

    [Header("Animasyon")]
    public float speedDampTime = 0.1f;

    [Header("Çömelme")]
    public float crouchColliderHeight = 1.35f;

    [Header("Ayak Sesi")]
    public AudioClip footstepClip;
    public float walkStepInterval   = 0.15f;
    public float sprintStepInterval = 0.1f;
    public LayerMask groundLayer;

    [Header("Ground Check")]
    public LayerMask groundLayers;
    [SerializeField] float groundCheckOffset = 0.1f;  // transform.position'dan yukarı offset
    [SerializeField] float groundCheckRadius = 0.28f; // küre yarıçapı

    [HideInInspector] public float gravity;
    [HideInInspector] public float normalColliderHeight;
    [HideInInspector] public Vector3 normalCenter;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public Transform cameraTransform;
    [HideInInspector] public Vector3 playerVelocity;
    [HideInInspector] public Transform playerModel;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public float stepTimer;

    float lastGroundedTime;
    const float groundGracePeriod = 0.1f;

    public StateMachine movementSM;
    public StandingState standing;
    public JumpingState jumping;
    public LandingState landing;
    public CrouchingState crouching;
    public SprintingState sprinting;
    public SprintJumpingState sprintJumping;
    public AttackState attacking;

    void Start()
    {
        controller      = GetComponent<CharacterController>();
        animator        = GetComponentInChildren<Animator>();
        playerInput     = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;
        audioSource     = GetComponent<AudioSource>();

        normalColliderHeight = controller.height;
        normalCenter         = controller.center;
        gravity              = Physics.gravity.y * gravityMultiplier;
        playerVelocity       = Vector3.zero;
        playerModel          = animator.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        movementSM    = new StateMachine();
        standing      = new StandingState(this, movementSM);
        jumping       = new JumpingState(this, movementSM);
        landing       = new LandingState(this, movementSM);
        crouching     = new CrouchingState(this, movementSM);
        sprinting     = new SprintingState(this, movementSM);
        sprintJumping = new SprintJumpingState(this, movementSM);
        attacking     = new AttackState(this, movementSM);

        movementSM.Initialize(standing);
    }

    void Update()
    {
        if (IsGrounded()) lastGroundedTime = Time.time;

        movementSM.currentState.HandleInput();
        movementSM.currentState.LogicUpdate();
        movementSM.currentState.PhysicsUpdate();
    }

    public bool IsGrounded()
    {
        Vector3 spherePos = transform.position + Vector3.up * groundCheckOffset;
        return Physics.CheckSphere(spherePos, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    // Grace period: son grounded anından bu yana 0.1s'den azsa hâlâ grounded say
    public bool IsGroundedWithGrace() =>
        IsGrounded() || (Time.time - lastGroundedTime < groundGracePeriod);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckOffset, groundCheckRadius);
    }
}
