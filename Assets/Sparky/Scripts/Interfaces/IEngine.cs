using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEngine {

	float[] throttle { get; set; } // individual engine throttle settings

    void SetThrottle(float throt); // set all engines to this throttle
    float GetThrust(float airSpeed, float airDensity, float temperature); // get thrust at a given airspeed and altitude
    float InverseGetThrust(float airspeed, float airDensity, float temperature, float drag); // literally the inverse of Get Thrust. Returns throttle setting.
    Vector3[] GetPosions(); // get all engine positions
}
