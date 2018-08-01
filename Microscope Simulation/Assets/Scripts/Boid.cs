using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Boid is a class that is attached to a gameobject that implements various autonomous agent properties
public class Boid : MonoBehaviour {

	#region CONSTS

	/// <summary>
	/// How far to go fromt he center of the screen before wrapping
	/// </summary>
	private const float  WIDTH_TO_WRAP = 11.25f;
	private const float HEIGHT_TO_WRAP = 6.25f;

	public const float MAX_FORCE = 0.1f;
	public const float MASS = 1.0f;

	#endregion

	#region VARIABLES
	public float maxSpeed;

	// Bounds the Boid cannot exceed in the world space
	public float northBounds;
	public float southBounds;
	public float eastBounds;
	public float westBounds;

	/// <summary>
	/// A Boid's desired distance from its neighbor
	/// </summary>
	protected float desiredSeperation;

	/// <summary>
	/// The max distance for another Boid to be considered a 'neighbor'
	/// </summary>
	private float furthestNeighbor;

	/// <summary>
	/// The furthest away distance another boid can be from this boid, to still be chased
	/// </summary>
	protected float furthestToChase;

	// Variables for tracking physics related stuff
	private Vector3 acceleration;
	private Vector3 velocity;
	public Vector3 position;

	protected bool isAlive;

	#endregion

	#region ACCESSORS

	public bool IsAlive
	{
		get
		{
			return isAlive;
		}
	}

	#endregion

	#region METHODS

	/// <summary>
	/// Used to initialize the member variables of this Boid
	/// </summary>
	public virtual void Init()
	{
		maxSpeed = 4.0f;

		Vector2 middleScreen = Camera.main.transform.position;

		northBounds = middleScreen.y + HEIGHT_TO_WRAP;
		southBounds = middleScreen.y - HEIGHT_TO_WRAP;
		eastBounds = middleScreen.x + WIDTH_TO_WRAP;
		westBounds = middleScreen.x - WIDTH_TO_WRAP;


		acceleration = Vector3.zero;
		velocity =  new Vector3(0, 0, 0);
		position = this.transform.position;

		furthestNeighbor = 2.5f;
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
	/// Internally handles the overall movement of the Boid object
	/// </summary>
	public void FinalizeMovement()
	{
		// Update acceleration
		velocity += acceleration * Time.deltaTime;

		// If the velocity is greater than maxSpeed, set the magnitude = maxSpeed
		if( Vector3.SqrMagnitude(velocity) > Mathf.Pow(maxSpeed, 2)) {
			velocity.Normalize();
			velocity *= maxSpeed;
		}
		
		// Update position
		position += velocity * Time.deltaTime;
		position = ClampPositionInBounds(position);
		this.transform.position = position;

		// Clear the acceleration for the next frame
		acceleration = Vector3.zero;
	}


	public virtual void Respawn(float xLoc, float yLoc)
	{
		Init();
		transform.position = new Vector2(xLoc, yLoc);
	}


	/// <summary>
	/// Finds the closest boid in the list and follows it
	/// </summary>
	/// <param name="boids">A list of boids to chase</param>
	/// <param name="chaseMultiplier">A scalar to multiply the chase force by</param>
	public void CallChase(List<Boid> boids, float chaseMultiplier)
	{
		Vector3 chaseForce = Chase(boids);

		chaseForce *= chaseMultiplier;

		ApplyForce(chaseForce);
	}


	/// <summary>
	/// Returns a force that is directed toward the closest boid in boids
	/// </summary>
	/// <param name="boids">A list of boids to chase</param>
	/// <returns>A force in the direction of the closest boid</returns>
	protected virtual Vector3 Chase(List<Boid> boids)
	{
		Vector3 closestTarget = this.position;
		// Some arbitrary distance, far away
		float closestSqrDist = Mathf.Pow(furthestToChase, 2);

		foreach (Boid boid in boids)
		{

			if (Vector3.SqrMagnitude(this.position - boid.position) < closestSqrDist)
			{
				closestSqrDist = Vector3.SqrMagnitude(this.position - boid.position);
				closestTarget = boid.position;
			}
		}
		return Seek(closestTarget);
	}


	/// <summary>
	/// Makes all relevant calls to apply a seperate force, align force, and a cohesion force which are the behaviors
	/// needed for a flocking algorithm
	/// </summary>
	/// <param name="boids">A list of references to other Boid objects</param>
	/// <param name="seperateMultiplier">A scalar to multiply the seperate force by</param>
	/// <param name="alignMultiplier">A scalar to multiply the align force by</param>
	/// <param name="cohesionMultiplier">A scalar to multiply the cohesion force by</param>
	public void CallFlock(List<Boid> boids, float seperateMultiplier, float alignMultiplier, float cohesionMultiplier)
	{
		Vector3 seperateForce = Avoid(boids);
		Vector3 alignForce = Align(boids);
		Vector3 cohesionForce = Cohesion(boids);

		seperateForce *= seperateMultiplier;
		alignForce *= alignMultiplier;
		cohesionForce *= cohesionMultiplier;

		ApplyForce(seperateForce);
		ApplyForce(alignForce);
		ApplyForce(cohesionForce);
	}


	/// <summary>
	/// If within a maxDist from the obstacle, try to move in the opposite direction of it.
	/// </summary>
	/// <param name="obstaclePosition">An object to avoid</param>
	/// <param name="avoidMultiplier">A scalar which will make this force a higher priority, the larger it is</param>
	/// <param name="maxDist">The maximum distance between this object's and the obstacle's position, for this force to 
	/// be applied</param>
	public void CallAvoid(Vector3 obstaclePosition, float avoidMultiplier, float maxDist)
	{
		float distSquared = Vector3.SqrMagnitude(obstaclePosition - this.position);
		if (distSquared < Mathf.Pow(maxDist, 2))
		{
			Vector3 avoidForce = Avoid(obstaclePosition);
			avoidForce *= avoidMultiplier;
			avoidForce *= 1 / (obstaclePosition - this.position).magnitude;
			ApplyForce(avoidForce);
		}
	}


	/// <summary>
	/// Kind of a crude solution, but get the Boid to show up on the opposite side if it goes out of bounds
	/// </summary>
	/// <param name="position">The current position of the Boid</param>
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
		desiredVelocity *= maxSpeed;
		Vector3 steeringForce = desiredVelocity - this.velocity;
		// Limit force
		LimitForce(steeringForce, MAX_FORCE);
		return steeringForce;
	}


