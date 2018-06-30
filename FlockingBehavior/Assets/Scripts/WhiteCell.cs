using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteCell : Boid
{
	public override void Init()
	{
		maxSpeed = 8.0f;
		furthestToChase = 4.0f;
		base.Init();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Virus virusComponent = collision.gameObject.GetComponent<Virus>();

		if(virusComponent != null)
		{
			virusComponent.Die();
		}
		
	}


	/// <summary>
	/// Returns a force that is directed toward the closest boid in boids
	/// </summary>
	/// <param name="boids">A list of boids to chase</param>
	/// <returns>A force in the direction of the closest boid</returns>
	protected override Vector3 Chase(List<Boid> boids)
	{
		Vector3 closestTarget = this.position;
		// Some arbitrary distance, far away
		float closestSqrDist = Mathf.Pow(furthestToChase, 2);

		foreach (Boid boid in boids)
		{
			Virus virusCast = boid as Virus;
			if (virusCast != null && virusCast.IsAlive && Vector3.SqrMagnitude(this.position - virusCast.position) < closestSqrDist)
			{
				closestSqrDist = Vector3.SqrMagnitude(this.position - virusCast.position);
				closestTarget = virusCast.position;
			}
		}
		return Seek(closestTarget);
	}
}
