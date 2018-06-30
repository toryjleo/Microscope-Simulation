using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

/// <summary>
/// Class used to spawn, manage, and update the Vehicle objects
/// </summary>
public class BoidManager : MonoBehaviour {

	/// <summary>
	/// Consts for threading
	/// </summary>
	private const int NUMBER_OF_ARROWS_TO_SPAWN = 100;
	private const int NUMBER_OF_WHITE_CELLS_TO_SPAWN = 1;
	private const int NUMBER_OF_THREADS = 4;

	/// <summary>
	/// Consts for the red cell
	/// </summary>
	private const int RIGHT_MOUSE_BTN = 1;
	private const float MAX_CELL_RADIUS_SIZE = 5.5f;
	private const float MIN_CELL_RADIUS_SIZE = 0.5f;
	private const float CELL_GROWTH_RATE = 5.0f;

	/// <summary>
	/// Force multipliers for Viruses
	/// </summary>
	private const float VIRUS_SEPERATE_MULTIPLIER = 2f;
	private const float VIRUS_ALIGN_MULTIPLIER = 1.75f;
	private const float VIRUS_COHESION_MULTIPLIER = 0.5f;
	private const float VIRUS_SEEK_MULTIPLIER = 1.0f;
	private const float VIRUS_AVOID_CELL_MULTIPLIER = 10f;
	private const float MAX_DISTANCE_FROM_WHITE_CELL = 2.5f;

	/// <summary>
	/// Force multipliers for White Cells
	/// </summary>
	private const float WHITE_CELL_SEEK_MULTIPLIER = 1.0f;

	/// <summary>
	/// Prefab of a vehicle to spawn
	/// </summary>
	public Virus virusPrefab;

	public WhiteCell whiteCellPrefab;

	public Camera camera;

	/// <summary>
	/// Used to keep track of vehicles
	/// </summary>
	public List<Boid> viruses;

	public List<WhiteCell> whiteCells;

	public PlayerWhiteCell player;

	/// <summary>
	/// Reference to the white background's renderer. Used to set the background's shader's unifroms
	/// </summary>
	public Renderer backgroundRenderer;

	/// <summary>
	/// Tracks the current size of the red cell's radius
	/// </summary>
	private float currentCellRadius = MIN_CELL_RADIUS_SIZE;


	/// <summary>
	/// Randomly spawns NUMBER_OF_ARROWS_TO_SPAWN arrows and adds them to the vehicles list
	/// </summary>
	void Start () {
		// Initialize Viruses
		for(int i = 0; i < NUMBER_OF_ARROWS_TO_SPAWN; i++)
		{
			float spawnY = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).y, Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
			float spawnX = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x, Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);

			Vector2 spawnPosition = new Vector2(spawnX, spawnY);
			Virus virus = Instantiate<Virus>(virusPrefab, spawnPosition, Quaternion.identity);
			virus.Init();
			viruses.Add(virus);
		}

		// Initialize White Cells
		for (int i = 0; i < NUMBER_OF_WHITE_CELLS_TO_SPAWN; i++)
		{
			float spawnY = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).y, Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
			float spawnX = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x, Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);

			Vector2 spawnPosition = new Vector2(spawnX, spawnY);
			WhiteCell whiteCell = Instantiate<WhiteCell>(whiteCellPrefab, spawnPosition, Quaternion.identity);
			whiteCell.Init();
			whiteCells.Add(whiteCell);
		}
		player.Init();
	}
	

	// Update is called once per frame
	void Update () {
		int subArrayLen = NUMBER_OF_ARROWS_TO_SPAWN / NUMBER_OF_THREADS;
		Thread[] threads = new Thread[NUMBER_OF_THREADS];

		// Call Flock for the Viruses
		for (int i = 0; i < NUMBER_OF_THREADS; i++)
		{
			// List of vehicles to pass to the thread
			List<Boid> subList;
			// If this is the last thread to spin up, it will handle the remainder of the vehicles
			if (i == NUMBER_OF_THREADS - 1)
			{
				subList = viruses.GetRange(i * subArrayLen, viruses.Count - (i * subArrayLen));
			}
			else
			{
				subList = viruses.GetRange(i * subArrayLen, subArrayLen);
			}
			// Spin up that thread
			threads[i] = new Thread(() => CallFlock(subList, viruses, VIRUS_SEPERATE_MULTIPLIER, VIRUS_ALIGN_MULTIPLIER, 
				VIRUS_COHESION_MULTIPLIER, VIRUS_AVOID_CELL_MULTIPLIER));
			threads[i].Start();
		}
		for (int i = 0; i < NUMBER_OF_THREADS; i++)
		{
			threads[i].Join();
		}

		// Call Seek for the WhiteCells
		foreach (WhiteCell cell in whiteCells)
		{
			cell.CallChase(viruses as List<Boid>, WHITE_CELL_SEEK_MULTIPLIER);
		}

		player.PlayerInput();

		foreach (Boid vehicle in viruses)
		{
			vehicle.FinalizeMovement();
		}

		foreach (WhiteCell cell in whiteCells)
		{
			cell.FinalizeMovement();
		}

		player.FinalizeMovement();

		UpdateShader();
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
			Vector3 whiteCellPos = camera.WorldToScreenPoint(whiteCells[0].position);
			backgroundRenderer.material.SetVector("_WhiteCellOne", new Vector2(whiteCellPos.x, whiteCellPos.y));

			whiteCellPos = camera.WorldToScreenPoint(player.position);
			backgroundRenderer.material.SetVector("_WhiteCellTwo", new Vector2(whiteCellPos.x, whiteCellPos.y));


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


	/// <summary>
	/// Calls Flock() for all members of boidList
	/// </summary>
	/// <param name="boidList">List of boids to call Flock() on. A Subset of others</param>
	/// <param name="others">List of all the boids</param>
	/// <param name="sperateMultiplier">Multiplier for the sperate force</param>
	/// <param name="alignMultiplier">Multiplier for the align force</param>
	/// <param name="cohesionMultiplier">Multiplier for the cohesion force</param>
	/// <param name="avoidCellMultiplier">Multiplier for the force that steers the vehicle away from the red cell</param>
	private void CallFlock(List<Boid> boidList, List<Boid> others, float sperateMultiplier,
		float alignMultiplier, float cohesionMultiplier, float avoidCellMultiplier)
	{

		foreach (Virus virus in boidList)
		{
			if (virus.IsAlive)
			{
				virus.CallFlock(others, sperateMultiplier, alignMultiplier, cohesionMultiplier);

				foreach (WhiteCell whiteCell in whiteCells)
				{
					virus.CallAvoid(whiteCell.position, avoidCellMultiplier, MAX_DISTANCE_FROM_WHITE_CELL);
				}
			}
		}
	}
}
