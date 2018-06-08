using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellShaderUpdater : MonoBehaviour {

	private Renderer rend;

	// Use this for initialization
	void Start ()
	{
		rend = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (rend != null && rend.material.shader != null)
		{
			Vector3 mousePos = MousePosScreenSpace();
			rend.material.SetVector("_MousePos", new Vector2(mousePos.x, mousePos.y));
		}
	}

	private Vector3 MousePosScreenSpace()
	{
		Vector3 v = Input.mousePosition;
		//v.z = 10.0f;
		//v = Camera.main.ScreenToWorldPoint(v);
		return v;
	}
}
