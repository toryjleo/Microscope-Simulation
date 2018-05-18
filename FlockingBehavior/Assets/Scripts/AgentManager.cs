using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used to manage the human and zombie objects in the scene
public class AgentManager : MonoBehaviour {
	public Vehicle arrowPrefab;
	public List<Vehicle> vehicles;
	private const int NUMBER_OF_ARROWS_TO_SPAWN = 5;
	private const float SEPERATE_MULTIPLIER = 1.5f;
	private const float ALIGN_MODIFIER = 1;
	private const float COHESION_MULTIPLIER = 1;
	private const float SEEK_MULTIPLIER = 1;
	public bool drawDebugLines;
	// Use this for initialization
	void Start () {
		drawDebugLines = false;
		for(int i = 0; i < NUMBER_OF_ARROWS_TO_SPAWN; i++)
		{
			Vehicle vehicle = Instantiate<Vehicle>(arrowPrefab);
			vehicles.Add(vehicle);
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Turn debug lines on/off
		if(Input.GetKeyDown(KeyCode.D)){
			drawDebugLines = !drawDebugLines;
		}
		foreach (Vehicle vehicle in vehicles)
		{
			//Vector3 seekForce = vehicle.Seek(MousePos());
			Vector3 seperateForce = vehicle.Seperate(vehicles);
			Vector3 alignForce = vehicle.Align(vehicles);
			Vector3 cohesionForce = vehicle.Cohesion(vehicles);

			seperateForce *= SEPERATE_MULTIPLIER;
			alignForce *= ALIGN_MODIFIER;
			cohesionForce *= COHESION_MULTIPLIER;
			//seekForce *= SEEK_MULTIPLIER;

			vehicle.ApplyForce(seperateForce);
			vehicle.ApplyForce(alignForce);
			vehicle.ApplyForce(cohesionForce);
			//vehicle.ApplyForce(seekForce);
		}
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
	/// needed for the vehicle to wander around aimlessly
	/// </summary>
	/*private void CallWander(Vehicle vehicleComponent) {
		Vector3 wanderForce = vehicleComponent.Wander();
		vehicleComponent.ApplyForce(wanderForce);
	}*/

	/// <summary>
	/// Call functions related to the calculations
	/// needed for the vehicle to seperate from the vehicleToGetAwayFrom
	/// </summary>
	private void CallSeperate(Vehicle vehicleComponent, List<Vehicle> vehicles) {
		Vector3 sum = Vector3.zero;
		int count = 0;
		foreach (Vehicle v in vehicles)
		{
			if (v == vehicleComponent)
			{
				continue;
			}
			else
			{
				Vector3 seperateForce = vehicleComponent.Seperation(v.transform.position, Vector3.Distance(vehicleComponent.transform.position, v.transform.position));
			}
		}
		//vehicleComponent.ApplyForce(seperateForce);
	}

	private void CallFlock(Vehicle vehicleComponent, List<Vehicle> otherVehicles)
	{
		foreach(Vehicle other in vehicles)
		{

		}
	}
}