	/// <summary>
	/// Get a force that moves this Boid away from all the other boids
	/// </summary>
	/// <param name="boids">A list of boids to move away from</param>
	/// <returns>A force that is the average vector that pushes this boid away from all the other boids</returns>
	public Vector3 Avoid(List<Boid> boids)
	{
		int count = 0;
		Vector3 sum = Vector3.zero;
		foreach (Boid boid in boids)
		{
			if (boid == this)
			{
				continue;
			}
			else
			{
				float sqrDist = Vector3.SqrMagnitude(this.position - boid.position);
				if (sqrDist < Mathf.Pow(desiredSeperation, 2))
				{
					Vector3 dir = this.position - boid.position;
					dir.Normalize();
					dir *= 1 / (this.position - boid.position).magnitude;
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
		LimitForce(steeringForce, MAX_FORCE);

		return steeringForce;
	}


	/// <summary>
	/// This will return a vector that will be ideal force to apply to get away from the passed in obstacle's position
	/// </summary>
	/// <param name="obstaclePosition">The position of the obstacle to avoid</param>
	/// <returns>A vector that pointing from obstaclePosition to this objects position</returns>
	public Vector3 Avoid(Vector3 obstaclePosition)
	{
		Vector3 dir = this.position - obstaclePosition;
		dir.Normalize();
		dir *= maxSpeed;
		Vector3 steeringForce = dir - velocity;
		LimitForce(steeringForce, MAX_FORCE);
		return steeringForce;
	}


	/// <summary>
	/// Get a force that moves this boid in the same direction as all the other boids
	/// </summary>
	/// <param name="boids">A list of boids to try to move in the same direction as</param>
	/// <returns>A force that is the average vector of all the other boids' velocities</returns>
	public Vector3 Align(List<Boid> boids)
	{
		int count = 0;
		Vector3 sum = Vector3.zero;
		foreach (Boid boid in boids)
		{
			if (boid == this)
			{
				continue;
			}
			else
			{
				float sqrDist = Vector3.SqrMagnitude(this.position - boid.position);
				if (sqrDist < Mathf.Pow(furthestNeighbor, 2))
				{
					sum += boid.velocity;
					count++;
				}
			}
		}
		if (count > 0)
		{
			sum /= count;
			sum *= maxSpeed;

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
	/// Get a force that moves this boid towards all the other boids
	/// </summary>
	/// <param name="boids">A list of boids to try to move towards</param>
	/// <returns>A force that is the average vector that pushes this boid towards all the other boids</returns>
	public Vector3 Cohesion(List<Boid> boids)
	{
		int count = 0;
		Vector3 sum = Vector3.zero;
		foreach (Boid boid in boids)
		{
			if (boid == this)
			{
				continue;
			}
			else
			{
				float sqrDist = Vector3.SqrMagnitude(this.position - boid.position);
				if (sqrDist < Mathf.Pow(furthestNeighbor, 2))
				{
					sum += boid.position;
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

	#endregion
}
