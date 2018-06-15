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
	/// Consts for threading
	/// </summary>
	private const int NUMBER_OF_ARROWS_TO_SPAWN = 100;
	private const int NUMBER_OF_THREADS = 4;

	/// <summary>
	/// Consts for the red cell
	/// </summary>
	private const int RIGHT_MOUSE_BTN = 1;
	private const float MAX_CELL_RADIUS_SIZE = 5.5f;
	private const float MIN_CELL_RADIUS_SIZE = 0.2f;
	private const float CELL_GROWTH_RATE = 1.0f;

	/// <summary>
	/// Force multipliers
	/// </summary>
	private const float SEPERATE_MULTIPLIER = 2f;
	private const float ALIGN_MULTIPLIER = 1.75f;
	private const float COHESION_MULTIPLIER = 0.5f;
	private const float SEEK_MULTIPLIER = 1.0f;
	private const float AVOID_CELL_MULTIPLIER = 10f;

	/// <summary>
	/// Prefab of a vehicle to spawn
	/// </summary>
	public Vehicle arrowPrefab;

	/// <summary>
	/// Used to keep track of vehicles
	/// </summary>
	public List<Vehicle> vehicles;

	/// <summary>
	/// Reference to the white background's renderer. Used to set the background's shader's unifroms
	/// </summary>
	public Renderer backgroundRenderer;

	/// <summary>
	/// Tracks the current size of the red cell's radius
	/// </summary>
	private float currentCellRadius = MIN_CELL_RADIUS_SIZE;

	/// <summary>
	/// Number of threads sitting idle, waiting for other threads to get to the same point of execution
	/// </summary>
	private int idleThreadNumber = 0;


	/// <summary>
	/// Randomly spawns NUMBER_OF_ARROWS_TO_SPAWN arrows and adds them to the vehicles list
	/// </summary>
	void Start () {
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
		UpdateShader();

		// Get status of mouse for the Vehicles
		Vector3 mousePos = MousePosWorldSpace();
		bool rightMouseBtnDown = Input.GetMouseButton(RIGHT_MOUSE_BTN);

		int subArrayLen = NUMBER_OF_ARROWS_TO_SPAWN / NUMBER_OF_THREADS;
		Thread[] threads = new Thread[NUMBER_OF_THREADS];

		idleThreadNumber = 0;

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
			threads[i] = new Thread(() => CallFlock(subList, vehicles, mousePos, SEPERATE_MULTIPLIER, ALIGN_MULTIPLIER, 
				COHESION_MULTIPLIER, AVOID_CELL_MULTIPLIER, rightMouseBtnDown));
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
	}


	/// <summary>
	/// Sets the background shader's uniforms based on user input. Specifically, it gives the "AnimatedCellShader" the 
	/// current position of the mouse and the radius of the red cell
	/// </summary>
	private void UpdateShader()
	{
		if (backgroundRenderer != null && backgroundRenderer.material.shader != null)
		{
			// Update mouse position
			Vector3 mousePos = MousePosScreenSpace();
			backgroundRenderer.material.SetVector("_MousePos", new Vector2(mousePos.x, mousePos.y));

			// Update radius around mouse
			if (Input.GetMouseButton(RIGHT_MOUSE_BTN))
			{
				currentCellRadius += CELL_GROWTH_RATE * Time.deltaTime;
			}
			else
			{
				currentCellRadius -= CELL_GROWTH_RATE * Time.deltaTime;
			}
			currentCellRadius = Mathf.Clamp(currentCellRadius, MIN_CELL_RADIUS_SIZE, MAX_CELL_RADIUS_SIZE);
			backgroundRenderer.material.SetFloat("_MouseRadius", currentCellRadius);
		}
	}


	/// <summary>
	/// Returns a Vector3 representing the current location of the mouse in screen space
	/// </summary>
	/// <returns>A vector3 whose x and y coordinates are the screen space coordinates of the mouse</returns>
	private Vector3 MousePosScreenSpace()
	{
		Vector3 v = Input.mousePosition;
		return v;
	}


	/// <summary>
	/// Returns a Vector3 representing the current location of the mouse in world space
	/// </summary>
	/// <returns>A vector3 whose x and y coordinates are the world space coordinates of the mouse</returns>
	private Vector3 MousePosWorldSpace()
	{
		Vector3 v = Input.mousePosition;
		v.z = 10.0f;
		v = Camera.main.ScreenToWorldPoint(v);
		return v;
	}


	/// <param name="cohesionMultiplier">Multiplier for the cohesion force</param>
	/// <summary>
	/// Calls Flock() for all members of vehicleList
	/// </summary>
	/// <param name="vehicleList">List of vehicles to call Flock() on</param>
	/// <param name="others">List of all the vehicles</param>
	/// <param name="mousePos">The cpprdinates of the mouse in world space</param>
	/// <param name="sperateMultiplier">Multiplier for the sperate force</param>
	/// <param name="alignMultiplier">Multiplier for the align force</param>
	/// <param name="cohesionMultiplier">Multiplier for the cohesion force</param>
	/// <param name="avoidCellMultiplier">Multiplier for the force that steers the vehicle away from the red cell</param>
	/// <param name="rightMouseBtnDown">Whether or not the right mouse button is currently being held down</param>
	private void CallFlock(List<Vehicle> vehicleList, List<Vehicle> others, Vector3 mousePos, float sperateMultiplier,
		float alignMultiplier, float cohesionMultiplier, float avoidCellMultiplier, bool rightMouseBtnDown)
	{
		float maxDistFromMouse = currentCellRadius / 2;

		foreach (Vehicle vehicle in vehicleList)
		{
			vehicle.CallFlock(others, sperateMultiplier, alignMultiplier, cohesionMultiplier);
			if (rightMouseBtnDown)
			{
				vehicle.CallAvoid(mousePos, avoidCellMultiplier, maxDistFromMouse);
			}
		}
	}
}
