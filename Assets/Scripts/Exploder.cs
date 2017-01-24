using UnityEngine;
using System.Collections;

using UnityEngine.Networking;

public class Exploder : NetworkBehaviour
{
	public GameObject explosion;
	
	[Server]
	public void Explode()
	{
		if (isClient)
		{
			GameObject ex = (GameObject)GameObject.Instantiate(explosion, transform.position, Quaternion.identity);
			ex.transform.localScale = new Vector3(3,3,3);
			GameObject.Destroy(ex, 1.2f);
		}
		NetworkServer.Destroy(gameObject);
	}
	
	public override void OnNetworkDestroy()
	{
		GameObject ex = (GameObject)GameObject.Instantiate(explosion, transform.position, Quaternion.identity);
		ex.transform.localScale = new Vector3(3,3,3);
		GameObject.Destroy(ex, 1.2f);
	}
}
