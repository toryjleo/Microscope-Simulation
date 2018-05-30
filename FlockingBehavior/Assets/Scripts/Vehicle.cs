using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Vehicle is a class that is attached to a gameobject that implements various autonomous agent properties
public class Vehicle : MonoBehaviour {

	#region Consts

	/// <summary>
	/// How far to go fromt he center of the screen before wrapping
	/// </summary>
	private const float  WIDTH_TO_WRAP = 11.25f;
	private const float HEIGHT_TO_WRAP = 6.25f;

	public const float MAX_SPEED = 4.0f;
	public const float MAX_FORCE = 0.1f;
	public const float MASS = 1.0f;

	#endregion

	/// <summary>
	/// Bounds the vehicle cannot exceed in the world space
	/// </summary>
	private float northBounds;
	private float southBounds;
	private float eastBounds;
	private float westBounds;

	/// <summary>
	/// A vehicle's desired distance from its neighbor
	/// </summary>
	private float desiredSeperation;

	/// <summary>
	/// The max distance for another vehicle to be considered a 'neighbor'
	/// </summary>
	private float furthestNeighbor;


	private float spriteHeight;

	/// <summary>
	/// Variables for tracking physics related stuff
	/// </summary>
	private Vector3 acceleration;
	private Vector3 velocity;
	public Vector3 position;

	/// <summary>
	/// Materials used for debug lines (Not currently relevant)
	/// </summary>
	public Material forwardVectorMaterial;
	public Material rightVectorMaterial;
	public Material targetVectorMaterial;

	/// <summary>
	/// Used to initialize the member variables of this vehicle
	/// </summary>
	public virtual void Init()
	{
		Vector2 middleScreen = Camera.main.transform.position;

		northBounds = middleScreen.y + HEIGHT_TO_WRAP;
		southBounds = middleScreen.y - HEIGHT_TO_WRAP;
		eastBounds = middleScreen.x + WIDTH_TO_WRAP;
		westBounds = middleScreen.x - WIDTH_TO_WRAP;


		SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
		spriteHeight = sprite.size.y;
		acceleration = Vector3.zero;
		velocity =  new Vector3(0, 0, 0);
		position = this.transform.position;

		desiredSeperation = 1.25f * spriteHeight;
		furthestNeighbor = 10;
	}


	/// <summary>
	/// Apply a force to this object this frame, taking into account its mass
	/// </summary>
	/// <param name="force">The overall force vector</param>
	public void ApplyForce(Vector3 force)
	{
		this.acceleration += force / MASS;
	}


	/// <summary>
	/// Limit the magnitude of the force variable to have a maximum magnitude of maxForce
	/// </summary>
	/// <param name="force">Force to check to change</param>
	/// <param name="maxForce">The maximum magnitde force should have</param>
	/// <returns>A force with a the same direction but a maximum magnitude of maxForce</returns>
	private Vector3 LimitForce(Vector3 force, float maxForce)
	{
		if (force.sqrMagnitude > Mathf.Pow(maxForce, 2))
		{
			force.Normalize();
			force *= maxForce;
		}
		return force;
	}

	/// <summary>
	/// Internally handles the overall movement of the vehicle object
	/// </summary>
	public void FinalizeMovement()
	{
		// Update acceleration
		velocity += acceleration * Time.deltaTime;

		// If the velocity is greater than maxSpeed, set the magnitude = maxSpeed
		if( Vector3.SqrMagnitude(velocity) > Mathf.Pow(MAX_SPEED, 2)) {
			velocity.Normalize();
			velocity *= MAX_SPEED;
		}

		// Rotate the transform so that the sprite is facing the direction of the velocity
		Vector2 dir = new Vector2(velocity.x, velocity.y);
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		
		// Update position
		position += velocity * Time.deltaTime;
		position = ClampPositionInBounds(position);
		this.transform.position = position;

		// Clear the acceleration for the next frame
		acceleration = Vector3.zero;
	}


	public void CallFlock(List<Vehicle> vehicles, float seperateMultiplier, float alignMultiplier, float cohesionMultiplier)
	{
		Vector3 seperateForce = Seperate(vehicles);
		Vector3 alignForce = Align(vehicles);
		Vector3 cohesionForce = Cohesion(vehicles);

		seperateForce *= seperateMultiplier;
		alignForce *= alignMultiplier;
		cohesionForce *= cohesionMultiplier;

		ApplyForce(seperateForce);
		ApplyForce(alignForce);
		ApplyForce(cohesionForce);
		
	}


