using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{

	private Material CRTMaterial;
	private Material VignetteMaterail;

	private void Awake()
	{
		CRTMaterial = new Material(Shader.Find("Custom/CRTShader"));
		VignetteMaterail = new Material(Shader.Find("Custom/Vignette"));
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, VignetteMaterail);
		Graphics.Blit(source, destination, CRTMaterial);
	}
}