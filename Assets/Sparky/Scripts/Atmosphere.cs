using UnityEngine;
using System.Collections;

public class Atmosphere : MonoBehaviour {

    [SerializeField]
    private float scale = 1; // meters per unit
    [SerializeField]
    private float seaLevel = 0;
    [SerializeField]
    private float pressureASL = 101.325f; // 1 atm. in kPa
    [SerializeField]
    private float densityASL = 1.225f; // at 288.15k in kg/m^3
    [SerializeField]
    private float temperatureASL = 288.15f; // surface temperature in Kelvin (288.15K = 15C)
    [SerializeField]
    private float tropopauseAltitude = 11000f; // meters above ASL
    [SerializeField]
    private float tropopauseTemp = 216.65f; // (216.65K = -56.5C)
    private const float AIR_MOLAR_MASS = 0.029f; // kg/mol
    private const float GAS_CONSTANT = 8.31447f;
    private const float MACH_COEFF = 20.02948f;
    private float pressureConstant;
    private float densityConstant;
    private float tempLapseAtTropo; // temperature lapse at troposphere, in K or C

    public float Scale
    {
        get { return scale; }
    }

    public float SeaLevel
    {
        get { return seaLevel; }
    }

    // Use this for initialization
    void Start () {
        pressureConstant = (Physics.gravity.y * AIR_MOLAR_MASS * scale) / (GAS_CONSTANT * temperatureASL);
        densityConstant = 1 / densityASL;
        tempLapseAtTropo = (temperatureASL - tropopauseTemp) / tropopauseAltitude;
        //print(pressureConstant);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public float Pressure(float altitude, bool atm = false) // returns the pressure at altitude in kPa (default) or atm.
    {
        float ret = Mathf.Exp(pressureConstant * altitude);
        if (!atm)
            ret *= pressureASL;
        return ret;
    }

    public float Density(float altitude, bool relative = false) // returns the density at altitude in kg/m^3 (default), or relative.
    {
        float ret = Pressure(altitude) / (0.287058f * Temperature(altitude));
        if (relative)
            ret *= densityConstant;
        return ret;
    }

    public float Temperature(float altitude)
    {
        float alt = altitude - seaLevel; // true altitude, if sea level is defined as something other than 0 (perhaps -10000, @ scale = 1)
        if (alt * scale < tropopauseAltitude - seaLevel)
            return temperatureASL - alt * tempLapseAtTropo * scale;
        else // mesopause coming soon!
            return tropopauseTemp;
    }

    public float SpeedOfSound(float altitude)
    {
        return Mathf.Sqrt(Temperature(altitude)) * MACH_COEFF;
    }

    public float Mach(float altitude, float speed) // a little bit expensive - use with caution
    {
        return speed / SpeedOfSound(altitude);
    }

    public float Atm2kPa(float pressure)
    {
        return pressure * pressureASL;
    }

    public float KPa2atm(float pressure)
    {
        return pressure / pressureASL;
    }
}
