using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Vehicle is a class that is attached to a gameobject that implements various autonomous agent properties
public class Vehicle : MonoBehaviour {

	// Public variables that we can tweak in Unity
	public GameObject seekTarget;
	public GameObject fleeTarget;
	public float maxSpeed;
	public float mass;

	// Private variables for tracking physics stuff
	public Vector3 acceleration;
	public Vector3 velocity;
	public Vector3 position;

	// Weights 
	public float seekWeight;
	public float fleeWeight;
	public float wanderRadius;

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
	private Vector3 futurePosition;

	// Materials used for debug lines
	public Material forwardVectorMaterial;
	public Material rightVectorMaterial;
	public Material targetVectorMaterial;

	// Use this for initialization
	public virtual void Start () {
		acceleration = Vector3.zero;
		velocity =  new Vector3(0, 0, 0);
		position = this.transform.position;
		seekWeight = 1;
		fleeWeight = 1;
		wanderRadius = 2;

		distanceAhead = 5;

		persueTimeStep = 1;
		evadeTimeStep = 1;
		wanderTimeStep = 5;
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

		// Add, or multiply more rather, the seekWeight
		steeringForce *= seekWeight;
		return steeringForce;
	}

	/// <summary>
	/// Calculate a steering force such that it
	/// allows us to seek a target's future position
	/// </summary>
	/// <param name="position">Current location of the target</param>
	/// <param name="vel">Current velocity of the target</param>
	/// <returns>The steering force to get to the target's position in persueTimestep seconds</returns>
	public Vector3 Persue(Vector3 position, Vector3 vel) {
		
		Vector3 futurePos = seekTarget.transform.position + vel * persueTimeStep;
		Debug.Log (futurePos);
		return Seek(futurePos);
	}

	/// <summary>
	/// Calculate a steering force such that it
	/// allows us to flee in the opposite direction
	///  of a target position
	/// </summary>
	/// <param name="targetPosition">Where to seek</param>
	/// <returns>The steering force to get away from the target</returns>
	public Vector3 Flee(Vector3 targetPosition) {
		// Calculate our "perfect" desired velocity
		Vector3 desiredVelocity = targetPosition - this.transform.position;

		// Limit desired velocity by max speed
		desiredVelocity.Normalize();
		desiredVelocity *= -maxSpeed;

		// How do we turn to start moving towards
		// the desired velocity?
		Vector3 steeringForce = desiredVelocity - this.velocity;
		steeringForce *= fleeWeight;
		return steeringForce;
	}

	/// <summary>
	/// Calculate a steering force such that it
	/// allows us to flee a target's future position
	/// </summary>
	/// <param name="position">Current location of the target</param>
	/// <param name="vel">Current velocity of the target</param>
	/// <returns>The steering force to get get away from the target's position in persueTimestep seconds</returns>
	public Vector3 Evade(Vector3 position, Vector3 vel) {

		Vector3 futurePos = fleeTarget.transform.position + vel * evadeTimeStep;
		return Flee(futurePos);
	}

	/// <summary>
	/// Calculate a steering force such that it
	/// pushes the vehicle in a random direction in front of it
	/// </summary>
	/// <returns>The steering force to a random direction in front of the wanderer</returns>
	public Vector3 Wander() {
		
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
