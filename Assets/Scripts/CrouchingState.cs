using UnityEngine;

public class CrouchingState : State
{
    private bool belowCeiling;
    private Vector2 inputVector;
    private float speedSmoothVel;

    public CrouchingState(Character character, StateMachine stateMachine)
        : base(character, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        belowCeiling = false;
        inputVector  = Vector2.zero;

        animator.SetTrigger("crouch");

        character.controller.height = character.crouchColliderHeight;
        character.controller.center = new Vector3(
            character.normalCenter.x,
            character.crouchColliderHeight / 2f,
            character.normalCenter.z);
    }

    public override void HandleInput()
    {
        inputVector = moveAction.ReadValue<Vector2>();

        if (crouchAction.triggered && !belowCeiling)
            stateMachine.ChangeState(character.standing);
    }

    public override void LogicUpdate()
    {
        float target  = inputVector.sqrMagnitude > 0.01f ? 1f : 0f;
        float current = animator.GetFloat("speed");
        animator.SetFloat("speed",
            Mathf.SmoothDamp(current, target, ref speedSmoothVel, character.speedDampTime));
    }

    public override void PhysicsUpdate()
    {
        belowCeiling = CheckCeiling();

        Vector3 moveDir = GetCameraMoveDirection(inputVector);
        character.controller.Move(moveDir * character.crouchSpeed * Time.deltaTime);
        ApplyGravity();
        RotateTowardMovement(moveDir);
    }

    public override void Exit()
    {
        character.controller.height = character.normalColliderHeight;
        character.controller.center = character.normalCenter;

        // Çömelirken hareket ediyorduysa hızı koru
        character.playerVelocity   = character.controller.velocity;
        character.playerVelocity.y = 0f;

        animator.SetTrigger("move");
    }

    private bool CheckCeiling()
    {
        Vector3 origin   = character.transform.position + Vector3.up * character.crouchColliderHeight;
        float   distance = character.normalColliderHeight - character.crouchColliderHeight + 0.05f;
        return Physics.Raycast(origin, Vector3.up, distance);
    }
}
