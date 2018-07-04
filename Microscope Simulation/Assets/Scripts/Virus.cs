using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : Boid
{

	#region VARIABLES

	SpriteRenderer sprite = null;

	private float spriteHeight;

	private bool isAlive;

	#endregion

	#region ACCESSORS

	public bool IsAlive
	{
		get
		{
			return isAlive;
		}
	}

	#endregion

	#region METHODS

	/// <summary>
	/// Used to initialize member variables of this Virus object
	/// </summary>
	public override void Init()
	{
		base.Init();
		isAlive = true;
		sprite = gameObject.GetComponent<SpriteRenderer>();
		spriteHeight = sprite.size.y;
		desiredSeperation = 1.5f * spriteHeight;
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
