using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Class used to spawn, manage, and update the Vehicle objects
/// </summary>
public class BoidManager : MonoBehaviour {

	#region CONSTS

	// Consts for spawning/respawning
	private const int NUMBER_OF_VIRUSES_TO_SPAWN = 100;
	private const int NUMBER_OF_VIRUSES_BEFORE_RESPAWN = 20;
	private const int NUMBER_OF_WHITE_CELLS_TO_SPAWN = 1;

	/// <summary>
	/// Const for threading
	/// </summary>
	private const int NUMBER_OF_THREADS = 4;

	// Force multipliers for Viruses
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

	#endregion

	#region VARIABLES

	/// <summary>
	/// Prefab of a vehicle to spawn
	/// </summary>
	public Virus virusPrefab;

	public WhiteCell whiteCellPrefab;

	public Camera mainCamera;

	/// <summary>
	/// Used to keep track of vehicles
	/// </summary>
	public List<Boid> viruses;

	public List<Boid> whiteCells;

	public PlayerWhiteCell player;

	/// <summary>
	/// Reference to the white background's renderer. Used to set the background's shader's unifroms
	/// </summary>
	public Renderer backgroundRenderer;

	#endregion

	#region METHODS

	/// <summary>
	/// Randomly spawns NUMBER_OF_ARROWS_TO_SPAWN arrows and adds them to the vehicles list
	/// </summary>
	void Start ()
	{
		// Initialize Viruses
		for(int i = 0; i < NUMBER_OF_VIRUSES_TO_SPAWN; i++)
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

		// Initialize the player controlled WhiteCell
		player.Init();
	}
	

	// Update is called once per frame
	void Update ()
	{
		// Initialize a list of all the boids for the viruses to avoid
		List<Boid> avoidList = new List<Boid>();
		avoidList.AddRange(whiteCells);
		avoidList.Add(player);

		// Initialize an array of threads
		int subArrayLen = NUMBER_OF_VIRUSES_TO_SPAWN / NUMBER_OF_THREADS;
		Thread[] threads = new Thread[NUMBER_OF_THREADS];

		// Get the nuber of viruses that are not consumed
		int numVirusesAlive = viruses.Where(x => x.IsAlive).Count<Boid>();

		// When the number of viruses is low, just spawn a bunch on the eastern bounds
		if (numVirusesAlive <= NUMBER_OF_VIRUSES_BEFORE_RESPAWN)
		{
			foreach (Boid virus in viruses)
			{
				Virus virusCast = virus as Virus;
				if (virusCast == null)
				{
					UnityEngine.Debug.LogError("Element in List viruses is not of type Virus");
				}
				else if (!virusCast.IsAlive)
				{
					float xLoc = Random.Range(virusCast.eastBounds - 1, virusCast.eastBounds);
					float yLoc = Random.Range(virusCast.southBounds, virusCast.northBounds);
					virusCast.Respawn(xLoc, yLoc);
				}
			}
		}

		// Call Flock for the Viruses
		for (int i = 0; i < NUMBER_OF_THREADS; i++)
		{
			// List of boids to pass to the thread
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
			threads[i] = new Thread(() => CallVirusBehaviours(subList, viruses, avoidList, VIRUS_SEPERATE_MULTIPLIER, VIRUS_ALIGN_MULTIPLIER, 
				VIRUS_COHESION_MULTIPLIER, VIRUS_AVOID_CELL_MULTIPLIER));
			threads[i].Start();
		}

		// Wait for threads to finish execution
		for (int i = 0; i < NUMBER_OF_THREADS; i++)
		{
			threads[i].Join();
		}

		// Call Chase for the WhiteCells
		foreach (Boid whiteCell in whiteCells)
		{
			whiteCell.CallChase(viruses as List<Boid>, WHITE_CELL_SEEK_MULTIPLIER);
		}

		// Apply forces to the player's controlled whiteCell, based on player input
		player.PlayerInput();

		// Finalize the movement of all the boids
		foreach (Boid virus in viruses)
		{
			virus.FinalizeMovement();
		}
		foreach (Boid whiteCell in whiteCells)
		{
			whiteCell.FinalizeMovement();
		}
		player.FinalizeMovement();

		UpdateShader();
	}


	/// <summary>
	/// Sets the background shader's uniforms based on user input. Specifically, this sets the two WhiteCell position 
	/// uniforms for the "AnimatedCellShader" 
	/// </summary>
	private void UpdateShader()
	{
		if (backgroundRenderer != null && backgroundRenderer.material.shader != null)
		{
			// Update the shader with the one whitecell's position
			Vector3 whiteCellPos = mainCamera.WorldToScreenPoint(whiteCells[0].position);
			backgroundRenderer.material.SetVector("_WhiteCellOne", new Vector2(whiteCellPos.x, whiteCellPos.y));

			// Update the shader with the one player whitecell's position
			whiteCellPos = mainCamera.WorldToScreenPoint(player.position);
			backgroundRenderer.material.SetVector("_WhiteCellTwo", new Vector2(whiteCellPos.x, whiteCellPos.y));
		}
	}


	/// <summary>
	/// Calls CallFlock() and CallAvoid() for all members of boidList
	/// </summary>
	/// <param name="boidList">List of boids to apply flocking behaviour to. A Subset of others</param>
	/// <param name="others">List of all the boids to flock toward</param>
	/// <param name="avoidList">List of all the boids to avoid making contact with. Should be a list of WhiteCells</param>
	/// <param name="sperateMultiplier">Multiplier for the sperate force</param>
	/// <param name="alignMultiplier">Multiplier for the align force</param>
	/// <param name="cohesionMultiplier">Multiplier for the cohesion force</param>
	/// <param name="avoidCellMultiplier">Multiplier for the force that steers the vehicle away from the red cell</param>
	private void CallVirusBehaviours(List<Boid> boidList, List<Boid> others, List<Boid> avoidList, float sperateMultiplier,
		float alignMultiplier, float cohesionMultiplier, float avoidCellMultiplier)
	{

		foreach (Virus virus in boidList)
		{
			if (virus.IsAlive)
			{
				virus.CallFlock(others, sperateMultiplier, alignMultiplier, cohesionMultiplier);

				foreach (Boid whiteCell in avoidList)
				{
					virus.CallAvoid(whiteCell.position, avoidCellMultiplier, MAX_DISTANCE_FROM_WHITE_CELL);
				}
			}
		}
	}

	#endregion
}
