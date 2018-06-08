using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{

	private Material CRTMaterial;

	private void Awake()
	{
		CRTMaterial = new Material(Shader.Find("Custom/CRTShader"));
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, CRTMaterial);
	}
}