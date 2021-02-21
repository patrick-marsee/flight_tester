using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILiftingBody {

    Vector3 velocity { get; set; } // meters per second
    Vector3 angularVelocity { get; set; } // degrees per second
    Vector3 indicatedVelocity { get; }
    Vector3 acceleration { get; set; }
    Vector3 angularAcceleration { get; set; }
    Vector3 geeForce { get; }
    float AoA { get; } // radians
    float sideslip { get; }
    float mass { get; set; }
    float dragC { get; set; } // drag coefficient, for the benefit of AI pilots.
    float tas { get; } // true air speed
    float ias { get; } // indicated airspeed
    bool isControlable { get; set; }
    bool isLanded { get; set; }
    bool braking { get; set; }
    
    // Callbacks that allow other objects to know what the final value of control
    // settings are. These should not allow their respective values to change
    // to avoid what amounts to data races.
    delegate void ControlSet(float aPitch);
    
    delegate void FlightEvent();
    
    void ConnectPitchSet(ControlSet aCallback);
    
    void ConnectYawSet(ControlSet aCallback);
    
    void ConnectRollSet(ControlSet aCallback);
    
    void ConnectThrustSet(ControlSet aCallback);
    
    void ConnectLandEvent(FlightEvent aCallback);
    
    void ConnectTakeoffEvent(FlightEvent aCallback);
    
    void ConnectCrashEvent(FlightEvent aCallback);
    
    // The values passed to these functions should be added to the internal values.
    void AdjustPitch(float aPitch);
    
    void AdjustYaw(float aYaw);
    
    void AdjustRoll(float aRoll);
    
    void AdjustThrust(float aThrust);

    void SetPosition(Vector3 pos, Quaternion rot, float speed);
}
