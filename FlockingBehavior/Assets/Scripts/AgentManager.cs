using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used to manage the human and zombie objects in the scene
public class AgentManager : MonoBehaviour {
	public Vehicle arrowPrefab;
	public List<Vehicle> vehicles;
	private const int NUMBER_OF_ARROWS_TO_SPAWN = 100;

	/// <summary>
	/// Force multipliers
	/// </summary>
	private const float SEPERATE_MULTIPLIER = 2f;
	private const float ALIGN_MODIFIER = 1.5f;
	private const float COHESION_MULTIPLIER = 1;
	private const float SEEK_MULTIPLIER = 1;

	public bool drawDebugLines;
	// Use this for initialization
	void Start () {
		drawDebugLines = false;
		for(int i = 0; i < NUMBER_OF_ARROWS_TO_SPAWN; i++)
		{
			/*Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
			spawnPosition = new Vector3(spawnPosition.x + 1 / (i + 1), spawnPosition.y + 1 / (i + 1), 0);*/
			float spawnY = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).y, Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
			float spawnX = Random.Range
				(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x, Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);

			Vector2 spawnPosition = new Vector2(spawnX, spawnY);
			Vehicle vehicle = Instantiate<Vehicle>(arrowPrefab, spawnPosition, Quaternion.identity);
			vehicles.Add(vehicle);
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Turn debug lines on/off
		if(Input.GetKeyDown(KeyCode.D)){
			drawDebugLines = !drawDebugLines;
		}
		CallFlock(vehicles);
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
	}

	private void CallFlock(List<Vehicle> otherVehicles)
	{
		foreach(Vehicle vehicle in vehicles)
		{
			Vector3 seperateForce = vehicle.Seperate(vehicles);
			Vector3 alignForce = vehicle.Align(vehicles);
			Vector3 cohesionForce = vehicle.Cohesion(vehicles);

			seperateForce *= SEPERATE_MULTIPLIER;
			alignForce *= ALIGN_MODIFIER;
			cohesionForce *= COHESION_MULTIPLIER;

			vehicle.ApplyForce(seperateForce);
			vehicle.ApplyForce(alignForce);
			vehicle.ApplyForce(cohesionForce);
		}
	}
}
