using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLiftingBody : ALiftingBody {

    [SerializeField]
    private Vector3 controlResponse;
	[SerializeField]
	private Vector3 controlSpeed; // How quickly controls take effect, in %/s. 0 means immediate controls.
								// recommended: 0 or > 4 for aerobatics, 0<x<2 for heavy aircraft, > 2 for lighter aircraft
	[SerializeField]
    private float bodyDragCoeff;
    [SerializeField]
    private float frontalArea;
    // Wing stuff
    [SerializeField]
    private float wingDragCoeff;
    [SerializeField]
    private float horizWingArea; // recommended: main wing area + 1/2 horiz stab area
    [SerializeField]
    private float vertWingArea; // recommended: side profile of aircraft / 2
    [SerializeField]
    private float horizAspectRatio; // recommended: main wing AR
    [SerializeField]
    private float vertAspectRatio; // recommended: vert stab AR
    [SerializeField]
    private float moment; // recommended: start with 0.5f, change until it feels right.
    [SerializeField]
    private Vector2 stability; // recommended: most civilian aircraft: (+, +); aerobatics, fighters: (0, +); relaxed-stability, forward-swept: (-, +)
    [SerializeField]
    private float stallSpeed; // Speed at which AoA = stall angle when flying straight @ 0m MSL (m/s)
    [SerializeField]
    private float cruiseSpeed; // Speed at which level aircraft also flies level @ 0m MSL. 0 means never; wings are symmetrical.(m/s)

    private Airfoil horizAirfoil, vertAirfoil; // approximation of all horizontal surfaces' airfoils.
    private float invCruiseSpeed;
	private Vector3 control; // The actual control being applied, after control speed

    // Use this for initialization
    override protected void Start()
    {
        base.Start();
        dragC = bodyDragCoeff + wingDragCoeff;
        //print(atm.Density(cruiseAlt, false));

        invCruiseSpeed = 1 / cruiseSpeed;

        //float horizAR = wingSpan * wingSpan / horizWingArea, vertAR = vertStabHeight * vertStabHeight / vertWingArea;

        float liftLevel, liftSlope, stallAngle;

        // Stats for horizontal AF
        // liftLevel
        if (cruiseSpeed != 0f)
            liftLevel = 2 * mass * -Physics.gravity.y / (1.0f * cruiseSpeed * cruiseSpeed * horizWingArea); // 2 * m * g / (p * V^2 * S) = Cl 
        else
            liftLevel = 0;
        print("liftLevel = " + liftLevel);
        // liftSlope
        liftSlope = Mathf.PI * Mathf.PI / 90 * (1 - Mathf.Exp(-horizAspectRatio * 0.5f));
        print("liftSlope = " + liftSlope);
        // stallAngle
        stallAngle = (2 * mass * -Physics.gravity.y / (1.0f * stallSpeed * stallSpeed * horizWingArea) - liftLevel) * Mathf.PI / (2 * liftSlope); // (2 * m * g / (p * V^2 * S) - L) * pi / (2 * l) = s
        print("stallAngle = " + stallAngle);

        horizAirfoil = new Airfoil(liftLevel, liftSlope, stallAngle, -moment, stability.x, wingDragCoeff);

        // Stats for vertical AF
        liftSlope = Mathf.PI * Mathf.PI / 90 * (1 - Mathf.Exp(-vertAspectRatio));

        stallAngle = Mathf.PI / (2 * liftSlope);

        vertAirfoil = new Airfoil(0f, liftSlope, stallAngle, 0.0f, stability.y, wingDragCoeff);
    }

    void FixedUpdate()
    {
        //Vector3 gravity = transform.InverseTransformDirection(Physics.gravity);
        acceleration = transform.InverseTransformDirection(Physics.gravity);
        acceleration += Vector3.forward * thrust / mass;
        lift(); // does lift, drag, and rotational moment
        velocity += acceleration * Time.fixedDeltaTime;
        //angularVelocity += angularAcceleration * Time.fixedDeltaTime;
        transform.Translate(velocity * Time.fixedDeltaTime * iScale);
        Vector3 prevVel = transform.TransformDirection(velocity);
        transform.Rotate(angularVelocity * Time.fixedDeltaTime);
        velocity = transform.InverseTransformDirection(prevVel);
        //transform.Rotate(new Vector3(pitch * controlResponse.x * Mathf.Sqrt(ias), yaw * controlResponse.y * Mathf.Sqrt(ias), roll * controlResponse.z * Mathf.Sqrt(ias)) * Time.fixedDeltaTime);
    }

    // Update is called once per frame
    void Update()
	{
		
	}

	private Vector3 TransformR(Vector3 R) // transform lift and drag into the same basis as velocity
	{
		Vector3 row1 = new Vector3 (Mathf.Cos (sideslip), Mathf.Sin (sideslip) * Mathf.Sin (AoA), Mathf.Sin (sideslip) * Mathf.Cos (AoA));
		Vector3 row2 = new Vector3 (0f, Mathf.Cos (AoA), -Mathf.Sin (AoA));
		Vector3 row3 = new Vector3 (-Mathf.Sin (sideslip), Mathf.Cos (sideslip) * Mathf.Sin (AoA), Mathf.Cos (sideslip) * Mathf.Cos (AoA));
		return new Vector3 (Vector3.Dot (row1, R), Vector3.Dot (row2, R), Vector3.Dot (row3, R)); 
	}

    private void lift() // different from LiftingBody.lift, in that it is much simpler
    {
        float speedSqrYZ = velocity.y * velocity.y + velocity.z * velocity.z;
        float speedSqrXZ = velocity.x * velocity.x + velocity.z * velocity.z;
        float horizLiftPerCoeff = speedSqrYZ * horizWingArea * atm.Density(transform.position.y, true) * 0.5f / mass;
        float vertLiftPerCoeff = speedSqrXZ * vertWingArea * atm.Density(transform.position.y, true) * 0.5f / mass;
        float degAoA = AoA * Mathf.Rad2Deg;
        float degSideslip = sideslip * Mathf.Rad2Deg;
        float horizDrag = horizAirfoil.getDrag(degAoA) * horizWingArea;
        float vertDrag = vertAirfoil.getDrag(degSideslip) * vertWingArea;
        float totalDrag = indicatedVelocity.sqrMagnitude * atm.Density(transform.position.y, true) * 0.5f / mass * (horizDrag + vertDrag + bodyDragCoeff * frontalArea);
        Vector3 relativeAccel = new Vector3(-vertAirfoil.getLift(degSideslip) * vertLiftPerCoeff, horizAirfoil.getLift(degAoA) * horizLiftPerCoeff, -totalDrag);
        Vector3 fixedAcceleration = TransformR(relativeAccel);
        if ((velocity.x + acceleration.x * Time.fixedDeltaTime) * velocity.x < 0.0f)
        {
            fixedAcceleration.x = -velocity.x / Time.fixedDeltaTime;
        }
        if ((velocity.y + acceleration.y * Time.fixedDeltaTime) * velocity.y < 0.0f)
        {
            fixedAcceleration.y = -velocity.y / Time.fixedDeltaTime;
        }
        acceleration += fixedAcceleration;
        angularVelocity = new Vector3(horizAirfoil.getMoment(degAoA) * horizLiftPerCoeff, vertAirfoil.getMoment(degSideslip) * vertLiftPerCoeff, 0f);
        angularVelocity += new Vector3(-moment * Mathf.Cos(Vector3.Angle(Physics.gravity, transform.up) * Mathf.Deg2Rad) * horizLiftPerCoeff, 0.0f, 0.0f);
        float controlCoeff = ias * invCruiseSpeed * Mathf.Max(0.0f, Mathf.Cos(AoA * 2)); // possible source of backwards flying bug: if ias is negative, controlCoeff is negative.

		if (controlSpeed.x > 0) {
			if (control.x < pitch) // figure out pitch control
				control.x += Mathf.Min (controlSpeed.x * Time.fixedDeltaTime, pitch - control.x);
			else if (control.x > pitch)
				control.x -= Mathf.Min (controlSpeed.x * Time.fixedDeltaTime, control.x - pitch);
		} else
			control.x = pitch;

		if (controlSpeed.y > 0) {
			if (control.y < yaw) // figure out yaw control
				control.y += Mathf.Min (controlSpeed.y * Time.fixedDeltaTime, yaw - control.y);
			else if (control.y > yaw)
				control.y -= Mathf.Min (controlSpeed.y * Time.fixedDeltaTime, control.y - yaw);
		} else
			control.y = yaw;

		if (controlSpeed.z > 0) {
			if (control.z < roll) // figure out roll control
				control.z += Mathf.Min (controlSpeed.z * Time.fixedDeltaTime, roll - control.z);
			else if (control.z > roll)
				control.z -= Mathf.Min (controlSpeed.z * Time.fixedDeltaTime, control.z - roll);
		} else
			control.z = roll;

		angularVelocity += Vector3.Scale(control, controlResponse) * Mathf.Sqrt(Mathf.Abs(controlCoeff)) * Mathf.Sign(controlCoeff); // If controlCoeff is negative, sqrt(controlCoeff) is NaN.
        // angularVelocity += new Vector3(pitch * controlResponse.x * Mathf.Sqrt(controlCoeff), yaw * controlResponse.y * Mathf.Sqrt(controlCoeff), roll * controlResponse.z * Mathf.Sqrt(controlCoeff));
    }

    private void drag() // depricated
    {
        float degAoA = AoA * Mathf.Rad2Deg;
        float degSideslip = sideslip * Mathf.Rad2Deg;
        float horizDrag = horizAirfoil.getDrag(degAoA) * horizWingArea;
        float vertDrag = vertAirfoil.getDrag(degSideslip) * vertWingArea;
        float totalDrag = indicatedVelocity.sqrMagnitude * atm.Density(transform.position.y, true) * 0.5f / mass * (horizDrag + vertDrag + bodyDragCoeff * frontalArea);
        acceleration += TransformR(Vector3.back * totalDrag);
    }
}
