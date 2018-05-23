using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to spawn, manage, and update the Vehicle objects
/// </summary>
public class AgentManager : MonoBehaviour {

	/// <summary>
	/// Prefab of a vehicle to spawn
	/// </summary>
	public Vehicle arrowPrefab;
	/// <summary>
	/// Used to keep track of 
	/// </summary>
	public List<Vehicle> vehicles;
	private const int NUMBER_OF_ARROWS_TO_SPAWN = 100;

	/// <summary>
	/// Force multipliers
	/// </summary>
	private const float SEPERATE_MULTIPLIER = 2f;
	private const float ALIGN_MULTIPLIER = 1.5f;
	private const float COHESION_MULTIPLIER = 1;
	private const float SEEK_MULTIPLIER = 1;

	public bool drawDebugLines;


	/// <summary>
	/// Randomly spawns NUMBER_OF_ARROWS_TO_SPAWN arrows and adds them to the vehicles list
	/// </summary>
	void Start () {
		drawDebugLines = false;
		for(int i = 0; i < NUMBER_OF_ARROWS_TO_SPAWN; i++)
		{
			float spawnY = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).y, Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
			float spawnX = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x, Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);

			Vector2 spawnPosition = new Vector2(spawnX, spawnY);
			Vehicle vehicle = Instantiate<Vehicle>(arrowPrefab, spawnPosition, Quaternion.identity);
			vehicle.Init();
			vehicles.Add(vehicle);
		}
	}
	
	// Update is called once per frame
	void Update () {
		Thread[] threads = new Thread[NUMBER_OF_ARROWS_TO_SPAWN];

		// Call Flock
		for(int i = 0; i < NUMBER_OF_ARROWS_TO_SPAWN; i++)
		{
			// vehicle.CallFlock(vehicles, SEPERATE_MULTIPLIER, ALIGN_MULTIPLIER, COHESION_MULTIPLIER);
			threads[i] = new Thread(() => vehicles[i].CallFlock(vehicles, SEPERATE_MULTIPLIER, ALIGN_MULTIPLIER, COHESION_MULTIPLIER));
			threads[i].Start();
		}
		for (int i = 0; i < NUMBER_OF_ARROWS_TO_SPAWN; i++)
		{
			threads[i].Join();
		}


		foreach (Vehicle vehicle in vehicles)
		{
			vehicle.FinalizeMovement();
		}
		// Turn debug lines on/off
		if (Input.GetKeyDown(KeyCode.D)){
			drawDebugLines = !drawDebugLines;
		}
	}


	/// <summary>
	/// Returns a Vector3 representing the current location of the mouse in world space
	/// </summary>
	/// <returns></returns>
	private Vector3 MousePos()
	{
		Vector3 v = Input.mousePosition;
		v.z = 10.0f;
		v = Camera.main.ScreenToWorldPoint(v);
		return v;
	}


	/// <summary>
	/// Gets a seperate, align, and cohesion force, multiplies them by their respective force multipliers, and applies
	/// those forces to all members of the vehicles list. It then goes through and finalizes the movement for all members
	/// of the vehicles list.
	/// </summary>
	private void CallFlock()
	{

		foreach(Vehicle vehicle in vehicles)
		{
			Vector3 seperateForce = vehicle.Seperate(vehicles);
			Vector3 alignForce = vehicle.Align(vehicles);
			Vector3 cohesionForce = vehicle.Cohesion(vehicles);

			seperateForce *= SEPERATE_MULTIPLIER;
			alignForce *= ALIGN_MULTIPLIER;
			cohesionForce *= COHESION_MULTIPLIER;

			vehicle.ApplyForce(seperateForce);
			vehicle.ApplyForce(alignForce);
			vehicle.ApplyForce(cohesionForce);
		}

		// Finalize movement
		foreach(Vehicle vehicle in vehicles)
		{
			vehicle.FinalizeMovement();
		}
	}
}
