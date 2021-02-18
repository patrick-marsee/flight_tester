using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ALiftingBody : MonoBehaviour, ILiftingBody {
    // This is really being used more like an interface. I'm using an abstract class instead beacause interfaces make property accessors a pain.

    [SerializeField]
    private Vector3 Velocity;
    [SerializeField]
    private Vector3 AngularVelocity;
    [SerializeField]
    private float startSpeed;
    [SerializeField]
    private float Mass;

    protected Rigidbody rBody;
    protected Atmosphere atm;
    //protected Vector3 prevVel;
    protected float iScale; // inverse scale (Note: only scale when the transform is being used!)

    public Vector3 velocity { get { return Velocity; } set { Velocity = value; } } // meters per second
    public Vector3 angularVelocity { get { return AngularVelocity; } set { AngularVelocity = value; } } // degrees per second
    public Vector3 indicatedVelocity { get { return velocity * Mathf.Sqrt(atm.Density(transform.position.y, true)); } }
    public Vector3 acceleration { get; set; }
    public Vector3 angularAcceleration { get; set; }
    public Vector3 geeForce { get { return (acceleration - transform.InverseTransformDirection(Physics.gravity)) / Physics.gravity.magnitude; } }
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
    public float mass { get { return Mass; } set { Mass = value; } }
    public float pitch { get; set; }
    public float yaw { get; set; }
    public float roll { get; set; }
    public float thrust { get; set; }
    public float dragC { get; set; } // drag coefficient, for the benefit of AI pilots.
	public float tas { get { return Mathf.Sqrt(velocity.z * velocity.z + velocity.y * velocity.y) * Mathf.Sign(velocity.z); } } // true air speed
    public float ias { get { return tas * Mathf.Sqrt(atm.Density(transform.position.y, true)); } } // indicated airspeed
    public bool isControlable { get; set; }
    public bool isLanded { get; set; }

    // Use this for initialization
    protected virtual void Start()
	{
        isLanded = false;
        isControlable = true;
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

    // Update is called once per frame
    //void Update()
    //{

    //}
}
