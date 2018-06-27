using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : Boid
{
	private float spriteHeight;

	public override void Init()
	{
		SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
		spriteHeight = sprite.size.y;
		desiredSeperation = 1.5f * spriteHeight;
		base.Init();
	}
}
