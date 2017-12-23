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
    float pitch { get; set; }
    float yaw { get; set; }
    float roll { get; set; }
    float thrust { get; set; }
    float dragC { get; set; } // drag coefficient, for the benefit of AI pilots.
    float tas { get; } // true air speed
    float ias { get; } // indicated airspeed
    bool isControlable { get; set; }

    // Use this for initialization
    //void Start()
    //{

    //}

    // Update is called once per frame
    //void Update()
    //{

    //}
}
