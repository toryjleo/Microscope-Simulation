using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : Boid
{
	SpriteRenderer sprite = null;

	private float spriteHeight;

	private bool isAlive;


	public bool IsAlive
	{
		get
		{
			return isAlive;
		}
	}


	public override void Init()
	{
		base.Init();
		isAlive = true;
		sprite = gameObject.GetComponent<SpriteRenderer>();
		spriteHeight = sprite.size.y;
		desiredSeperation = 1.5f * spriteHeight;
	}

	public void Die()
	{
		isAlive = false;
		if (sprite != null)
		{
			StartCoroutine(DeathCoroutine());
		}
	}


	private IEnumerator DeathCoroutine()
	{
		while(sprite.color.a > 0)
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
}
