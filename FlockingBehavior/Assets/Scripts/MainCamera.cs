using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{

	public Material CRTMaterial;
	private bool useCRT = true;


	private void Update()
	{
		UserInput();
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (useCRT)
		{
			Graphics.Blit(source, destination, CRTMaterial);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}


	private void UserInput()
	{
		// Turns on/off post-processing effects
		/*if (Input.GetKeyDown(KeyCode.Space))
		{
			useCRT = !useCRT;
		}*/
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}