using UnityEngine;
using System.Collections;
using System;

public class LiftingBody : ALiftingBody {

    //[SerializeField]
    //private Vector3 Velocity;
    //[SerializeField]
    //private Vector3 AngularVelocity;
    [SerializeField]
    private bool WaitForUpdate; // slightly less accurate and goes against Unity convention, but is faster.
    //[SerializeField]
    //private float startSpeed;
    [SerializeField]
    private bool useIndividualWings;
    [SerializeField]
    private Wing[] wings;
    [SerializeField]
    private float wingArea; // depricated
    [SerializeField]
    private Vector3 controlResponse;
    [SerializeField]
    private float lateralStability;
    [SerializeField]
    private float stallSpeed; // depricated
    [SerializeField]
    private float dragCoeff;
    //[SerializeField]
    //private float Mass;
    [SerializeField]
    private float momentOfInertia;
    //private Rigidbody rBody;
    //private Atmosphere atm;
    //private Vector3 control;
    //private Vector3 prevVel;
    // useful constants
    private float isStallSpeed; // inverse, squared: 1 / (stallSpeed^2)
    //private float iScale; // inverse scale (Note: only scale when the transform is being used!)

    private LiftingBodyProfile lbProf;

    //public Vector3 velocity { get { return Velocity; } set { Velocity = value; } } // meters per second
    //public Vector3 angularVelocity { get { return AngularVelocity; } set { AngularVelocity = value; } } // degrees per second
    //public Vector3 indicatedVelocity { get { return velocity * atm.Pressure(transform.position.y, true); } }
    //public Vector3 acceleration { get; set; }
    //public Vector3 angularAcceleration { get; set; }
    //public Vector3 geeForce { get { return (acceleration - transform.InverseTransformDirection(Physics.gravity)) / Physics.gravity.magnitude; } }
    //public float AoA { get { return -Mathf.Atan(velocity.y / velocity.z); } } // radians
    //public float mass { get { return Mass; } set { Mass = value; } }
    //public float pitch { get; set; }
    //public float yaw { get; set; }
    //public float roll { get; set; }
    //public float thrust { get; set; }
    //public float tas { get { return velocity.z; } } // true air speed
    //public float ias { get { return tas * atm.Pressure(transform.position.y, true); } } // indicated airspeed
    public float liftLimit {
        get {
            float speed = ias; // calculate ias only once
            return isStallSpeed * speed * speed;
        }
    }

	// Use this for initialization
	override protected void Start () {
        base.Start();
        //rBody = GetComponent<Rigidbody>();
        //atm = FindObjectOfType<Atmosphere>();
        //velocity = new Vector3(0f, 0f, startSpeed); // TO SCALE ALREADY
        //angularVelocity = new Vector3(0f, 0f, 0f);
        //prevVel = new Vector3(0f, 0f, startSpeed);
        isStallSpeed = 1 / (stallSpeed * stallSpeed);
        //iScale = 1 / atm.Scale;
        //if (rBody != null && mass == 0) mass = rBody.mass;
        for (int i = 0; i < wings.Length; i++)
        {
            wings[i].calculate();
        }
        if (!useIndividualWings)
            GenerateFlightModel();
        //print("Anything");
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //inertia();
        //Vector3 gravity = transform.InverseTransformDirection(Physics.gravity);
        acceleration = transform.InverseTransformDirection(Physics.gravity);
        acceleration += Vector3.forward * thrust / mass;
        //Vector3 lift = Vector3.up * wings.getLift(AoA, velocity.z) * Time.fixedDeltaTime;
        lift();
        //keel();
        drag();
        velocity += acceleration * Time.fixedDeltaTime;
        angularVelocity += angularAcceleration * Time.fixedDeltaTime;
        //print(velocity);
        transform.Translate(velocity * Time.fixedDeltaTime * iScale);
        Vector3 prevVel = transform.TransformDirection(velocity);
        transform.Rotate(angularVelocity * Time.fixedDeltaTime);
        velocity = transform.InverseTransformDirection(prevVel);
        //transform.Rotate(new Vector3(pitch * controlResponse.x * Mathf.Sqrt(ias), yaw * controlResponse.y * Mathf.Sqrt(ias), roll * controlResponse.z * Mathf.Sqrt(ias)));
    }

    private void Update()
    {
        //print(geeForce);
        if (WaitForUpdate && !useIndividualWings)
            lbProf.Update(indicatedVelocity, angularVelocity * atm.Pressure(transform.position.y, true));
    }

