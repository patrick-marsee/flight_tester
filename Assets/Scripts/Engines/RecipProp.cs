using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipProp : AEngine {

    [Serializable]
    public enum Layout { flat, radial, rotary, vBlock, fuelInjectedVBlock } 
    // Flat and radial are (for now) functionally identical.
    // Rotary causes extra torque. Vee dies when inverted. Fuel injected vee still works inverted, but at lower power.

    [SerializeField]
    private Layout engineLayout; 
    [SerializeField]
    private float criticalAltitude = 0f; // in meters. For naturally aspirated engines, set to 0. For turbocharged engines, set to around 500 to 800.
    private float trueEnginePower; // I called it "power," but I have no unit for it.
    private float criticalPressure = 0f;

    public override float GetThrust(float airSpeed, float airPressure, float temperature) // m/s, atm, K
    {
        float thrust;
        float mach = atmo.Mach(transform.position.y, airSpeed);
        criticalPressure = atmo.Pressure(transform.position.y - criticalAltitude, true);
        trueEnginePower = Mathf.Min(standingThrust * (criticalPressure), standingThrust);
        //print(trueEnginePower);
        if (mach < 1f) thrust = throtTotal * trueEnginePower * Mathf.Sqrt(airPressure) * Mathf.Sqrt(1f - atmo.Mach(transform.position.y, airSpeed));
        else thrust = 0f;
        if (engineLayout == Layout.vBlock || engineLayout == Layout.fuelInjectedVBlock)
        {
            thrust *= Mathf.Cos(Vector3.Angle(Vector3.up, lBody.geeForce) * Mathf.Deg2Rad) * 0.5f + 0.5f ;
            if (engineLayout == Layout.vBlock && Vector3.Angle(Vector3.up, lBody.geeForce) > 100)
            {
                print(Vector3.Angle(Vector3.up, lBody.geeForce));
                thrust = 0f;
                isOn = false;
            }
        }
        return thrust;
        //throw new NotImplementedException();
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}
}
