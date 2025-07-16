using System;
using System.Collections.Generic;
using System.Linq;

using TheSingularityWorkshop.FSM.API;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Animations;

public class SimpleSplashDemo : MonoBehaviour, IStateContext
{
    public List<GameObject> moniker;
    public Dictionary<string, Mesh> letterMeshes;
    private FSMHandle demoFSMHandle;

    public bool IsInitialized { get; private set; }
    public float StartTime { get; private set; }
    public float runTime = 5f;

    private void Awake()
    {
        Name = name;
        if (moniker == null)
        {
            Debug.Log("Creating moniker for SimpleSplashDemo");
            moniker = new List<GameObject>();
            moniker.Add(CreateSplashLetter("T"));
            moniker.Add(CreateSplashLetter("H"));
            moniker.Add(CreateSplashLetter("E"));
            moniker.Add(CreateSplashLetter("S"));
            moniker.Add(CreateSplashLetter("I"));
            moniker.Add(CreateSplashLetter("N"));
            moniker.Add(CreateSplashLetter("G"));
            moniker.Add(CreateSplashLetter("U"));
            moniker.Add(CreateSplashLetter("L"));
            moniker.Add(CreateSplashLetter("A"));
            moniker.Add(CreateSplashLetter("R"));
            moniker.Add(CreateSplashLetter("I"));
            moniker.Add(CreateSplashLetter("T"));
            moniker.Add(CreateSplashLetter("Y"));
            moniker.Add(CreateSplashLetter("W"));
            moniker.Add(CreateSplashLetter("O"));
            moniker.Add(CreateSplashLetter("R"));
            moniker.Add(CreateSplashLetter("K"));
            moniker.Add(CreateSplashLetter("S"));
            moniker.Add(CreateSplashLetter("H"));
            moniker.Add(CreateSplashLetter("O"));
            moniker.Add(CreateSplashLetter("P"));
        }

        if (!FSM_API.Exists("SimpleSplashDemo"))
        {
            //Create the FSM which will contro the demo.
            FSM_API.CreateProcessingGroup("SimpleSplashDemoPG");
            FSM_API.CreateFiniteStateMachine("SimpleSplashDemo", 9, "SimpleSplashDemoPG")//process these fsms every 10 frames
                .State("Initializing", OnEnterSimpleDemoInitializing)
                .State("Presenting", OnEnterSimpleDemoPresenting, OnUpdateSimpleDemoPresenting, OnExitSimpleDemoPresenting)
                .State("Exiting", OnEnterExiting)
                .Transition("Initializing", "Presenting", Initialized)
                .Transition("Presenting", "Exiting", Finished)
                .BuildDefinition();

        }
        demoFSMHandle = FSM_API.CreateInstance("SimpleSplashDemo", this, "SimpleSplashDemoPG");
        IsValid = true;
    }