    private void GenerateFlightModel() // generate an overall flight model from the wings. Probably less accurate, but more stable and less CPU intense.
    {
        lbProf = new LiftingBodyProfile(wings);
    }

    //private void inertia()
    //{
    //    velocity = transform.InverseTransformDirection(prevVel);
    //    //acceleration = velocity - prevVel;
    //}

    private void lift() // actually, all forces applied by airfoils
    {
        //Vector2 verticalVel = new Vector2(velocity.z, velocity.y);
        //float AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
        //print("AoA: " + AoA);
        //float speed = verticalVel.magnitude;
        //velocity += Vector3.up * wings.getLift(AoA) * wingArea * Time.fixedDeltaTime * liftLimit / mass;
        if (useIndividualWings)
        {
            Vector3[] linearForces = new Vector3[wings.Length];
            Vector3[] angularForces = new Vector3[wings.Length];
            //float[] dragForces = new float[wings.Length];
            //Vector3 drag = -velocity.normalized;
            for (int i = 0; i < wings.Length; i++) // first, get all lift, torque, and drag from the wings.
            {
                wings[i].control = new Vector3(pitch, yaw, roll);
                Forces forces = wings[i].getForces(indicatedVelocity, angularVelocity);
                linearForces[i] = forces.linearForces * Time.fixedDeltaTime;
                angularForces[i] = forces.angularForces * Time.fixedDeltaTime;
                //linearForces[i] = wings[i].getLinearForces(indicatedVelocity) * Time.fixedDeltaTime;
                //angularForces[i] = wings[i].getAngularForces(indicatedVelocity) * Time.fixedDeltaTime;
                //dragForces[i] = wings[i].getDrag(indicatedVelocity) * Time.fixedDeltaTime;
            }
            //angularVelocity = new Vector3();
            for (int j = 0; j < wings.Length; j++) // then, all lift, torque, and drag are composited. Drag is done afterwards.
            {
                velocity += linearForces[j];
                angularVelocity += angularForces[j];
                //float xTorque = linearForces[j].y * -wings[j].position.z * Time.fixedDeltaTime * 200;
                //float yTorque = linearForces[j].x * wings[j].position.z * Time.fixedDeltaTime * 200;
                //Vector2 wingAvePos = new Vector2(wings[j].position.x + Mathf.Cos(wings[j].dihedral) * wings[j].span * 0.5f, wings[j].position.x + Mathf.Sin(wings[j].dihedral) * wings[j].span * 0.5f);
                //if (wings[j].symmetrical) wingAvePos = new Vector2(0f, wingAvePos.y);
                //float zTorque = linearForces[j].magnitude * wingAvePos.magnitude * 100;
                //angularVelocity += new Vector3(xTorque, yTorque, 0f);
                //velocity += drag * dragForces[j];
            }
        }
        else // holistic calculation v.0.1
        {
            lbProf.pitch = pitch;
            lbProf.yaw = yaw;
            lbProf.roll = roll;
            if (!WaitForUpdate)
                lbProf.Update(indicatedVelocity, angularVelocity * atm.Pressure(transform.position.y, true));
            Vector3 accel = lbProf.GetLinearForces(indicatedVelocity) / mass;
            acceleration += accel;
            //velocity += accel;
            angularAcceleration = lbProf.GetMoment(momentOfInertia);
            //print("accel = " + accel.ToString());
            //print("angularVelocity = " + angularVelocity.ToString());
        }
        //velocity += Vector3.up * 
    }

    private void keel() // sideslip drag, effect of vert stab; don't think this is actually what it's called...
    {
        float horizVel = velocity.x;
        velocity += Vector3.left * horizVel * Mathf.Abs(horizVel) * Time.fixedDeltaTime; 
    }

    private void drag()
    {
        acceleration += Vector3.back * ias * Mathf.Abs(ias) * dragCoeff;
    }
}

class LiftingBodyProfile
{
    private const float FIX_LIFT = 0.5f; // multiply lift by this for correct results
    private const float FIX_DRAG = 0.5f; // multiply drag by this for correct results
    private const float FIX_MOMENT = 0.02f; // multiply this by moments for correct results

