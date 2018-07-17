using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteCell : Boid
{

	#region METHODS

	/// <summary>
	/// Used to initialize member variables of this WhiteCell object
	/// </summary>
	public override void Init()
	{
		base.Init();
		maxSpeed = 6.0f;
		furthestToChase = 4.0f;
	}


	/// <summary>
	/// If this object collides with a Virus object, it will disable the virus object (consume it) by calling its Die()
	/// method
	/// </summary>
	/// <param name="collision"></param>
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
	/// <param name="boids">A list of boids (of the child class Virus) to chase</param>
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

	#endregion

}
