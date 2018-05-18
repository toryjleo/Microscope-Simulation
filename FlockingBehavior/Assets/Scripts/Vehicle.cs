using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Vehicle is a class that is attached to a gameobject that implements various autonomous agent properties
public class Vehicle : MonoBehaviour {

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


	// Variable for how far to look ahead to avoid incoming obsticles
	public float distanceAhead;
	public float radius;

	// Timesteps
	public float persueTimeStep;
	public float evadeTimeStep;
	public float wanderTimeStep;


	// Used to determine when to set a new wander waypoint
	public float deltaTime;
	public float resetAmount;

	// Materials used for debug lines
	public Material forwardVectorMaterial;
	public Material rightVectorMaterial;
	public Material targetVectorMaterial;

	// Use this for initialization
	public virtual void Start () {
		acceleration = Vector3.zero;
		velocity =  new Vector3(0, 0, 0);
		position = this.transform.position;

		maxSpeed = 5;
		maxForce = 0.1f;
		desiredSeperation = 1;
		neighborDist = 4;
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

		// Throw the position back to Unity
		this.transform.position = position;

		// We're done with forces for this frame
		acceleration = Vector3.zero;
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
	/// allows us to seek a target's future position
	/// </summary>
	/// <param name="position">Current location of the target</param>
	/// <param name="vel">Current velocity of the target</param>
	/// <returns>The steering force to get to the target's position in persueTimestep seconds</returns>
	/*public Vector3 Persue(Vector3 position, Vector3 vel) {
		
		Vector3 futurePos = seekTarget.transform.position + vel * persueTimeStep;
		Debug.Log (futurePos);
		return Seek(futurePos);
	}*/

	/// <summary>
	/// Calculate a steering force such that it
	/// pushes the vehicle in a random direction in front of it
	/// </summary>
	/// <returns>The steering force to a random direction in front of the wanderer</returns>
	/*public Vector3 Wander() {
		
		if (deltaTime <=0 ) {
			Debug.Log("reset");
			Vector3 randomOffset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1,1));
			randomOffset.Normalize();
			randomOffset *= wanderRadius;
			futurePosition = position + (velocity * wanderTimeStep);
			futurePosition += randomOffset;
			deltaTime = Random.Range(1,resetAmount);
		} else {
			Debug.Log(deltaTime);
			deltaTime -= Time.deltaTime;
		}
		return Seek(futurePosition);
	}*/


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
