using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{

	public Material CRTMaterial;
	private bool useCRT = true;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			useCRT = !useCRT;
		}
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
}