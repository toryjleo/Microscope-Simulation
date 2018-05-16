using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used to manage the human and zombie objects in the scene
public class AgentManager : MonoBehaviour {
	public GameObject zombiePrefab;
	private List<GameObject> humans;
	private List<GameObject> zombies;
	public Vehicle vehicle;
	private GameObject[] obsticles;
	private bool seeking, fleeing;
	public bool drawDebugLines;
	// Use this for initialization
	void Start () {
		humans = new List<GameObject> ();
		zombies = new List<GameObject> ();
		drawDebugLines = false;

	}
	
	// Update is called once per frame
	void Update () {

		// Turn debug lines on/off
		if(Input.GetKeyDown(KeyCode.D)){
			drawDebugLines = !drawDebugLines;
		}

		CallSeekOnPosition(vehicle, MousePos());
		/*
		// Do function calls to seek or flee for all vehicles
		foreach (GameObject human in humans) {
			Human humanComponent = human.GetComponent(typeof(Human)) as Human;
			DebugLines debugComponent = human.GetComponent(typeof(DebugLines)) as DebugLines;

			debugComponent.enabled = drawDebugLines;
			
			if(human.transform.position.x >= 18 || human.transform.position.x <= -18 || human.transform.position.z >= 18 || human.transform.position.z <= -18) {
				CallSeekOnPosition(humanComponent, Vector2.zero);
			} else  {
				bool isEvading = false;
				foreach (GameObject zombie in zombies) {
					if(Vector3.Distance(zombie.transform.position, human.transform.position) < 6) {
						isEvading = true;
						CallEvade(humanComponent);
					}
				}
				if(!isEvading) {
					CallWander(humanComponent);
				}

			} 
			// Avoid objects at all costs
			CallAvoid(humanComponent);

			// Worry about freakin' clipping other humans
			foreach (GameObject otherHuman in zombies) {
				if(otherHuman == human) {
					continue;
				} else if (Vector3.Distance(human.transform.position, otherHuman.transform.position) < 1.5f){
					Zombie otherHumanComponent = otherHuman.GetComponent(typeof(Zombie)) as Zombie;
					CallSeperate(humanComponent, otherHumanComponent);
				}
			}
		}

		foreach (GameObject zombie in zombies) {
			Zombie zombieComponent = zombie.GetComponent(typeof(Zombie)) as Zombie;
			DebugLines debugComponent = zombie.GetComponent(typeof(DebugLines)) as DebugLines;

			debugComponent.enabled = drawDebugLines;

			if(zombie.transform.position.x >= 18 || zombie.transform.position.x <= -18 || zombie.transform.position.z >= 18 || zombie.transform.position.z <= -18) {
				CallSeekOnPosition(zombieComponent, Vector2.zero);
			} else if(humans.Count != 0) {
				CallPursue(zombieComponent);

				// Zombie "infects" a human
				if (Vector3.Distance (zombieComponent.position, zombieComponent.seekTarget.transform.position) < 1) {
					MakeHumanZombie (zombieComponent.seekTarget);
				}
			} else {
				CallWander(zombieComponent);
			}
			// Avoid objects at all cost
			CallAvoid(zombieComponent);

			// Worry about freakin' clipping other zombies
			foreach (GameObject otherZombie in zombies) {
				if(otherZombie == zombie) {
					continue;
				} else if (Vector3.Distance(zombie.transform.position, otherZombie.transform.position) < 1.5f){
					Zombie otherZombieComponent = otherZombie.GetComponent(typeof(Zombie)) as Zombie;
					CallSeperate(zombieComponent, otherZombieComponent);
				}
			}

		}*/

	}

	private Vector3 MousePos()
	{
		Vector3 v = Input.mousePosition;
		v.z = 10.0f;
		v = Camera.main.ScreenToWorldPoint(v);
		return v;
	}



	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to seek
	/// </summary>
	private void CallSeek(Vehicle vehicleComponent) {
		Vector3 targetPosition = vehicleComponent.seekTarget.transform.position;
		Vector3 steer = vehicleComponent.Seek( targetPosition );
		vehicleComponent.ApplyForce(steer);


		// Check if need to update waypoint
		if(Vector3.Distance(vehicleComponent.transform.position, targetPosition) < 2 && vehicleComponent.seekTarget.tag == "target") {
			vehicleComponent.seekTarget.transform.position = new Vector3(Random.Range(-9.0f, 9.0f), targetPosition.y, Random.Range(-9.0f, 9.0f));
		}
	}

	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to seek to get to the specified position
	/// </summary>
	private void CallSeekOnPosition(Vehicle vehicleComponent, Vector3 position) {
		Vector3 steer = vehicleComponent.Seek( position );
		vehicleComponent.ApplyForce(steer);

	}

	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to flee
	/// </summary>
	private void CallFlee(Vehicle vehicleComponent) {
		Vector3 targetPosition = vehicleComponent.fleeTarget.transform.position;
		Vector3 steer = vehicleComponent.Flee( targetPosition );
		vehicleComponent.ApplyForce(steer);
	}

	/*
	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to avoid obsticles
	/// </summary>
	private void CallAvoid(Vehicle vehicleComponent) {
		foreach(GameObject obsticle in obsticles) {
			Obsticle obsticleComponent = obsticle.GetComponent(typeof(Obsticle)) as Obsticle;
			Vector3 steer = vehicleComponent.AvoidObsticle( obsticleComponent );
			vehicleComponent.ApplyForce(steer);
		}
	}*/

	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to pursue its target
	/// </summary>
	private void CallPursue(Vehicle vehicleComponent) {
		if (Vector3.Distance (vehicleComponent.position, vehicleComponent.seekTarget.transform.position) > 3) {
			// If you arer a good distance from your target, keep pursuing them
			Vector3 targetPosition = vehicleComponent.seekTarget.transform.position;
			Vector3 targetVelocity = (vehicleComponent.seekTarget.GetComponent(typeof(Vehicle)) as Vehicle).velocity;
			Vector3 steer = vehicleComponent.Persue (targetPosition, targetVelocity);
			vehicleComponent.ApplyForce (steer);
		} else {
			// else go to your target's position
			CallSeek(vehicleComponent);
		}
			
	}

	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to flee
	/// </summary>
	private void CallEvade(Vehicle vehicleComponent) {
		Vector3 targetPosition = vehicleComponent.fleeTarget.transform.position;
		Vehicle targetVehicle = vehicleComponent.fleeTarget.GetComponent(typeof(Vehicle)) as Vehicle;
		Vector3 targetVelocity = targetVehicle.velocity;
		Vector3 steer = vehicleComponent.Evade( targetPosition, targetVelocity);
		vehicleComponent.ApplyForce(steer);
	}

	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to wander around aimlessly
	/// </summary>
	private void CallWander(Vehicle vehicleComponent) {
		Vector3 wanderForce = vehicleComponent.Wander();
		vehicleComponent.ApplyForce(wanderForce);
	}

	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to seperate from the vehicleToGetAwayFrom
	/// </summary>
	private void CallSeperate(Vehicle vehicleComponent, Vehicle vehicleToGetAwayFrom) {
		Vector3 seperateForce = vehicleComponent.Seperation(vehicleToGetAwayFrom.transform.position, Vector3.Distance(vehicleComponent.transform.position, vehicleToGetAwayFrom.transform.position));
		vehicleComponent.ApplyForce(seperateForce);
	}
}
