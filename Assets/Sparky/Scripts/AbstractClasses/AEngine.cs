using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AEngine : MonoBehaviour, IEngine {

    protected ALiftingBody lBody;
    protected Atmosphere atmo;
    protected float[] throt;
    protected float throtTotal // all of the engine's throttle settings added together. Useful? Don't question it.
    {
        get
        {
            float ret = 0f;
            foreach (float i in throt)
                ret += i;
            return ret;
        }
    }
    [SerializeField]
    protected Vector3[] position;
    [SerializeField]
    protected float standingThrust; // thrust when sitting still at 1 atm.

    public float[] throttle { get { return throt; } set { throt = value; } }
    public bool isOn { get; set; }

    // Use this for initialization
    protected virtual void Start()
	{
        lBody = GetComponent<ALiftingBody>();
        atmo = FindObjectOfType<Atmosphere>();
        throt = new float[position.Length];
        isOn = true;
        //print(throt.Length);
	}

    //Update is called once per frame

    //void Update()
    //{

    //}

    protected virtual void FixedUpdate()
    {
        //print(isOn);
        if (isOn && lBody.isControlable)
        {
            lBody.AdjustThrust(GetThrust(lBody.tas, atmo.Density(atmo.Altitude(transform.position.y), true), atmo.Temperature(atmo.Altitude(transform.position.y))));
        }
    }

    // current true airSpeed, relative density at curent altitude, temperature at current altitude
    public abstract float GetThrust(float airSpeed, float airDensity, float temperature);

    // desired true airspeed, relative density at desired altitude, temperature at desired altitude, aircraft's drag coefficient.
    public virtual float InverseGetThrust(float airSpeed, float airDensity, float temperature, float drag)
    {
        return -1f; // This is to let an AI pilot know that InverseGetThrust is unavailable for the engine type.
    }

    public void SetThrottle(float throt)
    {
        if (this.throt == null) return;
        for (int i = 0; i < this.throt.Length; i++)
            this.throt[i] = throt;
    }

    public Vector3[] GetPosions()
    {
        return position;
    }
}
