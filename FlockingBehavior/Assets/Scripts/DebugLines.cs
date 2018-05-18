using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Is put on an object with a Vehicle component to draw debug lines
public class DebugLines : MonoBehaviour {
	private Vehicle vehicleComponent;


	// Use this for initialization
	void Start () {
		vehicleComponent = GetComponent(typeof(Vehicle)) as Vehicle;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnRenderObject() {
		// Draw right vector
		if(vehicleComponent != null) {
			vehicleComponent.rightVectorMaterial.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Vertex(vehicleComponent.position);
			GL.Vertex(vehicleComponent.position + this.transform.right);
			GL.End();

			vehicleComponent.forwardVectorMaterial.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Vertex(vehicleComponent.position);
			GL.Vertex(vehicleComponent.position + this.transform.forward);
			GL.End();
		}
	}
}
