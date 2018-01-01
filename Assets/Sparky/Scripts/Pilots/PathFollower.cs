using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : APilot {

    [SerializeField]
    private Vector3[] path; // the path that the pilot follows. The path loops.
    [SerializeField]
    private float nodeRadius = 10; // units, not meters.
    [SerializeField]
    private float bankAngle = 15; // the angle that the pilot banks through turns.
    [SerializeField]
    private float speed; // the speed that the pilot will attempt to maintain

	// Use this for initialization
	//void Start()
	//{
		
	//}
	
	// Update is called once per frame
	void Update()
	{
		
	}
}
