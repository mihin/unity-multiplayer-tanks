using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Friction : NetworkBehaviour {

	public float amount = 0.05f;

	// Update is called once per frame
	void Update ()
	{
		Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
		
		// fake gravity/friction on movement
		if (Mathf.Abs(rigidbody2D.velocity.magnitude) > 0.01f)
		{
			rigidbody2D.velocity *= (1.0f - amount);
		}
		else if (Mathf.Abs(rigidbody2D.velocity.magnitude) > 0)
		{
			rigidbody2D.velocity = Vector2.zero;
		}
		
		// fake gravity/friction on spin
		if (Mathf.Abs(rigidbody2D.angularVelocity) > 0.01f) 
		{
			rigidbody2D.angularVelocity *= (1.0f - amount);
		}
		else if (Mathf.Abs(rigidbody2D.angularVelocity) > 0)
		{
			rigidbody2D.angularVelocity = 0;
		}
	}
	
	[ServerCallback]
	void OnCollisionEnter2D(Collision2D collider)
	{
		// this makes the object sync its position to clients each time it collides with something.
		SetDirtyBit(1);
	}
}
