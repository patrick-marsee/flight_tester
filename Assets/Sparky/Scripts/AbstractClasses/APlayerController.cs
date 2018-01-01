using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class APlayerController : MonoBehaviour {

    [SerializeField]
    protected string pitchAxis = "Vertical";
    [SerializeField]
    protected string yawAxis = "Yaw";
    [SerializeField]
    protected string rollAxis = "Horizontal";
    [SerializeField]
    protected string throttleAxis = "Throttle";
    [SerializeField]
    protected float aoaLimit = 0f;
    [SerializeField]
    protected float aoaMargin = 5f;
    [SerializeField]
    protected float gLimit = 0f;
    [SerializeField]
    protected float gMargin = 1f;
    [SerializeField]
    protected float minAltitude = 0f;
    [SerializeField]
    protected float maxAltitude = 10000f;
    protected float sqrGLimit = 0f;
    protected float pitch, yaw, roll, throttle;
    //protected Rigidbody rBody;
    protected ILiftingBody lBody;
    protected IEngine[] engine; // there may be multiple kinds of engine on one aircraft.

    // Use this for initialization
    void Start()
    {
        //rBody = GetComponent<Rigidbody>();
        lBody = GetComponent<ILiftingBody>();
        engine = GetComponents<IEngine>();
        if (gLimit > gMargin)
            sqrGLimit = (gLimit - gMargin) * (gLimit - gMargin);
    }

    protected virtual void SetControls()
    {
        if (lBody.isControlable)
        {
            lBody.pitch = pitch;
            lBody.yaw = yaw;
            lBody.roll = roll;
            for (int i = 0; i < engine.Length; i++)
            {
                engine[i].SetThrottle(throttle);
            }
        }
    }

    // Update is called once per frame
    //   void Update()
    //{

    //}
}
