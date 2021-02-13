using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Wing
{
    public bool symmetrical { get { return Symmetrical; } }
    public bool pitchControl { get { return PitchIncidence; } set { PitchIncidence = value; YawIncidence = false; RollIncidence = false; } }
    public bool elevators { get { return Elevators; } set { Elevators = value; } }
    public bool yawControl { get { return YawIncidence; } set { PitchIncidence = false; YawIncidence = value; RollIncidence = false; } }
    public bool rudder { get { return Rudder; } set { Rudder = value; } }
    public bool rollControl { get { return RollIncidence; } set { PitchIncidence = false; YawIncidence = false; RollIncidence = value; } }
    public bool alerons { get { return Alerons; } set { Alerons = value; } }
    public float controlAmount { get { return Control; } set { Control = value; } }
    public Vector3 position { get { return Position; } }
    public float dihedral { get { return Dihedral; } }
    public float sweep { get { return Sweep; } }
    public float span { get { return Span; } }
    public Airfoil activeAirfoil { get { return ActiveAirfoil; } }
    public float area { get { return WingArea; } }
    public float aspectRatio { get { return AspectRatio; } }
    public Vector3 aerodynamicCenter { get { return Col; } }
    public float center { get { return 1 / (1 + Center); } }
    public Vector3 control { get; set; } // x: pitch; y: yaw; z: roll
    public Vector3 angularResistance { get { return AngularResistance; } }

    [SerializeField]
    private bool Symmetrical = true; // Is it on both sides? (usually true, false for single vert stabs)
    //[SerializeField]
    //private bool OnLeft = true;
    [SerializeField]
    private bool PitchIncidence; // true if the whole wing moves, false otherwise.
    [SerializeField]
    private bool Elevators;
    [SerializeField]
    private bool YawIncidence;
    [SerializeField]
    private bool Rudder;
    //[SerializeField]
    //private float YawControl;
    [SerializeField]
    private bool RollIncidence;
    [SerializeField]
    private bool Alerons;
    //[SerializeField]
    //private float RollControl;
    [SerializeField]
    private float Control; // how much incidence to simulate, if *Incidence is true. Otherwise, this is lift to add * 10.
    [SerializeField]
    private Vector3 Position; // position of Aerodynamic Center when static.
    //[SerializeField]
    //private float StaticAC = 0.25f; // position of Aerodynamic Center when static, in percentage of wing cord (only used for subsonic flight)
    [SerializeField]
    private float Dihedral; // in degrees; usually: -5 to 5 for main wings, 0 for horiz stabs, 90 for most vert stabs, 60 for vert stabs on stealth planes
    [SerializeField]
    private float RootCord;
    [SerializeField]
    private float TipCord;
    [SerializeField]
    private Airfoil RootAirfoil;
    [SerializeField]
    private Airfoil TipAirfoil;
    [SerializeField]
    private float Sweep;
    [SerializeField]
    private float Span;
    private Airfoil ActiveAirfoil;
    private float WingArea;
    private float AspectRatio;
    private Vector3 Col; // Center of Lift (AC)
    private float Center; // 1 / (1 + Center) = % down the length of the wing where the Col is.
    private float PitchInstability;
    private Vector3 AngularResistance;

    public Wing(float rootCord, float tipCord, Airfoil rootAirfoil, Airfoil tipAirfoil, float sweep, float length)
    {
        RootCord = rootCord;
        TipCord = tipCord;
        RootAirfoil = rootAirfoil;
        TipAirfoil = tipAirfoil;
        Sweep = sweep;
        Span = length;
        calculate();
    }

    public void calculate()
    {
        WingArea = Span * (TipCord + (RootCord - TipCord) / 2f);
        AspectRatio = Span * Span / WingArea;
        Col = new Vector3(Span * Mathf.Cos(Dihedral * Mathf.Deg2Rad) * center + Position.x,
                          Span * Mathf.Sin(Dihedral * Mathf.Deg2Rad) * center + Position.y,
                          Position.z);
        AngularResistance = new Vector3(Col.z * Mathf.Cos(Dihedral * Mathf.Deg2Rad),
                                        -Col.z * Mathf.Abs(Mathf.Sin(Dihedral * Mathf.Deg2Rad)),
                                        -Mathf.Sqrt(Col.x * Col.x + Col.y * Col.y));
        float wingSlope = (TipCord - RootCord) / Span;
        float rootFactor = Span * (Span * (wingSlope * 5f / 6f) + RootCord * 3f / 2f);
        float tipFactor = Span * (wingSlope * Span / 3f + RootCord / 2f);
        Center = rootFactor / tipFactor;
        //float liftCoef = (RootAirfoil.getLiftCoef() * rootFactor + TipAirfoil.getLiftCoef() * tipFactor) / (rootFactor + tipFactor);
        float liftLevel = (RootAirfoil.getLiftLevel() * rootFactor + TipAirfoil.getLiftLevel() * tipFactor) / (rootFactor + tipFactor);
        float liftSlope = (RootAirfoil.getLiftSlope() * rootFactor + TipAirfoil.getLiftSlope() * tipFactor) / (rootFactor + tipFactor);
        float stallAngle = (RootAirfoil.getStallAngle() * rootFactor + TipAirfoil.getStallAngle() * tipFactor) / (rootFactor + tipFactor);
        float momentLevel = (RootAirfoil.getMomentLevel() * rootFactor + TipAirfoil.getMomentLevel() * tipFactor) / (rootFactor + tipFactor);
        float momentSlope = (RootAirfoil.getMomentSlope() * rootFactor + TipAirfoil.getMomentSlope() * tipFactor) / (rootFactor + tipFactor);
        float dragLevel = (RootAirfoil.getDragLevel() * rootFactor + TipAirfoil.getDragLevel() * tipFactor) / (rootFactor + tipFactor);
        //MonoBehaviour.print(liftLevel);
        ActiveAirfoil = new Airfoil(liftLevel * (1 - Mathf.Exp(-AspectRatio)), liftSlope * (1 - Mathf.Exp(-AspectRatio)), stallAngle * (1 / AspectRatio + 1), momentLevel, momentSlope, dragLevel);
        MonoBehaviour.print(ActiveAirfoil.ToString() + " Wing Area: " + WingArea + " Aspect Ratio: "+ AspectRatio);
    }

    public Airfoil getActiveAirfoil() { return ActiveAirfoil; }

    /*public Vector3 getActiveAC(float speed) // depricated; use aerodynamicCenter instead.
    {
        float cord = (RootCord - TipCord) * Center / 2 + TipCord;
        float setBack = Mathf.Tan(Sweep) * Span / (Center * 2);
        Vector3 ret;
        return Vector3.zero;
    }*/

    public float getLift(float angle, float speed) // depricated
    {
        return ActiveAirfoil.getLift(angle) * WingArea;
    }

    public Forces getForces(Vector3 velocity, Vector3 angularVelocity) // lift, drag, moment, angular forces
    {                                                                  // Only use with extreme caution -- generating flight model is recommended instead
        Vector2 verticalVel;
        Vector2 horizVel;
        Forces forces = new Forces();
        Vector3 dragForce = -velocity.normalized;
        if (Dihedral != 0) // find vertical velocity (z, y) and horizontal velocity (z, x) relative to the wing
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
                horizVel = new Vector2(velocity.z, Mathf.Cos(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
            }
            else
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI * 0.5f + Dihedral * Mathf.Deg2Rad) * velocity.y);
                horizVel = new Vector2(velocity.z, 0f);
            }
        }
        else
        {
            verticalVel = new Vector2(velocity.z, velocity.y);
            horizVel = new Vector2(velocity.z, velocity.x);
        }
        float AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
        //AoA += control.x * PitchControl * Mathf.Sign(Position.z) - control.y * YawControl + control.z * RollControl; // control input
        float AoAcoef = WingArea * verticalVel.sqrMagnitude * 0.001f;
        float lift = ActiveAirfoil.getLift(AoA) * AoAcoef;
        float drag = ActiveAirfoil.getDrag(AoA) * AoAcoef;
        float moment = ActiveAirfoil.getMoment(AoA) * AoAcoef;
        //forces.angularForces.z = (Mathf.Sin(Dihedral * Mathf.Deg2Rad) * moment - angularVelocity.z);
        //forces.angularForces.x = (-Mathf.Cos(Dihedral * Mathf.Deg2Rad) * moment - angularVelocity.x);
        forces.linearForces.x = Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
        forces.linearForces.y = Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
        forces.linearForces += dragForce * drag + dragForce * Mathf.Sin((Sweep + Vector2.Angle(Vector2.right, horizVel) * Mathf.Sign(horizVel.y)) * Mathf.Deg2Rad) * ActiveAirfoil.getDragLevel();
        forces.angularForces.x += forces.linearForces.y * position.z * 500;
        forces.angularForces.y += forces.linearForces.x * position.z * 500;
        Vector2 lateralForces = new Vector2(forces.linearForces.x, forces.linearForces.y);
        if (dihedral != 0)
            forces.angularForces.z += lateralForces.magnitude * Mathf.Sign(dihedral) * Mathf.Sign(lateralForces.x) * span * 500f;
        else
            forces.angularForces.z += lateralForces.y * span * 500f;
        //MonoBehaviour.print("Lift: " + lift.ToString());
        if (!Symmetrical) return forces; // if not symmetrical, just return now.
        if (Dihedral != 0)
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) - Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
                horizVel = new Vector2(velocity.z, Mathf.Cos(Mathf.Atan(velocity.y / velocity.x) - Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
            }
            else
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI / 2 - Dihedral * Mathf.Deg2Rad));
                horizVel = new Vector2(velocity.z, 0f);
            }
        }
        else
        {
            verticalVel = new Vector2(velocity.z, velocity.y);
            horizVel = new Vector2(velocity.z, velocity.x);
        }
        AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
        //AoA += control.x * PitchControl * Mathf.Sign(Position.z) + control.y * YawControl - control.z * RollControl; // control input
        AoAcoef = WingArea * verticalVel.sqrMagnitude * 0.001f;
        lift = ActiveAirfoil.getLift(AoA) * AoAcoef;
        drag = ActiveAirfoil.getDrag(AoA) * AoAcoef;
        moment = ActiveAirfoil.getMoment(AoA) * AoAcoef;
        Vector3 linearForces2 = new Vector3(); // we need to isolate the effects of each individual wing.
        Vector3 angularForces2 = new Vector3();
        //angularForces2.z += (Mathf.Sin(Dihedral * Mathf.Deg2Rad) * moment - angularVelocity.z);
        //angularForces2.x += (-Mathf.Cos(Dihedral * Mathf.Deg2Rad) * moment - angularVelocity.x);
        linearForces2.x += Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
        linearForces2.y += Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
        linearForces2 += dragForce * drag + dragForce * Mathf.Sin((Sweep - Vector2.Angle(Vector2.right, horizVel) * Mathf.Sign(horizVel.y)) * Mathf.Deg2Rad) * ActiveAirfoil.getDragLevel();
        angularForces2.x += linearForces2.y * position.z * 500;
        angularForces2.y += linearForces2.x * position.z * 500;
        lateralForces = new Vector2(linearForces2.x, linearForces2.y);
        if (dihedral != 0)
            angularForces2.z -= lateralForces.magnitude * Mathf.Sign(dihedral) * Mathf.Sign(lateralForces.x) * span * 500f;
        else
            angularForces2.z -= lateralForces.y * span * 500f;
        forces.linearForces += linearForces2;
        forces.angularForces += angularForces2;
        return forces;

    }

    public Vector3 getLinearForces(Vector3 velocity) // lift AND drag
    {
        Vector2 verticalVel;
        Vector2 horizVel;
        Vector3 forces = new Vector3();
        Vector3 dragForce = -velocity.normalized;
        if (Dihedral != 0) // find vertical velocity (z, y) and horizontal velocity (z, x) relative to the wing
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
                horizVel = new Vector2(velocity.z, Mathf.Cos(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
            }
            else
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI * 0.5f + Dihedral * Mathf.Deg2Rad) * velocity.y);
                horizVel = new Vector2(velocity.z, 0f);
            }
        }
        else
        {
            verticalVel = new Vector2(velocity.z, velocity.y);
            horizVel = new Vector2(velocity.z, velocity.x);
        }
        //MonoBehaviour.print("VerticalVel: " + verticalVel.ToString());
        float AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
        float AoAcoef = WingArea * verticalVel.sqrMagnitude * 0.002f;
        float lift = ActiveAirfoil.getLift(AoA) * AoAcoef;
        float drag = ActiveAirfoil.getDrag(AoA) * AoAcoef;
        forces.x = Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
        forces.y = Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
        forces += dragForce * drag + dragForce * Mathf.Sin((Sweep + Vector2.Angle(Vector2.right, horizVel) * Mathf.Sign(horizVel.y)) * Mathf.Deg2Rad) * ActiveAirfoil.getDragLevel();
        //MonoBehaviour.print("Lift: " + lift.ToString());
        if (!Symmetrical) return forces; // if not symmetrical, just return now.
        if (Dihedral != 0)
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) - Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
                horizVel = new Vector2(velocity.z, Mathf.Cos(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
            }
            else
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI / 2 - Dihedral * Mathf.Deg2Rad));
                horizVel = new Vector2(velocity.z, 0f);
            }
            AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
            AoAcoef = WingArea * verticalVel.sqrMagnitude * 0.002f;
            lift = ActiveAirfoil.getLift(AoA) * AoAcoef;
            drag = ActiveAirfoil.getDrag(AoA) * AoAcoef;
            forces.x += Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
            forces.y += Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
            forces += dragForce * drag + dragForce * Mathf.Sin((Sweep - Vector2.Angle(Vector2.right, horizVel) * Mathf.Sign(horizVel.y)) * Mathf.Deg2Rad) * ActiveAirfoil.getDragLevel();
            return forces;
        }
        else return forces * 2; // if dihedral is 0, the calculation is the same for both sides.
    }

    public Vector3 getAngularForces(Vector3 velocity) // depricated?
    {
        Vector2 verticalVel;
        Vector2 horizVel;
        Vector3 forces = new Vector3();
        if (Dihedral != 0)
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
                horizVel = new Vector2(velocity.z, Mathf.Cos(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
            }
            else
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI / 2 + Dihedral * Mathf.Deg2Rad) * velocity.y);
        }
        else
        {
            verticalVel = new Vector2(velocity.z, velocity.y);
        }
        //MonoBehaviour.print("VerticalVel: " + verticalVel.ToString());
        float AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
        float lift = ActiveAirfoil.getMoment(AoA) * WingArea * verticalVel.sqrMagnitude * 0.002f;
        forces.z = Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
        forces.x = -Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
        //MonoBehaviour.print("Lift: " + lift.ToString());
        if (!Symmetrical) return forces; // if not symmetrical, just return now.
        if (Dihedral != 0)
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) - Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
            }
            else
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI / 2 - Dihedral * Mathf.Deg2Rad));
            AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
            lift = ActiveAirfoil.getMoment(AoA) * WingArea * velocity.sqrMagnitude * 0.002f;
            forces.z += Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
            forces.x += -Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
            return forces;
        }
        else return forces * 2; // if dihedral is 0, the calculation is the same for both sides.
    }

    public float getDrag(Vector3 velocity) // depricated
    {
        Vector2 verticalVel;
        //Vector3 forces = new Vector3();
        if (Dihedral != 0)
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) + Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
            }
            else
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI / 2 + Dihedral * Mathf.Deg2Rad) * velocity.y);
        }
        else
        {
            verticalVel = new Vector2(velocity.z, velocity.y);
        }
        //MonoBehaviour.print("VerticalVel: " + verticalVel.ToString());
        float AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
        float drag = ActiveAirfoil.getDrag(AoA) * WingArea * verticalVel.sqrMagnitude * 0.002f;
        //forces.x = Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
        //forces.y = Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
        //MonoBehaviour.print("Lift: " + lift.ToString());
        if (!Symmetrical) return drag; // if not symmetrical, just return now.
        if (Dihedral != 0)
        {
            if (velocity.x != 0)
            {
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.Atan(velocity.y / velocity.x) - Dihedral * Mathf.Deg2Rad) * new Vector2(velocity.x, velocity.y).magnitude);
                if (velocity.x < 0) verticalVel.y = -verticalVel.y;
            }
            else
                verticalVel = new Vector2(velocity.z, Mathf.Sin(Mathf.PI / 2 - Dihedral * Mathf.Deg2Rad));
            AoA = Vector2.Angle(Vector2.right, verticalVel) * -Mathf.Sign(verticalVel.y); // AoA in degrees
            drag += ActiveAirfoil.getDrag(AoA) * WingArea * velocity.sqrMagnitude * 0.002f;
            //forces.x += Mathf.Sin(Dihedral * Mathf.Deg2Rad) * lift;
            //forces.y += Mathf.Cos(Dihedral * Mathf.Deg2Rad) * lift;
            return drag;
        }
        else return drag * 2; // if dihedral is 0, the calculation is the same for both sides.
    }
}

