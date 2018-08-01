using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : Boid
{

	#region VARIABLES

	SpriteRenderer sprite = null;

	private Color aliveColor;

	private float spriteHeight;
	private float rotationSpeed = 0;

	#endregion

	#region METHODS

	private void Update()
	{
		transform.Rotate(Vector3.forward * rotationSpeed);
	}


	/// <summary>
	/// Used to initialize member variables of this Virus object
	/// </summary>
	public override void Init()
	{
		base.Init();
		isAlive = true;
		sprite = gameObject.GetComponent<SpriteRenderer>();
		aliveColor = new Color(1, 1, 1, 0.6f);
		sprite.color = aliveColor;
		spriteHeight = sprite.size.y;
		desiredSeperation = 1.5f * spriteHeight;
		rotationSpeed = Random.Range(-1.5f, 1.5f);
	}


	public override void Respawn(float xLoc, float yLoc)
	{
		base.Respawn(xLoc, yLoc);
		sprite.color = aliveColor;
	}


	/// <summary>
	/// Disables the virus object and makes a call to DeathCoroutine
	/// </summary>
	public void Die()
	{
		isAlive = false;
		if (sprite != null)
		{
			StartCoroutine(DeathCoroutine());
		}
	}


	/// <summary>
	/// Causes the color of the sprite to darken and fade over time
	/// </summary>
	/// <returns></returns>
	private IEnumerator DeathCoroutine()
	{
		while (sprite.color.a > 0)
		{
			Color col = sprite.color;
			col.a -= .15f;
			col.r -= .15f;
			col.g -= .15f;
			col.b -= .15f;
			sprite.color = col;
			yield return new WaitForSeconds(.05f);
		}
		yield return null;
	}

	#endregion

}
