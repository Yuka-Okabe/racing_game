﻿using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(BezierCurve))]
public class HandlesTest : Editor
{
    bool rotationMode = false;

    void OnSceneGUI()
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.E)
                    rotationMode = true;
                if (e.keyCode == KeyCode.W)
                    rotationMode = false;
                break;
        }
        BezierCurve curve = (BezierCurve)target;


        Vector3 pos = Handles.PositionHandle(curve.b.position, curve.b.rotation);
        curve.a.transform.LookAt(pos);

        curve.b.position = pos;


        pos = Handles.PositionHandle(curve.c.position, curve.c.rotation);
        curve.d.transform.LookAt(2 * curve.d.transform.position - pos);
        curve.c.position = pos;  




        curve.a.gameObject.hideFlags = HideFlags.None;

        if(rotationMode) 
        {
            curve.a.rotation = Handles.RotationHandle(curve.a.rotation, curve.a.position);
            curve.d.rotation = Handles.RotationHandle(curve.d.rotation, curve.d.position);
        }
        else
        {
            curve.a.position = Handles.PositionHandle(curve.a.position, curve.a.rotation);
            curve.d.position = Handles.PositionHandle(curve.d.position, curve.d.rotation);
        }


        
        Handles.DrawLine(curve.a.position, curve.b.position);
        Handles.DrawLine(curve.d.position, curve.c.position);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Add curve"))
            AddCurve((BezierCurve)target);

    }

    [MenuItem("Track Editor/Add segment #C")]
    static void AddCurveFromMenu()
    {
        Debug.Log("Selected Transform is on " + Selection.activeTransform.gameObject.name + ".");
        BezierCurve previous = Selection.activeTransform.GetComponent<BezierCurve>();
        if (previous != null)
        {
            Selection.activeTransform = AddCurve(previous).transform;
        }
        else
        {
            Debug.LogError("Trying to add a new segment from an non-track object");
        }
    }


    static BezierCurve AddCurve(BezierCurve previous)
    {
        BezierCurve newCurve = new GameObject().AddComponent<BezierCurve>();
        newCurve.Start();
        newCurve.a.position = previous.d.position;
        newCurve.a.rotation = previous.d.rotation;

        newCurve.d.position = previous.d.position + previous.d.forward * 10.0f;
        //newCurve.a.rotation = previous.d.rotation;


        previous.nextCurve = newCurve;

        return newCurve;
    }


    Tool LastTool = Tool.None;

    void OnEnable()
    {
        LastTool = Tools.current;
        Tools.current = Tool.None;
    }

    void OnDisable()
    {
        Tools.current = LastTool;
    }
 
}
