/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class ANetworkLiftingBody : ALiftingBody {

    protected struct liftingBodyPacket
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 vel, angVel, accel;
        public float pitch, yaw, roll, thrust;

        public liftingBodyPacket(Transform transform, ANetworkLiftingBody nlBody)
        {
            pos = transform.position;
            rot = transform.rotation;
            vel = nlBody.velocity;
            angVel = nlBody.angularVelocity;
            accel = nlBody.acceleration;
            pitch = nlBody.pitch;
            yaw = nlBody.yaw;
            roll = nlBody.roll;
            thrust = nlBody.thrust;
        }
    }

    [SerializeField]
    protected float refreshRate = 9;
    protected float timeSinceLastRefresh = 0;

    // Use this for initialization
    protected virtual void Start()
    {
        ALiftingBody::Start();
        if (!hasAuthority)
            isControlable = false;
        else
            isControlable = true;
    }

    protected virtual void Update()
    {
        if (hasAuthority)
        {
            timeSinceLastRefresh += Time.deltaTime;
            if (timeSinceLastRefresh > 1 / refreshRate)
            {
                timeSinceLastRefresh -= 1 / refreshRate;
                if (isClient)
                    RpcSyncLB(new liftingBodyPacket(transform, this));
                else if (isServer)
                    CmdSyncLB(new liftingBodyPacket(transform, this));

            }
        }
    }

    [Command]
    protected void CmdSyncLB(liftingBodyPacket lbp)
    {
        if (!isLocalPlayer)
        {
            transform.position = lbp.pos;
            transform.rotation = lbp.rot;
            velocity = lbp.vel;
            angularVelocity = lbp.vel;
            acceleration = lbp.accel;
            pitch = lbp.pitch;
            yaw = lbp.yaw;
            roll = lbp.roll;
            thrust = lbp.thrust;
        }
    }

    [ClientRpc]
    protected void RpcSyncLB(liftingBodyPacket lbp)
    {
        transform.position = lbp.pos;
        transform.rotation = lbp.rot;
        velocity = lbp.vel;
        angularVelocity = lbp.vel;
        acceleration = lbp.accel;
        pitch = lbp.pitch;
        yaw = lbp.yaw;
        roll = lbp.roll;
        thrust = lbp.thrust;
        CmdSyncLB(lbp);
    }
}*/
