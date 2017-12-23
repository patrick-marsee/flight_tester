using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    // This is an outdated version of FlightSimController. Use that one instead!

    [SerializeField]
    private float aoaLimit = 0f;
    [SerializeField]
    private float aoaMargin = 5f;
    [SerializeField]
    private float gLimit = 0f;
    [SerializeField]
    private float gMargin = 1f;
    private float sqrGLimit = 0f;
    private Rigidbody rBody;
    private LiftingBody lBody;
    private IEngine[] engine; // there may be multiple kinds of engine on one aircraft.

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
        lBody = GetComponent<LiftingBody>();
        engine = GetComponents<IEngine>();
        if (gLimit > gMargin)
            sqrGLimit = (gLimit - gMargin) * (gLimit - gMargin);
	}
	
	// Update is called once per frame
	void Update () {
        //Vector3 relativeRot = Vector3.Project(transform.forward, rBody.angularVelocity);
        //relativeRot *= Mathf.Sqrt(rBody.angularVelocity.sqrMagnitude / relativeRot.sqrMagnitude);
        lBody.roll = -Input.GetAxis("Horizontal");// - relativeRot.z;
        lBody.pitch = Input.GetAxis("Vertical");// - relativeRot.x;
        if (aoaLimit != 0f)
        {
            float aoa = lBody.AoA * Mathf.Rad2Deg;
            //print(aoa);
            if (aoa > aoaLimit - aoaMargin)
                lBody.pitch = Mathf.Max(lBody.pitch, Mathf.Clamp((aoa - aoaLimit) / aoaMargin, -1f, 1f));
            
            else if (aoa < -aoaLimit + aoaMargin)
                lBody.pitch = Mathf.Min(lBody.pitch, Mathf.Clamp((aoaLimit + aoa) / aoaMargin, -1f, 1f));
            //print(lBody.pitch);
        }
        if (gLimit != 0f)
        {
            float g = lBody.geeForce.y;

            if (g > sqrGLimit)
                lBody.pitch = Mathf.Min(lBody.pitch, (gLimit * gLimit - g) / gMargin);
            else if (g < -sqrGLimit)
                lBody.pitch = Mathf.Max(lBody.pitch, (g - gLimit * gLimit) / gMargin);

        }
        lBody.yaw = Input.GetAxis("Yaw");
        for (int i = 0; i < engine.Length; i++)
            engine[i].SetThrottle(Input.GetAxis("Throttle") * 0.5f + 0.5f);
        //rBody.AddRelativeTorque(new Vector3(pitch, 0f, roll));
        //rBody.AddTorque(-rBody.angularVelocity);
	}
}

public struct SphereCoords
{
    private float r, p, t;
    
    public SphereCoords(float rho, float phi, float theta)
    {
        r = Mathf.Abs(rho);
        p = Mathf.Clamp(phi, 0.0f, Mathf.PI);
        t = theta;
    }

    public SphereCoords(Vector3 original)
    {
        r = original.magnitude;
        if (original.x != 0) t = Mathf.Atan(original.y / original.x);
        else t = Mathf.PI / 2;
        if (original.x <= 0) t += Mathf.PI;
        if (t < 0) t += Mathf.PI * 2;
        p = Mathf.Asin(Mathf.Sqrt(original.x * original.x + original.y * original.y) / r);
        if (p < 0) p += Mathf.PI;
    }

    public Vector3 GetComponents()
    {
        return new Vector3(r, p, t);
    }

    public Vector3 ToVector3()
    {
        float x = r * Mathf.Sin(p) * Mathf.Cos(t), y = r * Mathf.Sin(p) * Mathf.Sin(t), z = r * Mathf.Cos(p);
        return new Vector3(x, y, z);
    }

    public static SphereCoords operator* (float n, SphereCoords k)
    {
        Vector3 temp = k.GetComponents();
        return new SphereCoords(temp.x * n, temp.y, temp.z);
    }

    public static SphereCoords operator *(SphereCoords n, float k)
    {
        Vector3 temp = n.GetComponents();
        return new SphereCoords(temp.x * k, temp.y, temp.z);
    }
}