using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turbojet : AEngine {

    // Use this for initialization
    protected override void Start()
	{
        base.Start();
	}
	
	// Update is called once per frame
	//void Update()
	//{
		
	//}

    public override float GetThrust(float airSpeed, float airDensity, float temperature)
    {
        float M1 = atmo.SpeedOfSound(transform.position.y);
        float tempCoeff = 288.15f / temperature;
        float x = airSpeed * 2 / M1;
        return standingThrust * (1 - x + 0.9375f * x * x - x * x * x / 6f) * airDensity * tempCoeff * throtTotal; // 1 - x + 15x^(2)/16 - x^(3)/6
        //throw new NotImplementedException();
    }

    public override float InverseGetThrust(float airSpeed, float airDensity, float temperature, float drag)
    {
        return -1f;
        //throw new NotImplementedException();
        //float thrust = airSpeed * Mathf.Abs(airSpeed) * drag * airDensity * 0.5f;
        //float x = 0; // x * (-1 + 15x/16 - x^(2)/6) = thrust / (st * ad * tc * tt) - 1
    }
}
