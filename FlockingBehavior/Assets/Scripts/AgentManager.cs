using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

/// <summary>
/// Class used to spawn, manage, and update the Vehicle objects
/// </summary>
public class AgentManager : MonoBehaviour {

	/// <summary>
	/// Prefab of a vehicle to spawn
	/// </summary>
	public Vehicle arrowPrefab;
	/// <summary>
	/// Used to keep track of vehicles
	/// </summary>
	public List<Vehicle> vehicles;


	private const int NUMBER_OF_ARROWS_TO_SPAWN = 100;
	private const int NUMBER_OF_THREADS = 4;

	/// <summary>
	/// Force multipliers
	/// </summary>
	private const float SEPERATE_MULTIPLIER = 2f;
	private const float ALIGN_MULTIPLIER = 1.5f;
	private const float COHESION_MULTIPLIER = 0.5f;
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
		int subArrayLen = NUMBER_OF_ARROWS_TO_SPAWN / NUMBER_OF_THREADS;
		Thread[] threads = new Thread[NUMBER_OF_THREADS];

		// Call Flock
		for (int i = 0; i < NUMBER_OF_THREADS; i++)
		{
			// List of vehicles to pass to the thread
			List<Vehicle> subList;
			// If this is the last thread to spin up, it will handle the remainder of the vehicles
			if (i == NUMBER_OF_THREADS - 1)
			{
				subList = vehicles.GetRange(i * subArrayLen, vehicles.Count - (i * subArrayLen));
			}
			else
			{
				subList = vehicles.GetRange(i * subArrayLen, subArrayLen);
			}
			// Spin up that thread
			threads[i] = new Thread(() => CallFlock(subList, vehicles, SEPERATE_MULTIPLIER, ALIGN_MULTIPLIER, 
				COHESION_MULTIPLIER));
			threads[i].Start();
		}
		for (int i = 0; i < NUMBER_OF_THREADS; i++)
		{
			threads[i].Join();
		}

		foreach (Vehicle vehicle in vehicles)
		{
			vehicle.FinalizeMovement();
		}
		// Turn debug lines on/off
		if (Input.GetKeyDown(KeyCode.D))
		{
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
	/// Calls Flock() for all members of vehicleList
	/// </summary>
	/// <param name="vehicleList">List of vehicles to call Flock() on</param>
	/// <param name="others">List of all the vehicles</param>
	/// <param name="sperateMultiplier">Multiplier for the sperate force</param>
	/// <param name="alignMultiplier">Multiplier for the align force</param>
	/// <param name="cohesionMultiplier">Multiplier for the cohesion force</param>
	private void CallFlock(List<Vehicle> vehicleList, List<Vehicle> others, float sperateMultiplier, float alignMultiplier, float cohesionMultiplier)
	{
		foreach(Vehicle vehicle in vehicleList)
		{
			vehicle.CallFlock(others, sperateMultiplier, alignMultiplier, cohesionMultiplier);
		}
	}
}
