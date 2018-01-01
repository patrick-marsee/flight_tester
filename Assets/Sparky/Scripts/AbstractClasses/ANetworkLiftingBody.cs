using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class ANetworkLiftingBody : NetworkBehaviour, ILiftingBody {

    protected struct liftingBodyPacket
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 vel, angVel, accel;
        public float pitch, yaw, roll, thrust;

        public liftingBodyPacket(Transform transform, ANetworkLiftingBody nlBody)
        {
            pos = transform.position;
            rot = transform.rotation;
            vel = nlBody.velocity;
            angVel = nlBody.angularVelocity;
            accel = nlBody.acceleration;
            pitch = nlBody.pitch;
            yaw = nlBody.yaw;
            roll = nlBody.roll;
            thrust = nlBody.thrust;
        }
    }

    [SerializeField]
    private Vector3 Velocity;
    [SerializeField]
    private Vector3 AngularVelocity;
    [SerializeField]
    private float startSpeed;
    [SerializeField]
    private float Mass;
    [SerializeField]
    protected float refreshRate = 9;
    protected float timeSinceLastRefresh = 0;

    protected Rigidbody rBody;
    protected Atmosphere atm;
    protected Vector3 prevVel;
    protected float iScale; // inverse scale (Note: only scale when the transform is being used!)

    public Vector3 velocity { get { return Velocity; } set { Velocity = value; } } // meters per second
    public Vector3 angularVelocity { get { return AngularVelocity; } set { AngularVelocity = value; } } // degrees per second
    public Vector3 indicatedVelocity { get { return velocity * Mathf.Sqrt(atm.Density(transform.position.y, true)); } }
    public Vector3 acceleration { get; set; }
    public Vector3 angularAcceleration { get; set; }
    public Vector3 geeForce { get { return (acceleration - transform.InverseTransformDirection(Physics.gravity)) / Physics.gravity.magnitude; } }
    public float AoA { get { return -Mathf.Atan(velocity.y / velocity.z); } } // radians
    public float sideslip { get { return Mathf.Atan(velocity.x / velocity.z); } }
    public float mass { get { return Mass; } set { Mass = value; } }
    public float pitch { get; set; }
    public float yaw { get; set; }
    public float roll { get; set; }
    public float thrust { get; set; }
    public float dragC { get; set; } // drag coefficient, for the benefit of AI pilots.
    public float tas { get { return velocity.z; } } // true air speed
    public float ias { get { return tas * Mathf.Sqrt(atm.Density(transform.position.y, true)); } } // indicated airspeed
    public bool isControlable { get; set; }

    // Use this for initialization
    protected virtual void Start()
    {
        if (!hasAuthority)
            isControlable = false;
        else
            isControlable = true;
        rBody = GetComponent<Rigidbody>();
        atm = FindObjectOfType<Atmosphere>();
        velocity = new Vector3(0f, 0f, startSpeed); // TO SCALE ALREADY
        angularVelocity = new Vector3(0f, 0f, 0f);
        prevVel = new Vector3(0f, 0f, startSpeed);
        //isStallSpeed = 1 / (stallSpeed * stallSpeed);
        iScale = 1 / atm.Scale;
        if (rBody != null && mass == 0) mass = rBody.mass;
    }

    protected virtual void Update()
    {
        if (hasAuthority)
        {
            timeSinceLastRefresh += Time.deltaTime;
            if (timeSinceLastRefresh > 1 / refreshRate)
            {
                timeSinceLastRefresh -= 1 / refreshRate;
                if (isClient)
                    RpcSyncLB(new liftingBodyPacket(transform, this));
                else if (isServer)
                    CmdSyncLB(new liftingBodyPacket(transform, this));

            }
        }
    }

    [Command]
    protected void CmdSyncLB(liftingBodyPacket lbp)
    {
        if (!isLocalPlayer)
        {
            transform.position = lbp.pos;
            transform.rotation = lbp.rot;
            velocity = lbp.vel;
            angularVelocity = lbp.vel;
            acceleration = lbp.accel;
            pitch = lbp.pitch;
            yaw = lbp.yaw;
            roll = lbp.roll;
            thrust = lbp.thrust;
        }
    }

    [ClientRpc]
    protected void RpcSyncLB(liftingBodyPacket lbp)
    {
        transform.position = lbp.pos;
        transform.rotation = lbp.rot;
        velocity = lbp.vel;
        angularVelocity = lbp.vel;
        acceleration = lbp.accel;
        pitch = lbp.pitch;
        yaw = lbp.yaw;
        roll = lbp.roll;
        thrust = lbp.thrust;
        CmdSyncLB(lbp);
    }

    public void SetPosition(Vector3 pos, Quaternion rot, float speed)
    {
        transform.position = pos;
        transform.rotation = rot;
        velocity = new Vector3(0f, 0f, speed);
        prevVel = transform.forward * speed;
        angularVelocity = Vector3.zero;
    }
}