    private Wing[] wings;
    private int wingNum = 0; // the number of INDIVIDUAL wings (symmetrical counts as 2)
    private bool[] hasHorizComponent; // for Convinience and Depth
    private bool[] hasVertComponent;
    private Vector3[] wingACs;
    private float horizWingArea;
    private float vertWingArea;
    private Airfoil horizAirfoil;
    private Airfoil vertAirfoil;
    private Vector3 horizAerodynamicCenter;
    private Vector3 vertAerodynamicCenter;
    private float horizLift; // lift from horizontal surfaces
    private float vertLift; // 'lift' from vertical surfaces
    private float horizTorque;
    private float vertTorque;
    private float drag;
    private float AoA;

    // control surface deployment
    public float pitch { get; set; }
    public float yaw { get; set; }
    public float roll { get; set; }

    public LiftingBodyProfile(Wing[] wings)
    {
        this.wings = wings;
        hasHorizComponent = new bool[wings.Length];
        hasVertComponent = new bool[wings.Length];
        for (int i = 0; i < wings.Length; i++)
        {
            wingNum++;
            if (wings[i].symmetrical) wingNum++;
            hasHorizComponent[i] = true;
            hasVertComponent[i] = true;
        }
        wingACs = new Vector3[wingNum];
        horizWingArea = 0f;
        vertWingArea = 0f;
        horizAerodynamicCenter = Vector3.zero;
        vertAerodynamicCenter = Vector3.zero;

        int j = 0; // j keeps track of individual wings for wingACs[].
        for (int i = 0; i < wings.Length; i++)
        {
            if (wings[i].dihedral == 0) hasVertComponent[i] = false;
            else if (wings[i].dihedral == 90 || wings[i].dihedral == -90) hasHorizComponent[i] = false;
            float wingArea = wings[i].area;
            float thisHorizWingArea;
            float thisVertWingArea;
            //wingACs[j] = new Vector3(wings[i].span * Mathf.Cos(wings[i].dihedral * Mathf.Deg2Rad) * wings[i].center + wings[i].position.x,
            //                             wings[i].span * Mathf.Sin(wings[i].dihedral * Mathf.Deg2Rad) * wings[i].center + wings[i].position.y,
            //                             wings[i].position.z);
            wingACs[j] = wings[i].aerodynamicCenter;
            MonoBehaviour.print("wingACs[" + j + "] = " + wingACs[j].ToString());

            if (wings[i].symmetrical)
            {
                j++;
                wingArea += wingArea;
                //wingACs[j] = new Vector3(-(wings[i].span * Mathf.Cos(wings[i].dihedral * Mathf.Deg2Rad) * wings[i].center + wings[i].position.x),
                //                         wings[i].span * Mathf.Sin(wings[i].dihedral * Mathf.Deg2Rad) * wings[i].center + wings[i].position.y,
                //                         wings[i].position.z);
                wingACs[j] = Vector3.Scale(wings[i].aerodynamicCenter, new Vector3(-1, 1, 1));
                MonoBehaviour.print("wingACs[" + j + "] = " + wingACs[j].ToString());
            }
            thisHorizWingArea = wingArea * Mathf.Cos(wings[i].dihedral * Mathf.Deg2Rad);
            thisVertWingArea = wingArea * Mathf.Abs(Mathf.Sin(wings[i].dihedral * Mathf.Deg2Rad));
            horizWingArea += thisHorizWingArea;
            vertWingArea += thisVertWingArea;
            Vector3 centeredAC = new Vector3(0, 1, 1);
            centeredAC.Scale(wingACs[j]);
            horizAerodynamicCenter += centeredAC * thisHorizWingArea;
            vertAerodynamicCenter += centeredAC * thisVertWingArea;
            j++;
        }

        if (horizWingArea != 0)
            horizAerodynamicCenter /= horizWingArea;
        if (vertWingArea != 0)
            vertAerodynamicCenter /= vertWingArea;
        MonoBehaviour.print("horizAerodynamicCenter = " + horizAerodynamicCenter.ToString());
        MonoBehaviour.print("vertAerodynamicCenter = " + vertAerodynamicCenter.ToString());
    }