    private void OnEnterSimpleDemoInitializing(IStateContext context)
    {
        if (context is SimpleSplashDemo demo)
        {
            Debug.Log("Entering SimpleSplashDemo Initializing State");
            var t = demo.moniker[0].GetComponent<Oscillator>();
            t.floatAccesorSetDelegate += RotatorSet(t.gameObject, Axis.Y);
            t.floatAccessorGetDelegate += RotatorGet(t.gameObject, Axis.Y);
            t.MaximumValue = 360f;
            t.MinimumValue = 0f;
            t.dx = 1 / 360f;
            var h = demo.moniker[1].GetComponent<Oscillator>();
            h.floatAccesorSetDelegate += RotatorSet(h.gameObject, Axis.X);
            h.floatAccessorGetDelegate += RotatorGet(h.gameObject, Axis.X);
            h.MaximumValue = 360f;
            h.MinimumValue = 0f;
            h.dx = 1 / 360f;
            var e = demo.moniker[2].GetComponent<Oscillator>();
            e.floatAccesorSetDelegate += RotatorSet(e.gameObject, Axis.Z);
            e.floatAccessorGetDelegate += RotatorGet(e.gameObject, Axis.Z);
            e.MaximumValue = 360f;
            e.MinimumValue = 0f;
            e.dx = 1 / 360f;

            var s = demo.moniker[3].GetComponent<Oscillator>();
            t.floatAccesorSetDelegate += TranslatorSet(t.gameObject, Axis.X);
            t.floatAccessorGetDelegate += TranslatorGet(t.gameObject, Axis.X);
            t.MinimumValue = t.transform.position.x - .5f;
            t.MaximumValue = t.transform.position.x + .5f;
            var i = demo.moniker[4].GetComponent<Oscillator>();
            i.floatAccesorSetDelegate += ScalarSet(i.gameObject, Axis.Y);
            i.floatAccessorGetDelegate += ScalarGet(i.gameObject, Axis.Y);
            i.MinimumValue = .75f;
            i.MaximumValue = 1.25f;
            var n = demo.moniker[5].GetComponent<Oscillator>();
            n.floatAccesorSetDelegate += RotatorSet(n.gameObject, Axis.Z);
            n.floatAccessorGetDelegate += RotatorGet(n.gameObject, Axis.Z);
            var g = demo.moniker[6].GetComponent<Oscillator>();
            g.floatAccesorSetDelegate += TranslatorSet(g.gameObject, Axis.Y);
            g.floatAccessorGetDelegate += TranslatorGet(g.gameObject, Axis.Y);
            var u = demo.moniker[7].GetComponent<Oscillator>();
            u.floatAccesorSetDelegate += TranslatorSet(u.gameObject, Axis.Z);
            u.floatAccessorGetDelegate += TranslatorGet(u.gameObject, Axis.Z);
            var l = demo.moniker[8].GetComponent<Oscillator>();
            l.floatAccesorSetDelegate += ScalarSet(l.gameObject, Axis.X);
            l.floatAccessorGetDelegate += ScalarGet(l.gameObject, Axis.X);
            var a = demo.moniker[9].GetComponent<Oscillator>();
            a.floatAccesorSetDelegate += RotatorSet(a.gameObject, Axis.Y);
            a.floatAccessorGetDelegate += RotatorGet(a.gameObject, Axis.Y);
            var r = demo.moniker[10].GetComponent<Oscillator>();
            r.floatAccesorSetDelegate += TranslatorSet(r.gameObject, Axis.X);
            r.floatAccessorGetDelegate += TranslatorGet(r.gameObject, Axis.X);
            var i2 = demo.moniker[11].GetComponent<Oscillator>();
            i2.floatAccesorSetDelegate += ScalarSet(i2.gameObject, Axis.Y);
            i2.floatAccessorGetDelegate += ScalarGet(i2.gameObject, Axis.Y);
            var t2 = demo.moniker[12].GetComponent<Oscillator>();
            t2.floatAccesorSetDelegate += RotatorSet(t2.gameObject, Axis.Z);
            t2.floatAccessorGetDelegate += RotatorGet(t2.gameObject, Axis.Z);
            var y = demo.moniker[13].GetComponent<Oscillator>();
            y.floatAccesorSetDelegate += TranslatorSet(y.gameObject, Axis.X);
            y.floatAccessorGetDelegate += TranslatorGet(y.gameObject, Axis.X);

            var w = demo.moniker[14].GetComponent<Oscillator>();
            w.floatAccesorSetDelegate += ScalarSet(w.gameObject, Axis.Y);
            w.floatAccessorGetDelegate += ScalarGet(w.gameObject, Axis.Y);
            var o = demo.moniker[15].GetComponent<Oscillator>();
            o.floatAccesorSetDelegate += RotatorSet(o.gameObject, Axis.Z);
            o.floatAccessorGetDelegate += RotatorGet(o.gameObject, Axis.Z);
            var r2 = demo.moniker[16].GetComponent<Oscillator>();
            r2.floatAccesorSetDelegate += TranslatorSet(r2.gameObject, Axis.X);
            r2.floatAccessorGetDelegate += TranslatorGet(r2.gameObject, Axis.X);
            var k = demo.moniker[17].GetComponent<Oscillator>();
            k.floatAccesorSetDelegate += ScalarSet(k.gameObject, Axis.Y);
            k.floatAccessorGetDelegate += ScalarGet(k.gameObject, Axis.Y);
            var s2 = demo.moniker[18].GetComponent<Oscillator>();
            s2.floatAccesorSetDelegate += RotatorSet(s2.gameObject, Axis.Z);
            s2.floatAccessorGetDelegate += RotatorGet(s2.gameObject, Axis.Z);
            var h2 = demo.moniker[19].GetComponent<Oscillator>();
            h2.floatAccesorSetDelegate += TranslatorSet(h2.gameObject, Axis.X);
            h2.floatAccessorGetDelegate += TranslatorGet(h2.gameObject, Axis.X);
            var o2 = demo.moniker[20].GetComponent<Oscillator>();
            o2.floatAccesorSetDelegate += ScalarSet(o2.gameObject, Axis.Y);
            o2.floatAccessorGetDelegate += ScalarGet(o2.gameObject, Axis.Y);
            var p = demo.moniker[21].GetComponent<Oscillator>();
            p.floatAccesorSetDelegate += RotatorSet(p.gameObject, Axis.Z);
            p.floatAccessorGetDelegate += RotatorGet(p.gameObject, Axis.Z);
            IsInitialized = true;
            Debug.Log("SimpleSplashDemo Initialized");
        }
    }

    private void Update()
    {
        //Step our FSMs.
        FSM_API.Update("SimpleSplashDemoPG");//This should run every 10 frames, otherwise fast return;
        FSM_API.Update("OscillatorPG");//This iterates over a small group of oscillators (22).
    }

    private FloatAccessorGetDelegate ScalarGet(GameObject gameObject, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return go => go.transform.localScale.x;
            case Axis.Y:
                return go => go.transform.localScale.y;
            case Axis.Z:
                return go => go.transform.localScale.z;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }

