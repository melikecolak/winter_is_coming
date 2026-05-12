using UnityEngine;
using UnityEngine.InputSystem;

public class State
{
    protected Character character;
    protected StateMachine stateMachine;
    protected Animator animator => character.animator;

    protected InputAction moveAction;
    protected InputAction jumpAction;
    protected InputAction crouchAction;
    protected InputAction sprintAction;
    protected InputAction attackAction;

    public State(Character character, StateMachine stateMachine)
    {
        this.character    = character;
        this.stateMachine = stateMachine;

        var map   = character.playerInput.actions.FindActionMap("Player");
        moveAction   = map.FindAction("Move");
        jumpAction   = map.FindAction("Jump");
        crouchAction = map.FindAction("Crouch");
        sprintAction = map.FindAction("Sprint");
        attackAction = map.FindAction("Attack");
    }

    public virtual void Enter()         { Debug.Log("[State] " + GetType().Name); }
    public virtual void Exit()          { }
    public virtual void HandleInput()   { }
    public virtual void LogicUpdate()   { }
    public virtual void PhysicsUpdate() { }

    protected void ApplyGravity()
    {
        if (character.controller.isGrounded && character.playerVelocity.y < 0f)
            character.playerVelocity.y = -2f;

        character.playerVelocity.y += character.gravity * Time.deltaTime;
        character.controller.Move(Vector3.up * character.playerVelocity.y * Time.deltaTime);
    }

    protected void RotateTowardMovement(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.01f) return;
        Quaternion target = Quaternion.LookRotation(moveDir);
        character.playerModel.rotation = Quaternion.Slerp(
            character.playerModel.rotation, target,
            character.rotationSpeed * Time.deltaTime);
    }

    protected Vector3 GetCameraMoveDirection(Vector2 input)
    {
        Vector3 forward = character.cameraTransform.forward;
        Vector3 right   = character.cameraTransform.right;
        forward.y = 0f; forward.Normalize();
        right.y   = 0f; right.Normalize();
        return (forward * input.y + right * input.x).normalized;
    }

    protected void TickFootstep(float interval, Vector3 moveDirection)
    {
        character.stepTimer -= Time.deltaTime;
        if (character.stepTimer > 0f) return;

        if (moveDirection.sqrMagnitude < 0.01f)
        {
            character.stepTimer = interval;
            return;
        }
        if (character.footstepClip == null || character.audioSource == null)
        {
            character.stepTimer = interval;
            return;
        }

        Vector3 origin = character.transform.position + Vector3.up * 0.1f;
        if (!Physics.Raycast(origin, Vector3.down, 0.4f, character.groundLayer))
        {
            character.stepTimer = interval;
            return;
        }

        character.audioSource.PlayOneShot(character.footstepClip);
        character.stepTimer = interval;
    }
}
