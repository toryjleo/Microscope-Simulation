using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Vehicle is a class that is attached to a gameobject that implements various autonomous agent properties
public class Vehicle : MonoBehaviour {
	private const float  WIDTH_TO_WRAP = 11.25f;
	private const float HEIGHT_TO_WRAP = 6.25f;


	private float northBounds;
	private float southBounds;
	private float eastBounds;
	private float westBounds;

	// Public variables that we can tweak in Unity
	public float maxSpeed;
	public float maxForce;
	public float desiredSeperation;
	public float neighborDist;
	public float mass;

	// Private variables for tracking physics stuff
	public Vector3 acceleration;
	public Vector3 velocity;
	public Vector3 position;


	// Materials used for debug lines
	public Material forwardVectorMaterial;
	public Material rightVectorMaterial;
	public Material targetVectorMaterial;

	// Use this for initialization
	public virtual void Start ()
	{
		Vector2 middleScreen = Camera.main.transform.position;

		northBounds = middleScreen.y + HEIGHT_TO_WRAP;
		southBounds = middleScreen.y - HEIGHT_TO_WRAP;
		eastBounds = middleScreen.x + WIDTH_TO_WRAP;
		westBounds = middleScreen.x - WIDTH_TO_WRAP;

		acceleration = Vector3.zero;
		velocity =  new Vector3(0, 0, 0);
		position = this.transform.position;

		maxSpeed = 4;
		maxForce = 0.1f;
		desiredSeperation = 1;
		neighborDist = 50;
		mass = 1.0f;


	}
	
	// Update is called once per frame
	public virtual void Update ()
	{
		//Debug.Log(this.transform.position);

		// Last thing: actually move
		Movement();
	}
		

	/// <summary>
	/// Apply a force to this object this frame, taking
	/// into account its mass
	/// </summary>
	/// <param name="force">The overall force vector</param>
	public void ApplyForce(Vector3 force)
	{
		this.acceleration += force / this.mass;
	}


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
	/// Internally handles the overall movement based
	/// on acceleration, velocity, etc.
	/// </summary>
	public void Movement()
	{
		// Apply acceleration

		velocity += acceleration * Time.deltaTime;

		// If the velocity is greater than maxSpeed, set the magnitude = maxSpeed
		if( Vector3.Magnitude(velocity) > maxSpeed) {
			velocity.Normalize();
			velocity *= maxSpeed;
		}

		// rotate the transform
		Vector2 dir = new Vector2(velocity.x, velocity.y);
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		//this.transform.rotation = Quaternion.LookRotation(this.velocity);

		position += velocity * Time.deltaTime;


		position = ClampPositionInBounds(position);
		// Throw the position back to Unity
		this.transform.position = position;

		// We're done with forces for this frame
		acceleration = Vector3.zero;
	}


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
	/// Calculate a steering force such that it
	/// allows us to seek a target position
	/// </summary>
	/// <param name="targetPosition">Where to seek</param>
	/// <returns>The steering force to get to the target</returns>
	public Vector3 Seek(Vector3 targetPosition)
	{
		
		// Calculate our "perfect" desired velocity
		Vector3 desiredVelocity = targetPosition - this.transform.position;

		// Limit desired velocity by max speed
		desiredVelocity.Normalize();
		desiredVelocity *= maxSpeed;

		// How do we turn to start moving towards
		// the desired velocity?
		Vector3 steeringForce = desiredVelocity - this.velocity;

		// Limit force
		LimitForce(steeringForce, maxForce);

		return steeringForce;
	}


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
		sum *= maxSpeed;
		Vector3 steeringForce = sum - velocity;
		LimitForce(steeringForce, maxForce);

		return steeringForce;
	}


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
				if (dist < neighborDist)
				{
					sum += vehicle.velocity;
					count++;
				}
			}
		}
		if (count > 0)
		{
			sum /= count;
			sum *= maxSpeed;

			Vector3 steeringForce = sum - velocity;
			LimitForce(steeringForce, maxForce);
			return steeringForce;
		}
		else
		{
			return Vector3.zero;
		}
	}


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
				if (dist < neighborDist)
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


	/// <summary>
	/// Calculate a steering force such that it
	/// allows us to get away from neighbors to not clip
	/// </summary>
	/// <param name="targetPosition">Current location of the thing to seperate from</param>
	/// <param name="distance">Current distance from the entity to seperate from</param>
	/// <returns>The steering force to get get away from the other object 
	public Vector3 Seperation(Vector3 targetPosition, float distance) {
		
		Vector3 desiredVelocity = targetPosition - this.transform.position;

		// Limit desired velocity by max speed
		desiredVelocity.Normalize();
		desiredVelocity *= -maxSpeed;

		// How do we turn to start moving towards
		// the desired velocity?
		Vector3 steeringForce = desiredVelocity - this.velocity;
		float seperationWeight = 1/distance;
		steeringForce *= seperationWeight;
		return steeringForce;
	}
}