[Serializable]
public struct Airfoil
{
    const float LIFTSLOPE = Mathf.PI * Mathf.PI / 90f; // in units per degree; approx. 0.11
    const float DRAGSLOPE = 1.25f;
    //[SerializeField]
    //private float liftCoef; // depricated
    [SerializeField]
    private float liftLevel;
    [SerializeField]
    private float liftSlope;
    [SerializeField]
    private float stallAngle; // in DEGREEEEEEEEEEEEEEEEEEEEES
    [SerializeField]
    private float momentLevel;
    [SerializeField]
    private float momentSlope;
    [SerializeField]
    private float dragLevel;
    private float angleConversionRate;
    private float fixCoef;

    public Airfoil(float liftLevel, float liftSlope, float stallAngle, float momentLevel, float momentSlope, float dragLevel)
    {
        //this.liftCoef = liftCoef;
        this.liftLevel = liftLevel;
        this.liftSlope = liftSlope;
        this.stallAngle = stallAngle;
        this.momentLevel = momentLevel;
        this.momentSlope = momentSlope;
        this.dragLevel = dragLevel;
        angleConversionRate = Mathf.PI / (2 * stallAngle);
        fixCoef = 2 * liftSlope * stallAngle / Mathf.PI;
        //MonoBehaviour.print(angleConversionRate);
    }