	/// <summary>
	/// Kind of a crude solution, but get the vehicle to show up on the opposite side if it goes out of bounds
	/// </summary>
	/// <param name="position">The current position of the vehicle</param>
	/// <returns>A position within the specified bounds</returns>
	private Vector3 ClampPositionInBounds(Vector3 position)
	{
		if (position.y > northBounds)
		{
			position.y = southBounds + (position.y - northBounds);
		}
		else if (position.y < southBounds)
		{
			position.y = northBounds - (position.y - southBounds);
		}

		if (position.x > eastBounds)
		{
			position.x = westBounds + (position.x - eastBounds);
		}
		else if (position.x < westBounds)
		{
			position.x = eastBounds - (position.x - westBounds);
		}

		return position;
	}


	/// <summary>
	/// Calculate a steering force such that it allows us to seek a target position
	/// </summary>
	/// <param name="targetPosition">Where to seek</param>
	/// <returns>The steering force to get to the target</returns>
	public Vector3 Seek(Vector3 targetPosition)
	{
		Vector3 desiredVelocity = targetPosition - this.position;
		desiredVelocity.Normalize();
		desiredVelocity *= MAX_SPEED;
		Vector3 steeringForce = desiredVelocity - this.velocity;
		// Limit force
		LimitForce(steeringForce, MAX_FORCE);
		return steeringForce;
	}


	/// <summary>
	/// Get a force that moves this vehicle away from all the other vehicles
	/// </summary>
	/// <param name="vehicles">A list of vehicles to move away from</param>
	/// <returns>A force that is the average vector that pushes this vehicle away from all the other vehicles</returns>
	public Vector3 Seperate(List<Vehicle> vehicles)
	{
		int count = 0;
		Vector3 sum = Vector3.zero;
		foreach (Vehicle vehicle in vehicles)
		{
			if (vehicle == this)
			{
				continue;
			}
			else
			{
				float dist = Vector3.Distance(this.position, vehicle.position);
				if (dist < desiredSeperation)
				{
					Vector3 dir = this.position - vehicle.position;
					dir.Normalize();
					sum += dir;
					count++;
				}
			}
		}
		if (count > 0)
		{
			sum = sum / count;
		}
		sum *= MAX_SPEED;
		Vector3 steeringForce = sum - velocity;
		LimitForce(steeringForce, MAX_FORCE);

		return steeringForce;
	}


	/// <summary>
	/// Get a force that moves this vehicle in the same direction as all the other vehicles
	/// </summary>
	/// <param name="vehicles">A list of vehicles to try to move in the same direction as</param>
	/// <returns>A force that is the average vector of all the other vehicles' velocities</returns>
	public Vector3 Align(List<Vehicle> vehicles)
	{
		int count = 0;
		Vector3 sum = Vector3.zero;
		foreach (Vehicle vehicle in vehicles)
		{
			if (vehicle == this)
			{
				continue;
			}
			else
			{
				float dist = Vector3.Distance(this.position, vehicle.position);
				if (dist < furthestNeighbor)
				{
					sum += vehicle.velocity;
					count++;
				}
			}
		}
		if (count > 0)
		{
			sum /= count;
			sum *= MAX_SPEED;

			Vector3 steeringForce = sum - velocity;
			LimitForce(steeringForce, MAX_FORCE);
			return steeringForce;
		}
		else
		{
			return Vector3.zero;
		}
	}


	/// <summary>
	/// Get a force that moves this vehicle towards all the other vehicles
	/// </summary>
	/// <param name="vehicles">A list of vehicles to try to move towards</param>
	/// <returns>A force that is the average vector that pushes this vehicle towards all the other vehicles</returns>
	public Vector3 Cohesion(List<Vehicle> vehicles)
	{
		int count = 0;
		Vector3 sum = Vector3.zero;
		foreach (Vehicle vehicle in vehicles)
		{
			if (vehicle == this)
			{
				continue;
			}
			else
			{
				float dist = Vector3.Distance(this.position, vehicle.position);
				if (dist < furthestNeighbor)
				{
					sum += vehicle.position;
					count++;
				}
			}
		}
		if (count > 0)
		{
			sum /= count;
			return Seek(sum);
		}
		else
		{
			return Vector3.zero;
		}
	}
}
