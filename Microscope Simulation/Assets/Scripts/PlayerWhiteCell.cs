using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiteCell : WhiteCell
{

	#region METHODS

	/// <summary>
	/// Takes player input to move the player left, right, up and down
	/// </summary>
	public void PlayerInput()
	{
		if (Input.GetKey(KeyCode.W))
		{
			Vector3 seekForce = Seek(position + Vector3.up);
			ApplyForce(seekForce);
		}
		if (Input.GetKey(KeyCode.S))
		{
			Vector3 seekForce = Seek(position + Vector3.down);
			ApplyForce(seekForce);
		}
		if (Input.GetKey(KeyCode.A))
		{
			Vector3 seekForce = Seek(position + Vector3.left);
			ApplyForce(seekForce);
		}
		if (Input.GetKey(KeyCode.D))
		{
			Vector3 seekForce = Seek(position + Vector3.right);
			ApplyForce(seekForce);
		}
	}

	#endregion
}
