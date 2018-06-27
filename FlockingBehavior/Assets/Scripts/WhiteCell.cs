using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteCell : Boid
{
	public override void Init()
	{
		furthestToChase = 4.0f;
		base.Init();
	}
}
