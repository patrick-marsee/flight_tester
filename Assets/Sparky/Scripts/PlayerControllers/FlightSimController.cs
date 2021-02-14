using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightSimController : APlayerController {

    private bool startAutoTrim
    {
        get
        {
            //if (Mathf.Abs(pitch) < 0.1f) print("pitch == " + pitch);
            //if (Mathf.Abs(yaw) < 0.1f) print("yaw == " + yaw);
            //if (Mathf.Abs(roll) < 0.1f) print("roll == " + roll);
            //if (autoTrimIsOn == true) print("autoTrimIsOn == true");
            return Mathf.Abs(pitch) <= 0.1f && Mathf.Abs(yaw) <= 0.1f && Mathf.Abs(roll) <= 0.1f && !autoTrimIsOn;
        }
    }

    private bool stopAutoTrim
    {
        get
        {
            return (Input.GetButtonDown(pitchAxis) || Input.GetButtonDown(yawAxis) || Input.GetButtonDown(yawAxis));
        }
    }

    [SerializeField]
    private bool autoTrim = false;

    //private float pitch, yaw, roll;
    private Vector3 autoTrimAngle;
    private bool autoTrimIsOn = false;
	// Use this for initialization
	//void Start()
	//{
		
	//}
	
	// Update is called once per frame
	void Update()
	{
        roll = -Input.GetAxis(rollAxis);
        pitch = Input.GetAxis(pitchAxis);
        yaw = Input.GetAxis(yawAxis);
        //if (!autoTrimIsOn)
        //{
        //    lBody.pitch = pitch;
        //    lBody.yaw = yaw;
        //    lBody.roll = roll;
        //}
        //lBody.roll = -Input.GetAxis(rollAxis);// - relativeRot.z;
        //lBody.pitch = Input.GetAxis(pitchAxis);// - relativeRot.x;
        if (aoaLimit != 0f)
        {
            float aoa = lBody.AoA * Mathf.Rad2Deg; // aoa = AoA in degrees
            //print(aoa);
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

        if (lBody.isControlable)
        {
            lBody.pitch = pitch;
            lBody.yaw = yaw;
            lBody.roll = roll;
            //lBody.yaw = Input.GetAxis(yawAxis);
            for (int i = 0; i < engine.Length; i++)
                engine[i].SetThrottle(Input.GetAxis(throttleAxis) * 0.5f + 0.5f);
        }

        if (autoTrim)
        {
            if (startAutoTrim)
            {
                autoTrimAngle = transform.rotation.eulerAngles;
                autoTrimIsOn = true;
            }
            if (stopAutoTrim)
            {
                autoTrimIsOn = false;
            }
            if (autoTrimIsOn) AutoTrim();
        }
    }

    void AutoTrim()
    {
        print(lBody.pitch);
        lBody.pitch = -lBody.angularVelocity.x * 0.0625f;
        lBody.yaw = -lBody.angularVelocity.y * 0.0625f;
        lBody.roll = lBody.angularVelocity.z * 0.0625f;
    }
}
