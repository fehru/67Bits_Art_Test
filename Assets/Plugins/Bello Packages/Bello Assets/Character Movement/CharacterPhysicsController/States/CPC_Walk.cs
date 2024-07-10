using UnityEngine;

public class CPC_Walk : State<CPC_Character>
{
    public CPC_Walk() { }
    public CPC_Walk(bool substate, int targetAnimationLayer)
    {
        this.substate = substate;
        this.targetAnimationLayer = targetAnimationLayer;
    }

    private bool substate;
    private int targetAnimationLayer;

    public override void EnterState(CPC_Character cpc_Character)
    {
        cpc_Character.currentState = CPC_States.Walking;
        cpc_Character.animator.CrossFadeInFixedTime(Default_Animations_Hashes.Walk, 0.25f, targetAnimationLayer);
    }
    public override bool ExitState(CPC_Character cpc_Character)
    {
        if (substate) return false;
        return true;
    }
    public override bool CheckStateSwitch(CPC_Character cpc_Character)
    {
        if (cpc_Character.joystickAxis.magnitude == 0)
        {
            if (substate) cpc_Character.RemoveSubstate(this);
            else cpc_Character.SwichState(new CPC_Idle(targetAnimationLayer));
            return true;
        }
        return false;
    }
    public override void UpdateState(CPC_Character cpc_Character)
    {
        if (substate || CheckStateSwitch(cpc_Character)) return;

        cpc_Character.SetMovementVelocity(cpc_Character.joystickAxis, cpc_Character.speed);
        cpc_Character.HandleRotation(cpc_Character.MovementDirection, Vector3.up);
        CheckStateSwitch(cpc_Character);
    }
}
