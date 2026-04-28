using UnityEngine;

public class AttackState : State
{
    private float timePassed;
    private float clipLength;
    private float clipSpeed;
    private bool  attack;

    private const int CombatLayerIndex = 1;

    public AttackState(Character character, StateMachine stateMachine)
        : base(character, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        attack    = false;
        timePassed = 0f;
        character.animator.applyRootMotion = true;
        character.animator.SetFloat("speed", 0f);
    }

    public override void HandleInput()
    {
        if (attackAction.triggered)
            attack = true;
    }

    public override void LogicUpdate()
    {
        timePassed += Time.deltaTime;

        var clipInfo = character.animator.GetCurrentAnimatorClipInfo(CombatLayerIndex);
        if (clipInfo.Length > 0)
        {
            clipLength = clipInfo[0].clip.length;
            clipSpeed  = character.animator.GetCurrentAnimatorStateInfo(CombatLayerIndex).speed;
        }

        if (clipSpeed <= 0f) return;

        if (timePassed >= clipLength / clipSpeed)
        {
            if (attack)
            {
                attack     = false;
                timePassed = 0f;
                character.animator.SetTrigger("attack");
                stateMachine.ChangeState(character.attacking);
            }
            else
            {
                character.animator.SetTrigger("move");
                stateMachine.ChangeState(character.standing);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        character.animator.applyRootMotion = false;
    }
}
