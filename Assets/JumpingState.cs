using UnityEngine;

public class JumpingState : State
{
    private Vector2 inputVector;
    private Vector3 moveDirection;
    private float maxJumpVelocity;
    private float velSmoothRef;

    public JumpingState(Character character, StateMachine stateMachine)
        : base(character, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        maxJumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(character.gravity) * character.jumpHeight);
        character.playerVelocity.y = maxJumpVelocity;
    }

    public override void HandleInput()
    {
        inputVector = moveAction.ReadValue<Vector2>();
    }

    public override void LogicUpdate()
    {
        if (character.IsGroundedWithGrace() && character.playerVelocity.y <= 0f)
        {
            animator.SetFloat("verticalVelocity", 0f);
            stateMachine.ChangeState(character.standing);
        }
    }

    public override void PhysicsUpdate()
    {
        // verticalVelocity parametresini normalize et (-1 düşüş, +1 zıplama)
        float normalized = Mathf.Clamp(character.playerVelocity.y / maxJumpVelocity, -1f, 1f);
        float current = animator.GetFloat("verticalVelocity");
        animator.SetFloat("verticalVelocity",
            Mathf.SmoothDamp(current, normalized, ref velSmoothRef, 0.08f));

        if (character.airControl > 0f)
        {
            moveDirection = GetCameraMoveDirection(inputVector);
            character.controller.Move(
                moveDirection * character.playerSpeed * character.airControl * Time.deltaTime);
            RotateTowardMovement(moveDirection);
        }

        character.playerVelocity.y += character.gravity * Time.deltaTime;
        character.controller.Move(Vector3.up * character.playerVelocity.y * Time.deltaTime);
    }
}
