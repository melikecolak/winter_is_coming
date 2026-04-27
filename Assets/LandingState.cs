using UnityEngine;

public class LandingState : State
{
    private const float LandDuration = 0.5f;
    private float timer;

    public LandingState(Character character, StateMachine stateMachine)
        : base(character, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        timer = LandDuration;
        character.playerVelocity.y = 0f;
        // "land" trigger'ı JumpingState.LogicUpdate'te zaten set edildi
    }

    public override void LogicUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            animator.SetTrigger("move");
            stateMachine.ChangeState(character.standing);
        }
    }

    public override void PhysicsUpdate()
    {
        // Zeminde kal
        character.playerVelocity.y = -2f;
        character.controller.Move(Vector3.up * character.playerVelocity.y * Time.deltaTime);
    }
}
