using UnityEngine;

public class SprintingState : State
{
    private bool sprint;
    private bool sprintJump;
    private Vector2 inputVector;
    private Vector3 moveDirection;
    private float speedSmoothVel;

    public SprintingState(Character character, StateMachine stateMachine)
        : base(character, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        sprint      = true;
        sprintJump  = false;
        inputVector = Vector2.zero;
    }

    public override void HandleInput()
    {
        inputVector = moveAction.ReadValue<Vector2>();

        sprint     = sprintAction.IsPressed() && inputVector.sqrMagnitude > 0.01f;
        sprintJump = jumpAction.triggered && sprint;
    }

    public override void LogicUpdate()
    {
        if (!sprint)
        {
            stateMachine.ChangeState(character.standing);
            return;
        }

        float current = animator.GetFloat("speed");
        animator.SetFloat("speed",
            Mathf.SmoothDamp(current, 1.5f, ref speedSmoothVel, character.speedDampTime));

        animator.SetFloat("verticalVelocity", 0f);

        if (sprintJump)
            stateMachine.ChangeState(character.sprintJumping);
    }

    public override void PhysicsUpdate()
    {
        moveDirection = GetCameraMoveDirection(inputVector);
        character.controller.Move(moveDirection * character.sprintSpeed * Time.deltaTime);
        ApplyGravity();
        RotateTowardMovement(moveDirection);
        TickFootstep(character.sprintStepInterval, moveDirection);
    }

    public override void Exit()
    {
        character.playerVelocity.y = 0f;
    }
}
