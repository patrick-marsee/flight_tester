using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ALiftingBody : MonoBehaviour, ILiftingBody {
    // This is really being used more like an interface. I'm using an abstract class instead beacause interfaces make property accessors a pain.

    [SerializeField]
    protected Vector3 mVelocity;
    [SerializeField]
    protected Vector3 mAngularVelocity;
    [SerializeField]
    private float startSpeed;
    [SerializeField]
    private float Mass;
    [SerializeField]
    protected float mBrakingPower;
    [SerializeField]
    private bool startLanded;

    protected Rigidbody rBody;
    protected Atmosphere atm;
    protected float iScale; // inverse scale (Note: only scale when the transform is being used!)
    protected Vector3 mAcceleration;
    protected Vector3 mAngularAcceleration;
    
    protected float mPitch;
    protected float mRoll;
    protected float mYaw;
    protected float mThrust;
    
    private ILiftingBody.ControlSet mPitchCallbacks;
    private ILiftingBody.ControlSet mYawCallbacks;
    private ILiftingBody.ControlSet mRollCallbacks;
    private ILiftingBody.ControlSet mThrustCallbacks;
    protected ILiftingBody.FlightEvent mLandCallbacks;
    protected ILiftingBody.FlightEvent mTakeoffCallbacks;
    protected ILiftingBody.FlightEvent mCrashCallbacks;
    
    private bool mControlLock;

    public Vector3 velocity {
        get { return mVelocity; }
        set { mVelocity = value; }
    } // meters per second
    
    public Vector3 angularVelocity {
        get { return mAngularVelocity; }
        set { mAngularVelocity = value; }
    } // degrees per second
    
    public Vector3 indicatedVelocity {
        get { return mVelocity * Mathf.Sqrt(atm.Density(transform.position.y, true)); }
    }
    
    public Vector3 acceleration {
        get { return mAcceleration; }
        set { mAcceleration = value; }
    }
    
    public Vector3 angularAcceleration {
        get { return mAngularAcceleration; }
        set { mAngularAcceleration = value; }
    }
    
    public Vector3 geeForce {
        get { return (acceleration - transform.InverseTransformDirection(Physics.gravity)) / Physics.gravity.magnitude; }
    }
    
    public float AoA
    { 
        get 
        { 
            return (velocity.z != 0.0f ? -Mathf.Atan(velocity.y / velocity.z) : 0.0f); 
        } 
    } // radians
    
    public float sideslip {
        get 
        { 
            return (velocity.z != 0.0f ? Mathf.Atan(velocity.x / velocity.z) : 0.0f);
        } 
    }
    
    public float mass {
        get { return Mass; }
        set { Mass = value; }
    }
    
    public float dragC {
        get;
        set;
    } // drag coefficient, for the benefit of AI pilots.
    
	public float tas {
        get { return Mathf.Sqrt(mVelocity.z * mVelocity.z + mVelocity.y * mVelocity.y) * Mathf.Sign(mVelocity.z); }
    } // true air speed
    
    public float ias {
        get { return tas * Mathf.Sqrt(atm.Density(transform.position.y, true)); }
    } // indicated airspeed
    
    public bool isControlable {
        get;
        set;
    }
    
    public bool isLanded {
        get;
        set;
    }
    
    public bool braking {
        get;
        set;
    }
    
    public bool isLandingGearDeployed {
        get;
        set;
    }

    // Use this for initialization
    protected virtual void Start()
	{
        mControlLock = false;
        isLanded = startLanded;
        isControlable = true;
        braking = startLanded;
        isLandingGearDeployed = startLanded;
        rBody = GetComponent<Rigidbody>();
        atm = FindObjectOfType<Atmosphere>();
        velocity = new Vector3(0f, 0f, startSpeed); // TO SCALE ALREADY
        angularVelocity = new Vector3(0f, 0f, 0f);
        //prevVel = transform.forward * startSpeed;
        //isStallSpeed = 1 / (stallSpeed * stallSpeed);
        iScale = 1 / atm.Scale;
        if (rBody != null && mass == 0) mass = rBody.mass;
    }

    public void SetPosition(Vector3 pos, Quaternion rot, float speed)
    {
        transform.position = pos;
        transform.rotation = rot;
        velocity = new Vector3(0f, 0f, speed);
        //prevVel = transform.forward * speed;
        angularVelocity = Vector3.zero;
    }
    
    public void ConnectPitchSet(ILiftingBody.ControlSet aCallback)
    {
        mPitchCallbacks += aCallback;
    }
    
    public void ConnectYawSet(ILiftingBody.ControlSet aCallback)
    {
        mYawCallbacks += aCallback;
    }
    
    public void ConnectRollSet(ILiftingBody.ControlSet aCallback)
    {
        mRollCallbacks += aCallback;
    }
    
    public void ConnectThrustSet(ILiftingBody.ControlSet aCallback)
    {
        mThrustCallbacks += aCallback;
    }
    
    public void ConnectLandEvent(ILiftingBody.FlightEvent aCallback)
    {
        mLandCallbacks += aCallback;
    }
    
    public void ConnectTakeoffEvent(ILiftingBody.FlightEvent aCallback)
    {
        mTakeoffCallbacks += aCallback;
    }
    
    public void ConnectCrashEvent(ILiftingBody.FlightEvent aCallback)
    {
        mCrashCallbacks += aCallback;
    }
    
    public void AdjustPitch(float aPitch)
    {
        if (!mControlLock)
        {
            mPitch += aPitch;
        }
        else
        {
            throw new InvalidOperationException("Pitch cannot be modified once it's finalized.");
        }
    }
    
    public void AdjustYaw(float aYaw)
    {
        if (!mControlLock)
        {
            mYaw += aYaw;
        }
        else
        {
            throw new InvalidOperationException("Yaw cannot be modified once it's finalized.");
        }
    }
    
    public void AdjustRoll(float aRoll)
    {
        if (!mControlLock)
        {
            mRoll += aRoll;
        }
        else
        {
            throw new InvalidOperationException("Roll cannot be modified once it's finalized.");
        }
    }
    
    public void AdjustThrust(float aThrust)
    {
        if (!mControlLock)
        {
            mThrust += aThrust;
        }
        else
        {
            throw new InvalidOperationException("Thrust cannot be modified once it's finalized.");
        }
    }
    
    protected void FinalizeControls()
    {
        mControlLock = true;
        if (mPitchCallbacks != null)
            mPitchCallbacks(mPitch);
        if (mYawCallbacks != null)
            mYawCallbacks(mYaw);
        if (mRollCallbacks != null)
            mRollCallbacks(mRoll);
        if (mThrustCallbacks != null)
            mThrustCallbacks(mThrust);
        mControlLock = false;
    }
    
    protected void ResetControls()
    {
        mPitch = 0.0f;
        mYaw = 0.0f;
        mRoll = 0.0f;
        mThrust = 0.0f;
    }

    // Update is called once per frame
    //void Update()
    //{

    //}
}