    public void Update(Vector3 vel, Vector3 angVel)
    {
        //MonoBehaviour.print("angVel = " + angVel);
        float AoN = Vector3.Angle(vel, Vector3.forward); // Angle off nose - AoA and sideslip angle
        float angleAngle = Vector2.Angle(new Vector2(vel.x, vel.y), Vector2.right) * -Mathf.Sign(vel.y); // the angle of the angle off nose
        //AoA = Vector2.Angle(new Vector2(vel.z, vel.y), Vector2.right) * -Mathf.Sign(vel.y);
        //float sideslipAngle = Vector2.Angle(new Vector2(vel.z, vel.x), Vector2.right) * Mathf.Sign(vel.x);
        horizLift = 0;
        vertLift = 0;
        horizTorque = 0;
        vertTorque = 0;
        drag = 0;
        horizAerodynamicCenter = Vector3.zero;
        vertAerodynamicCenter = Vector3.zero;

        int j = 0; // j keeps track of individual wings for wingACs[].
        for (int i = 0; i < wings.Length; i++)
        {
            float relativeAoA = AoN * Mathf.Sin((angleAngle - wings[i].dihedral) * Mathf.Deg2Rad);
            if (wings[i].pitchControl) relativeAoA += wings[i].controlAmount * -Mathf.Sign(wings[i].position.z) * pitch;
            if (wings[i].yawControl) relativeAoA -= wings[i].controlAmount * -Mathf.Sign(wings[i].position.z) * yaw;
            if (wings[i].rollControl) relativeAoA += wings[i].controlAmount * roll;
            relativeAoA += Vector3.Dot(wings[i].angularResistance, angVel) * 0.02f; // This probably works, right?
            //float fixedVel = vel.sqrMagnitude * FIX_LIFT;
            float areaVel = wings[i].area * vel.sqrMagnitude * FIX_LIFT;
            float spanVel = wings[i].area * vel.sqrMagnitude * FIX_DRAG;
            float camber = 0f; // camber added by various flaps
            if (wings[i].elevators) camber -= Mathf.Sign(wings[i].position.z) * pitch;
            if (wings[i].rudder) camber += Mathf.Sign(wings[i].position.z) * yaw;
            if (wings[i].alerons) camber += roll;
            float lift = (wings[i].activeAirfoil.getLift(relativeAoA) + camber) * areaVel;
            //MonoBehaviour.print("wings[" + i + "] lift = " + lift);
            float thisHorizLift;
            float thisVertLift;
            float thisDrag;
            //if (wings[i].symmetrical) lift += lift;
            if (hasHorizComponent[i]) // wing is horizontal
            {
                thisHorizLift = Mathf.Cos(wings[i].dihedral * Mathf.Deg2Rad) * lift; // lift in the VERTICAL direction, caused by the HORIZONTAL component of the wing.
                horizAerodynamicCenter += wingACs[j] * thisHorizLift; // how much the lift here affects how the plane pitches and rolls
                horizLift += thisHorizLift; // add this wing's lift to the lift sum
                horizTorque += Mathf.Abs(thisHorizLift); // * Mathf.Cos(relativeAoA * Mathf.Deg2Rad);
            }
            if (hasVertComponent[i]) // wing is vertical
            {
                thisVertLift = Mathf.Abs(Mathf.Sin(wings[i].dihedral * Mathf.Deg2Rad)) * lift;
                vertAerodynamicCenter += wingACs[j] * thisVertLift;
                vertLift += thisVertLift;
                vertTorque += Mathf.Abs(thisVertLift); // * Mathf.Cos(relativeAoA * Mathf.Deg2Rad);
            }
            thisDrag = wings[i].activeAirfoil.getDrag(relativeAoA) * spanVel;
            horizTorque += thisDrag * Mathf.Abs(Mathf.Sin(relativeAoA));
            vertTorque += thisDrag * Mathf.Abs(Mathf.Sin(relativeAoA));
            if (wings[i].symmetrical)
            {
                j++;
                relativeAoA = AoN * Mathf.Sin((angleAngle - wings[i].dihedral) * Mathf.Deg2Rad);
                if (wings[i].pitchControl) relativeAoA += wings[i].controlAmount * -Mathf.Sign(wings[i].position.z) * pitch;
                if (wings[i].yawControl) relativeAoA += wings[i].controlAmount * -Mathf.Sign(wings[i].position.z) * yaw;
                if (wings[i].rollControl) relativeAoA -= wings[i].controlAmount * roll;
                relativeAoA += Vector3.Dot(Vector3.Scale(wings[i].angularResistance, new Vector3(1, -1, -1)), angVel) * 0.02f;
                camber = 0f; // camber added by various flaps
                if (wings[i].elevators) camber -= Mathf.Sign(wings[i].position.z) * pitch;
                if (wings[i].rudder) camber -= Mathf.Sign(wings[i].position.z) * yaw;
                if (wings[i].alerons) camber -= roll;
                lift = (wings[i].activeAirfoil.getLift(relativeAoA) + camber) * areaVel;
                //lift = wings[i].activeAirfoil.getLift(relativeAoA) * areaVel;
                if (hasHorizComponent[i])
                {
                    thisHorizLift = Mathf.Cos(wings[i].dihedral * Mathf.Deg2Rad) * lift;
                    horizAerodynamicCenter += wingACs[j] * thisHorizLift;
                    horizLift += thisHorizLift;
                    horizTorque += Mathf.Abs(thisHorizLift); // * Mathf.Cos(relativeAoA * Mathf.Deg2Rad);
                }
                if (hasVertComponent[i])
                {
                    thisVertLift = Mathf.Abs(Mathf.Sin(wings[i].dihedral * Mathf.Deg2Rad)) * lift;
                    vertAerodynamicCenter += wingACs[j] * thisVertLift;
                    vertLift += thisVertLift;
                    vertTorque += Mathf.Abs(thisVertLift); // * Mathf.Cos(relativeAoA * Mathf.Deg2Rad);
                }
                thisDrag += wings[i].activeAirfoil.getDrag(relativeAoA) * spanVel;
                horizTorque += thisDrag * Mathf.Abs(Mathf.Sin(relativeAoA));
                vertTorque += thisDrag * Mathf.Abs(Mathf.Sin(relativeAoA));
            }
            //MonoBehaviour.print("thisHorizLift = " + horizLift);
            //vertAerodynamicCenter += wingACs[j] * thisVertLift;
            //horizAerodynamicCenter += wingACs[j] * thisHorizLift;
            //horizLift += thisHorizLift;
            //vertLift += thisVertLift;
            //thisDrag = wings[i].activeAirfoil.getDrag(AoA) * wings[i].span * vel.sqrMagnitude * FIX_LIN_FORCE;
            //if (wings[i].symmetrical) thisDrag += thisDrag;
            drag += thisDrag;
            j++;
        }

        if (horizTorque != 0)
            horizAerodynamicCenter /= horizTorque;
        if (vertTorque != 0)
            vertAerodynamicCenter /= vertTorque;
        //if (horizLift != 0)
        //    horizAerodynamicCenter /= horizLift;
        //if (vertLift != 0)
        //    vertAerodynamicCenter /= vertLift;
        //MonoBehaviour.print("horizAerodynamicCenter = " + horizAerodynamicCenter.ToString());
        //MonoBehaviour.print("vertAerodynamicCenter = " + vertAerodynamicCenter.ToString());
    }

