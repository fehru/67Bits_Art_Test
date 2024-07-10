using UnityEngine;

public class CC_Walk : State<CC_Character>
{
    public CC_Walk() { }
    public CC_Walk(bool substate, int targetAnimationLayer)
    {
        this.substate = substate;
        this.targetAnimationLayer = targetAnimationLayer;
    }

    private bool substate;
    private int targetAnimationLayer;

    public override void EnterState(CC_Character cc_Character)
    {
        cc_Character.currentState = CC_States.Walking;
        cc_Character.animator.CrossFadeInFixedTime(Default_Animations_Hashes.Walk, 0.25f, targetAnimationLayer);
    }
    public override bool ExitState(CC_Character cc_Character)
    {
        if (substate) return false;
        return true;
    }
    public override bool CheckStateSwitch(CC_Character cc_Character)
    {
        if (cc_Character.MovementDirection.magnitude == 0)
        {
            if (substate)cc_Character.RemoveSubstate(this);
            else cc_Character.SwichState(new CC_Idle(targetAnimationLayer));
            return true;
        }
        return false;
    }
    public override void UpdateState(CC_Character cc_Character)
    {
        if (substate) return;

        cc_Character.SetMovementDirection(cc_Character.joystickAxis, cc_Character.speed);
        cc_Character.HandleRotation(cc_Character.MovementDirection, Vector3.up);
        CheckStateSwitch(cc_Character);
    }
}
