using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;

public class Pause
{
    private static float? InitialFixed;
    private static float? LastNonZeroValue;
    //private static InputSettings.UpdateMode InitialUpdateMode;

    public static void SetEnabled(bool value)
    {
        if (value)
        {
            SetGameTime(0);
        }
        else
        {
            SetGameTime((LastNonZeroValue) ?? 1);
        }
    }

    public static void SetGameTime(float value)
    {
        Time.timeScale = value;

        if (LastNonZeroValue != 0)
        {
            LastNonZeroValue = value;
        }

        // Fixed delta time at same framerate for any time scale.
        if (InitialFixed == null)
        {
            InitialFixed = Time.fixedDeltaTime;
        }

        //if (InitialUpdateMode == null)
        //{
        //    InitialUpdateMode = InputSystem.settings.updateMode;
        //}

        // Input system updating on Dynami
        if (value > 0)
        {
            Time.fixedDeltaTime = (float)InitialFixed / value;
            //InputSystem.settings.updateMode = InitialUpdateMode;
        }
        else
        {
            Time.fixedDeltaTime = (float)InitialFixed / value;
            //InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        }
    }
}
