using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockArrow : Vehicle
{

	// Use this for initialization
	public override void Start()
	{
		maxSpeed = 5;
		mass = 1;

		acceleration = Vector3.zero;
		velocity = new Vector3(0, 0, 0);
		position = this.transform.position;
		seekWeight = 1;
		fleeWeight = 1;
		wanderRadius = 2;

		distanceAhead = 5;

		persueTimeStep = 1;
		evadeTimeStep = 1;
		wanderTimeStep = 5;
	}
}
