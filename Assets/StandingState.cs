using UnityEngine;

public class StandingState : State
{
    private bool jump;
    private bool crouch;
    private bool sprint;
    private bool attack;
    private Vector2 inputVector;
    private Vector3 moveDirection;
    private float speedSmoothVel;
    private float vertVelSmoothRef;

    public StandingState(Character character, StateMachine stateMachine)
        : base(character, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        jump = crouch = sprint = false;
        inputVector   = Vector2.zero;
        moveDirection = Vector3.zero;
    }

    public override void HandleInput()
    {
        inputVector = moveAction.ReadValue<Vector2>();
        jump   = jumpAction.triggered;
        crouch = crouchAction.triggered;
        sprint = sprintAction.IsPressed() && inputVector.sqrMagnitude > 0.01f;
        attack = attackAction.triggered;
    }

    public override void LogicUpdate()
    {
        float target  = inputVector.sqrMagnitude > 0.01f ? 1f : 0f;
        float current = animator.GetFloat("speed");
        animator.SetFloat("speed",
            Mathf.SmoothDamp(current, target, ref speedSmoothVel, character.speedDampTime));

        float curVert = animator.GetFloat("verticalVelocity");
        animator.SetFloat("verticalVelocity",
            Mathf.SmoothDamp(curVert, 0f, ref vertVelSmoothRef, 0.1f));

        if (jump)   { stateMachine.ChangeState(character.jumping);   return; }
        if (crouch) { stateMachine.ChangeState(character.crouching); return; }
        if (sprint) { stateMachine.ChangeState(character.sprinting); return; }
        if (attack) { animator.SetTrigger("attack"); stateMachine.ChangeState(character.attacking); return; }
    }

    public override void PhysicsUpdate()
    {
        moveDirection = GetCameraMoveDirection(inputVector);
        character.controller.Move(moveDirection * character.playerSpeed * Time.deltaTime);
        ApplyGravity();
        RotateTowardMovement(moveDirection);
        TickFootstep(character.walkStepInterval, moveDirection);
    }

    public override void Exit()
    {
        character.playerVelocity.y = 0f;
    }
}
