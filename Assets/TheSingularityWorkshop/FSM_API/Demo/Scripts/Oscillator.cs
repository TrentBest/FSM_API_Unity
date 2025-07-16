using System;

using TheSingularityWorkshop.FSM.API;

using UnityEngine;

public class Oscillator : MonoBehaviour, IStateContext
{

    public string Name { get; set; }
    public bool IsValid { get; set; } = true;

    public FSMHandle OscillatorFSM { get; private set; }

    [SerializeField]
    public FloatAccesorSetDelegate floatAccesorSetDelegate;
    [SerializeField]
    public FloatAccessorGetDelegate floatAccessorGetDelegate;


    [SerializeField] public bool IsDriven { get; set; } = false;
    public float dx { get; set; } = .01f;

    public float Amplitude => MaximumValue - MinimumValue;
    public float MinimumValue { get;  set; } = -1f;
    public float MaximumValue { get;  set; } = 1f;


    public void Awake()
    {

        if (!FSM_API.Exists("OscillatorFSM"))
        {
            FSM_API.CreateProcessingGroup("OscillatorPG");
            FSM_API.CreateFiniteStateMachine("OscillatorFSM", -1, "OscillatorPG")
                .State("Initializing", OnEnterInitializing)
                .State("Maximum", OnEnterMaximum, OnUpdateMaximum, OnExitMaximum)
                .State("Minimum", OnEnterMinimum, OnUpdateMinimum, OnExitMinimum)

                .Transition("Initializing", "Maximum", IsCloserToMax)
                .Transition("Initializing", "Minimum", IsCloserToMin)
                 .Transition("Maximum", "Minimum", IsMin)
                 .Transition("Minimum", "Maximum", IsMax)
                .BuildDefinition();
        }
        OscillatorFSM = FSM_API.CreateInstance("OscillatorFSM", this, "OscillatorPG");
    }

    private bool IsCloserToMin(IStateContext context)
    {
        if(context is Oscillator osc)
        {
            var diff1 = osc.MaximumValue - osc.floatAccessorGetDelegate(osc.gameObject);
            var diff2 = osc.floatAccessorGetDelegate(osc.gameObject) - osc.MinimumValue;
            return diff2 < diff1;
        }
        return false;
    }

    private bool IsCloserToMax(IStateContext context)
    {
        if(context is Oscillator osc)
        {
            var diff1 = osc.MaximumValue - osc.floatAccessorGetDelegate(osc.gameObject);
            var diff2 = osc.floatAccessorGetDelegate(osc.gameObject) - osc.MinimumValue;
            return diff1 <= diff2;
        }
        return false;
    }

    private void OnEnterInitializing(IStateContext context)
    {
        if (context is Oscillator osc)
        {
            Debug.Log($"{osc.Name} Initializing Oscillator");
            if (osc.floatAccessorGetDelegate == null || osc.floatAccesorSetDelegate == null)
            {
                osc.floatAccessorGetDelegate = ScaleGet(osc.gameObject);
                osc.floatAccesorSetDelegate = ScaleSet(osc.gameObject);
            }
            var current = osc.floatAccessorGetDelegate(osc.gameObject);
            if(current < osc.MinimumValue)
            {
                current = osc.MinimumValue;
                osc.dx = -osc.dx;
                Debug.Log($"{osc.Name} Setting Initial Value to Minimum: {current}");
            }
            else if(current > osc.MaximumValue)
            {
                current = osc.MaximumValue;
                osc.dx = -osc.dx; 
                Debug.Log($"{osc.Name} Setting Initial Value to Maximum: {current}");
            }
            osc.floatAccesorSetDelegate(osc.gameObject, current);
        }
    }

    private FloatAccessorGetDelegate ScaleGet(GameObject gameObject)
    {
        return (go) => go.transform.localScale.x;
    }

    private FloatAccesorSetDelegate ScaleSet(GameObject gameObject)
    {
        return (go, value) =>
        {
            go.transform.localScale = new Vector3(value, go.transform.localScale.y, go.transform.localScale.z);
        };
    }

    private void OnEnterMaximum(IStateContext context)
    {
        if (context is Oscillator osc)
        {
            Debug.Log($"{osc.Name} Entering Maximum State");
            
            osc.dx = -osc.dx; // Reverse direction
        }
    }

    private void OnUpdateMaximum(IStateContext context)
    {
        if (context is Oscillator osc)
        {
            //Debug.Log($"{osc.Name} Updating Maximum State");
            osc.floatAccesorSetDelegate(osc.gameObject, osc.floatAccessorGetDelegate(osc.gameObject) + osc.dx);
        }
    }

    private void OnExitMaximum(IStateContext context)
    {

    }

    private void OnEnterMinimum(IStateContext context)
    {
        if (context is Oscillator osc)
        {
            Debug.Log($"{osc.Name} Entering Minimum State");
            osc.dx = -osc.dx; // Reverse direction
        }
    }

    private void OnUpdateMinimum(IStateContext context)
    {
        if (context is Oscillator osc)
        {
            //Debug.Log($"{osc.Name} Updating Minimum State");
            osc.floatAccesorSetDelegate(osc.gameObject, osc.floatAccessorGetDelegate(osc.gameObject) + osc.dx);
        }
    }

    private void OnExitMinimum(IStateContext context)
    {

    }

    private bool IsMax(IStateContext context)
    {
        if (context is Oscillator osc)
        {
            return osc.floatAccessorGetDelegate(osc.gameObject) >= osc.MaximumValue;
        }
        return false;
    }




    private bool IsMin(IStateContext context)
    {
        if (context is Oscillator osc)
        {
            return osc.floatAccessorGetDelegate(osc.gameObject) <= osc.MinimumValue;
        }
        return false;
    }

 
}

