﻿using UnityEngine;
using System.Collections;

public class WheelController : MonoBehaviour { 
    public Rigidbody rigidbody;
    public float mass = 30.0f;
    public float radius = 0.5f;
    public float tractionCoeff = 8.0f;
    public float maxTractionAmt = 100.0f;
    public float sideTraction;
    public float angularVelocity;

    public float AngularVelocity
    {
        get { return angularVelocity * 0.017453292519968f; }
        set { angularVelocity = value; }
    }

    public float rpm
    {
        get { return angularVelocity; }
    }

    public float driveTorque;
    public float brakeTorque;

    public float tractionTorque;
    public float totalTorque;
    public float linearVel;
    public float slipRatio;

    public Vector3 localVel;

    public float slipAngle;

    public AnimationCurve frictionCurve;
    public AnimationCurve sideCurve;

    public GameObject wheelGeometry;
    public float tractionForce;
    public float maxSideForce = 500.0f;

    public float distanceToCM;
    public LayerMask raycastIgnore;
    public float steeringAngle;
    public Vector3 fwd;

    float prevSteringAngle;
	// Update is called once per frame
	void Update () 
    {
        //wheelGeometry.transform.localRotation *= Quaternion.Euler(0.0f, steeringAngle - prevSteringAngle, 0.0f);
        wheelGeometry.transform.Rotate(rigidbody.transform.up, steeringAngle - prevSteringAngle, Space.World);

        wheelGeometry.transform.Rotate(Vector3.right, angularVelocity * Time.deltaTime, Space.Self);
        wheelGeometry.transform.localPosition = transform.localPosition;
        prevSteringAngle = steeringAngle;
	}
    void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = (Vector3.zero);
    }
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -rigidbody.transform.up, radius, raycastIgnore))
        {
            SimulateTraction();
        }
    }

    public Vector3 prevPos;
    public float w;

    public float fwdForce;
    public float sideForce;

    void SimulateTraction()
    {
        fwd = rigidbody.transform.forward;
        //fwd = Quaternion.Euler(0, steeringAngle * 2.0f, 0) * fwd;
        Vector3 right = rigidbody.transform.right;
        //right = Quaternion.Euler(0,  steeringAngle * 2.0f, 0) * right;
        //right = -Vector3.Cross(fwd, transform.up).normalized;

        // WHAAAAAT
        distanceToCM = 1.5f;
        // TO-DO: Change this to wheel space
        //localVel = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
        //localVel = (transform.position - prevPos) / Time.fixedDeltaTime;
        //localVel = transform.InverseTransformDirection(localVel);
        localVel = transform.InverseTransformDirection(rigidbody.GetPointVelocity(transform.position));
       // Debug.DrawLine(transform.position, transform.position + rigidbody.GetPointVelocity(transform.position), Color.green);
        //totalTorque = driveTorque + tractionTorque + brakeTorque;

        float wheelInertia = mass * radius * radius * 0.5f;    // Mass is 70.0kg
        //totalTorque = (-1.0f * Mathf.Sign(driveTorque) * tractionTorque + driveTorque - brakeTorque);
        //Mathf.Clamp(brakeTorque, -driveTorque, driveTorque);
        if (angularVelocity == 0.0f)
            brakeTorque = 0.0f;
        brakeTorque = -1.0f * Mathf.Sign(angularVelocity) * brakeTorque * angularVelocity *0.01f;
       // totalTorque = driveTorque - brakeTorque;
        totalTorque = driveTorque  + brakeTorque;

        float wheelAngularAccel = (totalTorque) / wheelInertia;

        // If the wheel is driven by the engine
        if (totalTorque != 0.0f)
        {
            angularVelocity += wheelAngularAccel * Time.fixedDeltaTime;
            linearVel = angularVelocity * 0.017453292519968f * radius;
        }
        // If the wheel is spinning free
        else
        {
            angularVelocity = (localVel.z) * (1.0f / 0.017453292519968f) * (1.0f / radius);
            linearVel = (localVel.z);
        }

        //linearVel = angularVelocity * 0.017453292519968f * radius;

        slipRatio = (linearVel - localVel.z) / Mathf.Abs(localVel.z);

        // If it's NaN, then the car and the wheel are stopped (0 / 0 division)
        if (float.IsNaN(slipRatio))
        {
            slipRatio = 0.0f;
        }
        // If it's infinity, then the wheel is spinning but the car is stopped (x / 0) division
        else if (float.IsInfinity(slipRatio))
        {
            slipRatio = 1.0f * Mathf.Sign(slipRatio);
        }
        





        w = Mathf.Lerp( rigidbody.angularVelocity.y * -(Mathf.Abs(distanceToCM)), w, Time.fixedDeltaTime * 100.081f);

        slipAngle = Mathf.Sign(steeringAngle) * Mathf.Atan((localVel.x - w / Mathf.Abs(localVel.z))) - steeringAngle * Mathf.Sign(localVel.z);//, slipAngle, Time.fixedDeltaTime * 10.5f);

        if (float.IsNaN(slipAngle))
        {
            slipAngle = 0.0f;
        }
        if (float.IsInfinity(slipAngle))
        {
            slipAngle = 1.0f * Mathf.Sign(slipAngle);
        }
        //transform.position += Vector3.right * linearVel * Time.fixedDeltaTime;


        //float tractionForce = slipRatio * tractionCoeff;
        tractionForce = frictionCurve.Evaluate(Mathf.Abs(slipRatio)) * tractionCoeff * Mathf.Sign(slipRatio);
        tractionForce = Mathf.Clamp(tractionForce, -maxTractionAmt, maxTractionAmt);
        tractionTorque = tractionForce / radius;

        //if(driveTorque != 0.0f)
        Vector3 tractionForceV = fwd * tractionForce;
        //Debug.DrawLine(transform.position, 0.01f * tractionForceV + transform.position, Color.red);
        //tractionForceV = transform.TransformDirection(tractionForceV);
        rigidbody.AddForceAtPosition(tractionForceV, transform.position);
        fwdForce = tractionForceV.magnitude;


        Vector3 sideForce = -right * sideCurve.Evaluate(Mathf.Abs(slipAngle)) * Mathf.Sign(slipAngle * 0.6f) * sideTraction;  //* Mathf.Clamp((localVel.magnitude / 1.0f), 0.0f, 1.0f);
        //if(steeringAngle != 0.0f)
        //{
        //    sideForce *= Mathf.Sign(steeringAngle);
        //}
        this.sideForce = sideForce.magnitude;
        //sideForce = sideForce.magnitude > maxSideForce ? sideForce.normalized * maxSideForce : sideForce;

        //sideForce = transform.TransformDirection(sideForce);
        rigidbody.AddForceAtPosition(sideForce, transform.position);

        Debug.DrawLine(transform.position, 0.01f * sideForce + transform.position, Color.yellow);

        prevPos = transform.position;

        //Debug.DrawLine(transform.position, transform.position + fwd * 5.0f, Color.blue);
        //Debug.DrawLine(transform.position, transform.position + right * 5.0f, Color.red);

    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(GetComponent<Rigidbody>().worldCenterOfMass, 0.1f);
    }
}
