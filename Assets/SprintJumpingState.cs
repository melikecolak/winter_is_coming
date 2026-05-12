using UnityEngine;

public class SprintJumpingState : State
{
    private const float JumpDuration = 1f;
    private float timer;

    public SprintJumpingState(Character character, StateMachine stateMachine)
        : base(character, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        timer = JumpDuration;
        animator.SetTrigger("sprintJump");
        animator.applyRootMotion = true;
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

    public override void Exit()
    {
        animator.applyRootMotion = false;
    }
}