    private FloatAccessorGetDelegate EmmissionGet(GameObject gameObject)
    {
        var renderer = gameObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new InvalidOperationException("GameObject does not have a Renderer component.");
        }
        return go => renderer.material.GetFloat("_EmissionStrength");
    }

    private FloatAccesorSetDelegate EmmissionSet(GameObject gameObject)
    {
        var renderer = gameObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new InvalidOperationException("GameObject does not have a Renderer component.");
        }
        return (go, value) => renderer.material.SetFloat("_EmissionStrength", value);
    }

    private FloatAccesorSetDelegate ScalarSet(GameObject gameObject, Axis axis)
    {
        switch(axis)
        {
            case Axis.X:
                return (go, value) => go.transform.localScale = new Vector3(value, go.transform.localScale.y, go.transform.localScale.z);
            case Axis.Y:
                return (go, value) => go.transform.localScale = new Vector3(go.transform.localScale.x, value, go.transform.localScale.z);
            case Axis.Z:
                return (go, value) => go.transform.localScale = new Vector3(go.transform.localScale.x, go.transform.localScale.y, value);
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }

    private FloatAccessorGetDelegate TranslatorGet(GameObject gameObject, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return go => go.transform.localPosition.x;
            case Axis.Y:
                return go => go.transform.localPosition.y;
            case Axis.Z:
                return go => go.transform.localPosition.z;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }

    private FloatAccesorSetDelegate TranslatorSet(GameObject gameObject, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return (go, value) => go.transform.localPosition = new Vector3(value, go.transform.localPosition.y, go.transform.localPosition.z);
            case Axis.Y:
                return (go, value) => go.transform.localPosition = new Vector3(go.transform.localPosition.x, value, go.transform.localPosition.z);
            case Axis.Z:
                return (go, value) => go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, value);
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null); // Corrected nameof(y) to nameof(axis)
        }
    }

    private FloatAccessorGetDelegate RotatorGet(GameObject gameObject, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return go => go.transform.localRotation.eulerAngles.x;
            case Axis.Y:
                return go => go.transform.localRotation.eulerAngles.y;
            case Axis.Z:
                return go => go.transform.localRotation.eulerAngles.z;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }

    private FloatAccesorSetDelegate RotatorSet(GameObject gameObject, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return (go, value) => go.transform.localRotation = Quaternion.Euler(value, go.transform.localRotation.eulerAngles.y, go.transform.localRotation.eulerAngles.z);
            case Axis.Y:
                return (go, value) => go.transform.localRotation = Quaternion.Euler(go.transform.localRotation.eulerAngles.x, value, go.transform.localRotation.eulerAngles.z);
            case Axis.Z:
                return (go, value) => go.transform.localRotation = Quaternion.Euler(go.transform.localRotation.eulerAngles.x, go.transform.localRotation.eulerAngles.y, value);
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }



    //If I know that I have 10 different ways I can anitmate my letters, and I have twenty letters, that means we will
    //have 2 letters doing the same animation at the same time... 


    private void OnEnterSimpleDemoPresenting(IStateContext context)
    {
        if(context is SimpleSplashDemo demo)
        {
            demo.StartTime = Time.time;
            Debug.Log("Entering SimpleSplashDemo Presenting State");
        }
    }

    private void OnUpdateSimpleDemoPresenting(IStateContext context)
    {
        if(context is SimpleSplashDemo demo)
        {
            Debug.Log("SimpleSplashDemo Presenting State Update");
        }
    }

    private void OnExitSimpleDemoPresenting(IStateContext context)
    {
        if(context is SimpleSplashDemo demo)
        {
            Debug.Log("Exiting SimpleSplashDemo Presenting State");
        }
    }

    private void OnEnterExiting(IStateContext context)
    {
        if(context is SimpleSplashDemo demo)
        {
            Debug.Log("Exiting SimpleSplashDemo");
        }
    }

    private bool Initialized(IStateContext context)
    {
        if (context is SimpleSplashDemo demo)
        {
            return demo.IsInitialized;
        }
        return false;
    }

    private bool Finished(IStateContext context)
    {
        if (context is SimpleSplashDemo demo)
        {
            return demo.StartTime + demo.runTime < Time.time;
        }
        return false;
    }


    private GameObject CreateSplashLetter(string letter)
    {
        GameObject go = new GameObject(letter);
        go.transform.SetParent(transform);
        var renderFilter = go.AddComponent<MeshFilter>();
        renderFilter.mesh = letterMeshes[letter];
        var render = go.AddComponent<MeshRenderer>();
        Oscillator oscillator = go.AddComponent<Oscillator>();
        go.transform.localPosition = new Vector3(0, 0, 0);
        Debug.Log($"Created letter {letter} with mesh {renderFilter.mesh.name}");
        return go;
    }

    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
}
public enum  MotionAxis
{
    X,
    Y,
    Z,
    All
}

[Serializable]
public delegate float FloatAccessorGetDelegate(GameObject go);
[Serializable]
public delegate void FloatAccesorSetDelegate(GameObject go, float value);
