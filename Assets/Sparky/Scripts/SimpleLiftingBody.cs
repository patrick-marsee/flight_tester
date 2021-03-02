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
    private float wingSweep; // recommended: the sweep of the wing, 1/4 of the way back from the leading edge of the wings.
    [SerializeField]
    private float moment; // recommended: start with 0.5f, change until it feels right.
    [SerializeField]
    private Vector2 stability; // recommended: most civilian aircraft: (+, +); aerobatics, fighters: (0, +); relaxed-stability, forward-swept: (-, +)
    [SerializeField]
    private float stallSpeed; // Speed at which AoA = stall angle when flying straight @ 0m MSL (m/s)
    [SerializeField]
    private float cruiseSpeed; // Speed at which level aircraft also flies level @ 0m MSL. 0 means never; wings are symmetrical.(m/s)
    [SerializeField]
    private float rideHeight;

    private Airfoil horizAirfoil, vertAirfoil; // approximation of all horizontal surfaces' airfoils.
    private float invCruiseSpeed;
	private Vector3 mControl; // The actual control being applied, after control speed

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
        FinalizeControls();
        if (!isLanded)
        {
            fly();
        }
        else
        {
            taxi();
        }
        ResetControls();
    }
    
    private void fly()
    {
        mAcceleration = transform.InverseTransformDirection(Physics.gravity);
        if (braking)
        {
            mThrust -= mBrakingPower * (ias / cruiseSpeed);
        }
        mAcceleration += Vector3.forward * mThrust / mass;
        lift();
        control();
        mVelocity += mAcceleration * Time.fixedDeltaTime;
        transform.Translate(mVelocity * Time.fixedDeltaTime * iScale);
        Vector3 prevVel = transform.TransformDirection(mVelocity);
        transform.Rotate(mAngularVelocity * Time.fixedDeltaTime);
        mVelocity = transform.InverseTransformDirection(prevVel);
        // Check to see if we've landed
        if (isLandingGearDeployed)
        {
            RaycastHit ground;
            Physics.Raycast(transform.position, -transform.up, out ground, rideHeight, 0xFF, QueryTriggerInteraction.Ignore);
            if (ground.collider != null && ground.distance < rideHeight)
            {
                AttemptLanding(ref ground);
            }
        }
    }
    
    private void taxi()
    {
        // We need the ground's normal.
        RaycastHit ground;
        Physics.Raycast(transform.position, -transform.up, out ground, rideHeight + 1.0f, 0xFF, QueryTriggerInteraction.Ignore);
        Vector3 gravity = transform.InverseTransformDirection(Physics.gravity);
        Vector3 groundNormal = transform.InverseTransformDirection(ground.normal);
        if (braking)
        {
            mThrust -= 50000.0f;
        }
        acceleration = Vector3.forward * mThrust / mass;
        mVelocity -= Vector3.Project(mVelocity, gravity);
        lift();
        taxiControl();
        mVelocity += acceleration * Time.fixedDeltaTime;
        mVelocity.x = 0.0f;
        mVelocity.z = Mathf.Max(0.0f, mVelocity.z);
        Vector3 upwardsAccel = Vector3.Project(mAcceleration, groundNormal);
        Vector3 gravityEOR = Vector3.Project(gravity, groundNormal);
        if (upwardsAccel.sqrMagnitude > gravityEOR.sqrMagnitude
            && Mathf.Sign(upwardsAccel.y) == Mathf.Sign(groundNormal.y) // Make sure we're not somehow taking off into the ground.
            || ground.collider == null)
        {
            isLanded = false;
            if (mTakeoffCallbacks != null)
            {
                mTakeoffCallbacks();
            }
        }
        transform.Rotate(mAngularVelocity * Time.fixedDeltaTime);
        Vector3 axis = Vector3.Cross(Vector3.up, groundNormal);
        float angle = controlResponse.x * 0.5f * (1.0f - (upwardsAccel.magnitude / gravityEOR.magnitude)); // for now
        float flatAngle = Vector3.Angle(Vector3.up, groundNormal);
        if (flatAngle > 30.0f && mCrashColliderCallbacks != null)
        {
            mCrashColliderCallbacks(ground.collider);
        }
        transform.Rotate(axis, Mathf.Min(flatAngle, angle * Time.fixedDeltaTime));
        transform.Translate(mVelocity * Time.fixedDeltaTime * iScale + Vector3.up * (rideHeight - ground.distance));
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
        float speedSqrYZ = mVelocity.y * mVelocity.y + mVelocity.z * mVelocity.z;
        float speedSqrXZ = mVelocity.x * mVelocity.x + mVelocity.z * mVelocity.z;
        float altitude = atm.Altitude(transform.position.y);
        float horizLiftPerCoeff = speedSqrYZ * horizWingArea * atm.Density(altitude, true) * 0.5f / mass;
        float vertLiftPerCoeff = speedSqrXZ * vertWingArea * atm.Density(altitude, true) * 0.5f / mass;
        float degAoA = AoA * Mathf.Rad2Deg;
        float degSideslip = sideslip * Mathf.Rad2Deg;
        float wingDrag = horizAirfoil.getDrag(degAoA) * horizWingArea + vertAirfoil.getDrag(degSideslip) * vertWingArea;
        float mach = atm.Mach(altitude, mVelocity.magnitude);
        if (mach > 0.7f) // No need to do expensive calculations unless we're going fast enough for them to matter.
        {
            wingDrag += 3 * wingDrag * Mathf.Min(Mathf.Exp(16 * (mach - 1.0f + Mathf.Log10(1 - wingSweep / 90.0f))), Mathf.Exp(-mach + 1.0f));
        }
        float totalDrag = indicatedVelocity.sqrMagnitude * atm.Density(altitude, true) * 0.5f * (wingDrag + bodyDragCoeff * frontalArea) / mass;
        Vector3 relativeAccel = new Vector3(-vertAirfoil.getLift(degSideslip) * vertLiftPerCoeff, horizAirfoil.getLift(degAoA) * horizLiftPerCoeff, -totalDrag);
        Vector3 fixedAcceleration = TransformR(relativeAccel);
        if ((mVelocity.x + mAcceleration.x * Time.fixedDeltaTime) * mVelocity.x < 0.0f)
        {
            fixedAcceleration.x = -mVelocity.x / Time.fixedDeltaTime;
        }
        if ((mVelocity.y + mAcceleration.y * Time.fixedDeltaTime) * mVelocity.y < 0.0f)
        {
            fixedAcceleration.y = -mVelocity.y / Time.fixedDeltaTime;
        }
        mAcceleration += fixedAcceleration;
        mAngularVelocity = new Vector3(horizAirfoil.getMoment(degAoA) * horizLiftPerCoeff, vertAirfoil.getMoment(degSideslip) * vertLiftPerCoeff, 0f);
        mAngularVelocity += new Vector3(-moment * Mathf.Cos(Vector3.Angle(Physics.gravity, transform.up) * Mathf.Deg2Rad) * horizLiftPerCoeff, 0.0f, 0.0f);
    }
    
    private void updateControls()
    {
        if (controlSpeed.x > 0) {
			if (mControl.x < mPitch) // figure out pitch control
				mControl.x += Mathf.Min (controlSpeed.x * Time.fixedDeltaTime, mPitch - mControl.x);
			else if (mControl.x > mPitch)
				mControl.x -= Mathf.Min (controlSpeed.x * Time.fixedDeltaTime, mControl.x - mPitch);
		} else
			mControl.x = mPitch;

		if (controlSpeed.y > 0) {
			if (mControl.y < mYaw) // figure out yaw control
				mControl.y += Mathf.Min (controlSpeed.y * Time.fixedDeltaTime, mYaw - mControl.y);
			else if (mControl.y > mYaw)
				mControl.y -= Mathf.Min (controlSpeed.y * Time.fixedDeltaTime, mControl.y - mYaw);
		} else
			mControl.y = mYaw;

		if (controlSpeed.z > 0) {
			if (mControl.z < mRoll) // figure out roll control
				mControl.z += Mathf.Min (controlSpeed.z * Time.fixedDeltaTime, mRoll - mControl.z);
			else if (mControl.z > mRoll)
				mControl.z -= Mathf.Min (controlSpeed.z * Time.fixedDeltaTime, mControl.z - mRoll);
		} else
			mControl.z = mRoll;
    }
    
    private void control() // How a plane controls in the air
    {
        float controlCoeff = ias * invCruiseSpeed * Mathf.Max(0.0f, Mathf.Cos(AoA * 2)); // possible source of backwards flying bug: if ias is negative, controlCoeff is negative.

		updateControls();

		mAngularVelocity += Vector3.Scale(mControl, controlResponse) * Mathf.Sqrt(Mathf.Abs(controlCoeff)) * Mathf.Sign(controlCoeff); // If controlCoeff is negative, sqrt(controlCoeff) is NaN.
    }
    
    private void taxiControl() // How a plane controls on the ground
    {
        float controlCoeff = ias * invCruiseSpeed * Mathf.Max(0.0f, Mathf.Cos(AoA * 2)); // possible source of backwards flying bug: if ias is negative, controlCoeff is negative.

		updateControls();
        
        mAngularVelocity += Vector3.Scale(mControl, new Vector3(controlResponse.x * Mathf.Sqrt(Mathf.Abs(controlCoeff)) * Mathf.Sign(controlCoeff),
                                                                tas * 0.1f * Mathf.Exp(tas * -0.1f) * 60.0f,
                                                                controlResponse.y * Mathf.Sqrt(Mathf.Abs(controlCoeff)) * Mathf.Sign(controlCoeff)));
    }
    
    // Detect if we've crashed or landed.
    private void OnTriggerEnter(Collider other)
    {
        RaycastHit ground;
        other.Raycast(new Ray(transform.position, -transform.up), out ground, rideHeight + 1.0f);
        // The "Ground" tag is so that you can't land on other planes and weirdness like that.
        if (ground.collider != other && mCrashColliderCallbacks != null)
        {
            mCrashColliderCallbacks(other);
        }
        else
        {
            AttemptLanding(ref ground);
        }
    }
    
    private void AttemptLanding(ref RaycastHit ground)
    {
        if (((ground.collider != null && !ground.collider.CompareTag("Ground")) || !isLandingGearDeployed) && mCrashColliderCallbacks != null)
        {
            mCrashColliderCallbacks(ground.collider);
        }
        Vector3 upwardsAccel = Vector3.Project(acceleration, ground.normal);
        Vector3 gravityEOR = Vector3.Project(Physics.gravity, ground.normal);
        if (upwardsAccel.sqrMagnitude < gravityEOR.sqrMagnitude)
        {
            isLanded = true;
            if (mLandCallbacks != null)
            {
                mLandCallbacks();
            }
        }
    }
}
