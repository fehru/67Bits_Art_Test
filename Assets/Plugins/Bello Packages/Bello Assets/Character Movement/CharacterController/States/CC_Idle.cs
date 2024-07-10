using UnityEngine;

public class CC_Idle : State<CC_Character>
{
    public CC_Idle() { }
    public CC_Idle(int targetAnimationLayer) { this.targetAnimationLayer = targetAnimationLayer; }

    private int targetAnimationLayer;

    public override void EnterState(CC_Character cc_Character)
    {
        cc_Character.currentState = CC_States.Idle;
        cc_Character.animator.CrossFadeInFixedTime(Default_Animations_Hashes.Idle, 0.25f, targetAnimationLayer);
    }
    public override bool ExitState(CC_Character cc_Character)
    {
        return true;
    }

    public override bool CheckStateSwitch(CC_Character cc_Character)
    {
        if (cc_Character.joystickAxis.magnitude > 0)
        {
            cc_Character.SwichState(new CC_Walk(false, targetAnimationLayer));
            return true;
        }
        return false;
    }

    public override void UpdateState(CC_Character cc_Character)
    {
        CheckStateSwitch(cc_Character);
    }
}
