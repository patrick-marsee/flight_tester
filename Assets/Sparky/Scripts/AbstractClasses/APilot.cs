using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class APilot : MonoBehaviour {

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

    public Vector3 target { get; set; } // The point in space that the pilot is trying to get to.
    protected float pitch, yaw, roll, throttle;
    //protected Rigidbody rBody;
    protected ILiftingBody lBody;
    protected IEngine[] engine;

	// Use this for initialization
	protected virtual void Start()
	{
        //rBody = GetComponent<Rigidbody>();
        lBody = GetComponent<ILiftingBody>();
        engine = GetComponents<IEngine>();
	}
	
	// Update is called once per frame
	//void Update()
	//{
		
	//}

    protected virtual void SetControls()
    {
        if (lBody.isControlable)
        {
            lBody.AdjustPitch(pitch);
            lBody.AdjustYaw(yaw);
            lBody.AdjustRoll(roll);
            for (int i = 0; i < engine.Length; i++)
            {
                engine[i].SetThrottle(throttle);
            }
        }
    }
}