    //public float getLiftCoef() // vestigial
    //{
    //    return liftCoef;
    //}

    public float getLiftLevel()
    {
        return liftLevel;
    }

    public float getLiftSlope()
    {
        return liftSlope;
    }

    public float getStallAngle()
    {
        return stallAngle;
    }

    public float getMomentLevel()
    {
        return momentLevel;
    }

    public float getMomentSlope()
    {
        return momentSlope;
    }

    public float getDragLevel()
    {
        return dragLevel;
    }

    public float getLift(float angle) // angle in degrees
    {
        if (angleConversionRate == 0)
        {
            angleConversionRate = Mathf.PI / (2 * stallAngle);
            fixCoef = 2 * liftSlope * stallAngle / Mathf.PI;
            //MonoBehaviour.print(angleConversionRate);
        }
        float ret = 0;
        float normalFlight = Mathf.Sin(angle * angleConversionRate) * fixCoef;
        //MonoBehaviour.print("normalFlight = " + normalFlight);
        float stalling = Mathf.Sin(angle * (Mathf.Deg2Rad + Mathf.Deg2Rad));
        if (angle >= 0)
        {
            if (angle < stallAngle + stallAngle)
                ret = Mathf.Max(normalFlight, stalling) + liftLevel;
            else
                ret = stalling + liftLevel;
        }
        else
        {
            if (angle > -stallAngle - stallAngle)
                ret = Mathf.Min(normalFlight, stalling) + liftLevel;
            else
                ret = stalling + liftLevel;
        }
        //MonoBehaviour.print("Cl: " + ret);
        return ret;
    }

    public float getMoment(float angle) // angle in degrees
    {
        return momentSlope * Mathf.Sin(angle * Mathf.Deg2Rad) + momentLevel;
    }

    public float getDrag(float angle)
    {
        float sinSqr = Mathf.Sin(angle * Mathf.Deg2Rad); // we have to square the sin, so we only want to calculate the sin once.
        return dragLevel + DRAGSLOPE * sinSqr * sinSqr;
        //return (-Mathf.Cos(angle * (Mathf.Deg2Rad + Mathf.Deg2Rad)) + 1) * 0.5f;
    }

    public override string ToString()
    {
        string ret = "Lift Intercept: " + liftLevel;
        ret += " Stall Angle: " + stallAngle;
        ret += " Moment Intercept: " + momentLevel;
        return ret += " Moment Slope: " + momentSlope;
    }
}

public struct Forces
{
    public Vector3 linearForces;
    public Vector3 angularForces;
}