    public Vector3 GetLinearForces(Vector3 vel) // returns linear forces on the basis of the aircraft's current velocity.
    {
        //MonoBehaviour.print("vertLift = " + vertLift);
        Vector3 normalizedVel = vel.normalized;
        Vector3 retDrag = normalizedVel * -drag;
        Vector3 retVertLift = new Vector3(normalizedVel.z, normalizedVel.y, -normalizedVel.x) * vertLift;
        Vector3 retHorizLift = new Vector3(normalizedVel.x, normalizedVel.z, -normalizedVel.y) * horizLift;
        return retDrag + retHorizLift + retVertLift;
    }

    public Vector3 GetMoment(float momentOfInertia)
    {
        //MonoBehaviour.print("vertAC = " + vertAerodynamicCenter.ToString());
        float pitchMoment = -horizAerodynamicCenter.z * horizTorque / momentOfInertia;// + pitch * 60;
        float yawMoment = vertAerodynamicCenter.z * vertTorque / momentOfInertia;// + yaw * 30;
        float rollMoment = horizAerodynamicCenter.x * horizTorque / momentOfInertia + vertAerodynamicCenter.y * vertTorque / momentOfInertia;// + roll * 90;
        //if (pitchMoment == float.NaN)
            //MonoBehaviour.print("pitchMoment = " + pitchMoment);
        //if (yawMoment == float.NaN)
            //MonoBehaviour.print("yawMoment = " + yawMoment);
        //if (rollMoment == float.NaN)
            //MonoBehaviour.print("rollMoment = " + rollMoment);
        return new Vector3(pitchMoment, yawMoment, rollMoment);
    }

    public Vector3 GetHorizAC()
    {
        return horizAerodynamicCenter;
    }

    public Vector3 GetVertAC()
    {
        return vertAerodynamicCenter;
    }

    public float GetAoA()
    {
        return AoA;
    }
}