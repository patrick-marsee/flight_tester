using UnityEngine;
using System.Collections;

public class PowerPlant : MonoBehaviour {

    [SerializeField]
    protected float maxThrust; // in Newtons
    protected LiftingBody plane;
    protected Atmosphere atmo;

    public float throttle { get; set; }

	// Use this for initialization
	void Start () {
        plane = GetComponent<LiftingBody>();
        atmo = FindObjectOfType<Atmosphere>();
	}
	
	// Update is called once per frame
	void Update () {
        plane.thrust = throttle * maxThrust * 0.5f + maxThrust * 0.5f;
	}
}
