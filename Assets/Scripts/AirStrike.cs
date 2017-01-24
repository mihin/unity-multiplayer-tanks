using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class AirStrike : NetworkBehaviour
{
	public GameObject bomber;
	public TankCombat tc;
	public bool pendingStrike;
	
	bool mouseDown;
	List<Vector2> path;
	Vector2 last;
		
	[SyncVar]
	public int numStrikes = 3;
	
	[SyncVar]
	public float strikeTimer;
		
	[ClientCallback]
	void Update ()
	{
		
		if (Input.GetMouseButtonDown(0) && pendingStrike)
		{
			mouseDown = true;
			path = new List<Vector2>();
			Vector3 mp = Input.mousePosition;
			mp.z = 10;
			last = Camera.main.ScreenToWorldPoint(mp);
			path.Add(last);
		}
		
		if (mouseDown)
		{
			Vector3 mp = Input.mousePosition;
			mp.z = 10;
			Vector3 p = Camera.main.ScreenToWorldPoint(mp);
			if (((Vector2)p-last).magnitude > 4.0f)
			{
				path.Add(p);
				last = p;
				if (path.Count >= 5)
				{
					CmdStrike(path.ToArray());
					mouseDown = false;
					pendingStrike = false;
				}
			}
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			if (mouseDown)
			{
				mouseDown = false;
				pendingStrike = false;
				CmdStrike(path.ToArray());
			}
		}
	}
	
	[Command]
	public void CmdStrike(Vector2[] strikePath)
	{
		if (!tc.alive)
			return;
			
		if (numStrikes <= 0)
			return;
			
		if (Time.time < strikeTimer)
		{
			// not allowed yet
			return;
		}
		
		numStrikes -=1;
		strikeTimer = Time.time + 10.0f;
		
		GameObject b = (GameObject)GameObject.Instantiate(bomber, strikePath[0], Quaternion.identity);
		b.GetComponent<Bomber>().FollowPath(strikePath);
		b.GetComponent<Bomber>().team = tc.team;
		NetworkServer.Spawn(b);
	}
	
	void OnGUI()
	{
		GUI.color = Color.white;
		if (NetworkClient.active && ClientScene.localPlayers.Count > 0 && ClientScene.localPlayers[0].gameObject != null)
		{
			AirStrike a = ClientScene.localPlayers[0].gameObject.GetComponent<AirStrike>();
			float airStrikeCooldown = Time.time - a.strikeTimer;
			if (airStrikeCooldown < 0)
			{
				GUI.Label (new Rect(200, 70, 200, 20), "AirStrikes: <" + (int)(0-airStrikeCooldown) + "> seconds");
			}
			else
			{
				if (a.pendingStrike)
				{
					GUI.Label (new Rect(200, 70, 200, 20), "Click and Drag Strike Path");
				}
				else if (GUI.Button(new Rect(200, 70, 200, 20), "AirStrikes (" + a.numStrikes + ")") && a.numStrikes > 0)
				{
					a.pendingStrike = true;
				}
			}
		}
	}
	
}
