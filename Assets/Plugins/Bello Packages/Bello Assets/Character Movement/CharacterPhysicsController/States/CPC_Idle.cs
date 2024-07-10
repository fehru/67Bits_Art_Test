using UnityEngine;

public class CPC_Idle : State<CPC_Character>
{
    public CPC_Idle() { }
    public CPC_Idle(int targetAnimationLayer) { this.targetAnimationLayer = targetAnimationLayer; }

    private int targetAnimationLayer;

    public override void EnterState(CPC_Character cpc_Character)
    {
        cpc_Character.currentState = CPC_States.Idle;
        cpc_Character.SetMovementVelocity(Vector2.zero, 0);
        cpc_Character.animator.CrossFadeInFixedTime(Default_Animations_Hashes.Idle, 0.25f, targetAnimationLayer);
    }
    public override bool ExitState(CPC_Character cpc_Character)
    {
        return true;
    }

    public override bool CheckStateSwitch(CPC_Character cpc_Character)
    {
        if (cpc_Character.joystickAxis.magnitude > 0)
        {
            cpc_Character.SwichState(new CPC_Walk(false, targetAnimationLayer));
            return true;
        }
        return false;
    }

    public override void UpdateState(CPC_Character cpc_Character)
    {
        CheckStateSwitch(cpc_Character);
    }
}
