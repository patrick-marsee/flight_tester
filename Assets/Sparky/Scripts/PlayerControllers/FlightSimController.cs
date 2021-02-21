using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightSimController : APlayerController {
    
    const float TRIM_INTENSITY = 1.0f / 1024.0f;

    private bool startAutoTrim()
    {
            //if (Mathf.Abs(pitch) < 0.1f) print("pitch == " + pitch);
            //if (Mathf.Abs(yaw) < 0.1f) print("yaw == " + yaw);
            //if (Mathf.Abs(roll) < 0.1f) print("roll == " + roll);
            //if (autoTrimIsOn == true) print("autoTrimIsOn == true");
            return !stopAutoTrim() && !autoTrimIsOn;
    }

    private bool stopAutoTrim()
    {
            return (Input.GetAxis(pitchAxis) != 0.0f || Input.GetAxis(yawAxis) != 0.0f || Input.GetAxis(rollAxis) != 0.0f);
    }

    //[SerializeField]
    private bool autoTrim = false;

    //private float pitch, yaw, roll;
    private Vector3 autoTrimAngle;
    private bool autoTrimIsOn = false;
    private Vector3 prevAngVel;
    private Vector3 trim;
	
	// Update is called once per frame
	void FixedUpdate()
	{
        roll = -Input.GetAxis(rollAxis);
        pitch = Input.GetAxis(pitchAxis);
        yaw = Input.GetAxis(yawAxis);
        throttle = Input.GetAxis(throttleAxis) * 0.5f + 0.5f;
        
        if (aoaLimit != 0f)
        {
            float aoa = lBody.AoA * Mathf.Rad2Deg; // aoa = AoA in degrees
            if (aoa > aoaLimit - aoaMargin && aoa < 90.0f)
                pitch = Mathf.Max(pitch, Mathf.Clamp((aoa - aoaLimit) / aoaMargin, -1f, 1f));

            else if (aoa < aoaMargin - aoaLimit && aoa > -90.0f)
                pitch = Mathf.Min(pitch, Mathf.Clamp((aoaLimit + aoa) / aoaMargin, -1f, 1f));
        }
        if (gLimit != 0f)
        {
            float g = lBody.geeForce.y;
            float dynamicGMargin = Mathf.Clamp(gMargin * lBody.ias * 0.1f, 0.1f, gLimit);

            if (g > gLimit - dynamicGMargin)
                pitch = Mathf.Max(pitch, Mathf.Clamp((g - gLimit) / dynamicGMargin, -1f, 1f));
            else if (g < dynamicGMargin - gLimit)
                pitch = Mathf.Min(pitch, Mathf.Clamp((g + gLimit) / dynamicGMargin, -1f, 1f));

        }

        //print("pitch = " + pitch.ToString());
        /*if (lBody.isControlable)
        {
            lBody.AdjustPitch(pitch);
            lBody.AdjustYaw(yaw);
            lBody.AdjustRoll(roll);
            for (int i = 0; i < engine.Length; i++)
                engine[i].SetThrottle(Input.GetAxis(throttleAxis) * 0.5f + 0.5f);
        }*/
        SetControls();

        if (autoTrim)
        {
            if (startAutoTrim())
            {
                prevAngVel = lBody.angularVelocity;
                trim = Vector3.zero;
                autoTrimIsOn = true;
            }
            if (stopAutoTrim())
            {
                autoTrimIsOn = false;
            }
            if (autoTrimIsOn) AutoTrim();
        }
    }

    void AutoTrim()
    {
        Vector3 AngularAccel = (lBody.angularVelocity - prevAngVel) / (Time.deltaTime * 2);
        trim -= (lBody.angularVelocity + AngularAccel) * TRIM_INTENSITY;
        prevAngVel = lBody.angularVelocity;
        lBody.AdjustPitch(trim.x);
        lBody.AdjustYaw(trim.y);
        lBody.AdjustRoll(trim.z);
    }
}
