using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicTestEngine : AEngine {

    public override float GetThrust(float airSpeed, float airDensity, float temperature)
    {
        return throtTotal * standingThrust;
        //throw new NotImplementedException();
    }

    public override float InverseGetThrust(float airSpeed, float airDensity, float temperature, float drag)
    {
        float thrust = airSpeed * Mathf.Abs(airSpeed) * drag * airDensity * 0.5f;
        return thrust / standingThrust;
    }

    // Use this for initialization
    //   void Start()
    //{

    //}

    // Update is called once per frame
    //void Update()
    //{

    //}
}
