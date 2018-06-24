using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteCell : Vehicle
{
	private float furthestToChase;


	public void CallChase(List<Vehicle> vehicles, float chaseMultiplier)
	{
		Vector3 chaseForce = Chase(vehicles);

		chaseForce *= chaseMultiplier;

		ApplyForce(chaseForce);
	}

	private Vector3 Chase(List<Vehicle> vehicles)
	{

		Vector3 closestTarget = this.position;
		// Some arbitrary distance, far away
		float closestSqrDist = 1000f;

		foreach (Vehicle vehicle in vehicles)
		{

			if (Vector3.SqrMagnitude(this.position - vehicle.position) < closestSqrDist)
			{
				closestSqrDist = Vector3.SqrMagnitude(this.position - vehicle.position);
				closestTarget = vehicle.position;
			}
		}
		return Seek(closestTarget);
	}



}
