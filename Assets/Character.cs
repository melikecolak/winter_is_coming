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

    [HideInInspector] public float gravity;
    [HideInInspector] public float normalColliderHeight;
    [HideInInspector] public Vector3 normalCenter;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public Transform cameraTransform;
    [HideInInspector] public Vector3 playerVelocity;
    [HideInInspector] public Transform playerModel;

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
        controller     = GetComponent<CharacterController>();
        animator       = GetComponentInChildren<Animator>();
        playerInput    = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;

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
        movementSM.currentState.HandleInput();
        movementSM.currentState.LogicUpdate();
        movementSM.currentState.PhysicsUpdate();
        DiagnosticLog();
    }

    // GECICI DIAGNOSTIC — CollisionDiagnostic.cs ile birlikte sil.
    GameObject  _diagNPC;
    int         _diagFrame;
    void DiagnosticLog()
    {
        if (_diagNPC == null)
            _diagNPC = GameObject.FindGameObjectWithTag("Enemy");
        if (_diagNPC == null) return;

        float dist = Vector3.Distance(transform.position, _diagNPC.transform.position);
        if (dist > 2.5f) return;

        _diagFrame++;
        var flags = controller.collisionFlags;
        Debug.Log(
            $"[PLR #{_diagFrame:D4}] " +
            $"pos={V(transform.position)} | " +
            $"cc.vel={V(controller.velocity)} | " +
            $"cc.collFlags={flags} | " +
            $"cc.grounded={controller.isGrounded} | " +
            $"state={movementSM.currentState.GetType().Name} | " +
            $"dist={dist:F3}"
        );
    }

    static string V(Vector3 v) => $"({v.x:F3},{v.y:F3},{v.z:F3})";
}
