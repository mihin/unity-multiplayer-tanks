using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;



public class ControlPoint : NetworkBehaviour
{
	[SyncVar]
	public int score;
	
	[SyncVar]
	bool complete;
	
	[SyncVar]
	int owningTeam = -1;
	
	[SyncVar]
	int tryingTeam = -1;
	
	float scoreTimer;	
	const int controlTime = 10;
	
	public List<TankCombat> targets = new List<TankCombat>();
	
	public override void OnStartServer ()
	{
		PlayGame.singleton.controlComplete += 1;
	}
	
	void OnTriggerEnter2D(Collider2D collider)
	{
		TankCombat tc = collider.gameObject.GetComponent<TankCombat>();
		if (tc != null)
		{
			targets.Add (tc);
			
			if (score < controlTime)
				GetComponent<ParticleSystem>().Play();
		}
	}

	void OnTriggerExit2D(Collider2D collider)
	{
		TankCombat tc = collider.gameObject.GetComponent<TankCombat>();
		if (tc != null)
		{
			targets.Remove(tc);
			if (targets.Count == 0)
			{
				GetComponent<ParticleSystem>().Stop();
			}
		}
	}

	void Update()
	{
		if (Time.time > scoreTimer)
		{
			UpdateScore();
			UpdateColor();
			scoreTimer = Time.time + 1.0f;
		}
	}
	
	[ClientCallback]
	void UpdateColor()
	{
		if (ClientScene.localPlayers.Count == 0)
			return;
		
		Color owningColor = Color.white;
		Color tryingColor = Color.white;

		var controller = ClientScene.localPlayers[0];
		if (controller == null)
			return;

		if (controller.gameObject == null)
			return;

		var playerObj = controller.gameObject;
		TankCombat myTC = playerObj.GetComponent<TankCombat>();
		if (owningTeam != -1)
		{
			if (myTC.team == owningTeam)
			{
				owningColor = Color.green;
			}
			else
			{
				owningColor = Color.red;
			}
		}
		GetComponent<SpriteRenderer>().material.color = owningColor;
		
		if (tryingTeam != -1)
		{
			if (myTC.team == tryingTeam)
			{
				tryingColor = Color.green;
			}
			else
			{
				tryingColor = Color.red;
			}
		}
		ParticleSystemRenderer psr = (ParticleSystemRenderer)(GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>());
		psr.material.color = tryingColor;		
	}
	
	[ServerCallback]
	void UpdateScore()
	{
		int foundTeam = -1;
		HashSet<int> foundTeams = new HashSet<int>();
		foreach (var tc in targets)
		{
			if (tc.alive)
			{
				foundTeams.Add(tc.team);
				foundTeam = tc.team;
			}
		}
		
		// multiple teams on control point, score in unchanged.
		if (foundTeams.Count > 1)
		{
			tryingTeam = -1;
			return;
		}
		
		// zero teams on control point, score in unchanged.
		if (foundTeams.Count == 0)
		{
			tryingTeam = -1;
			return;
		}
		
		tryingTeam = foundTeam;
		// find number of players of foundTeam on the point
		int count = 0;
		foreach (TankCombat tc in targets)
		{
			if (tc.alive && tc.team == foundTeam)
				count += 1;
		}
		
		// owning team alone on control point, score ticks up
		if (tryingTeam == owningTeam || owningTeam == -1)
		{
			ScoreTickUp(tryingTeam, count);
			return;
		}
		
		// other team alone on control point, score ticks down
		if (tryingTeam != owningTeam)
		{
			ScoreTickDown(tryingTeam, count);
		}
	}
	
	[Server]
	void ScoreTickUp(int team, int amount)
	{
		if (!complete)
		{
			if (score == 0)
			{
				owningTeam = team;
			}
			score += amount;
			if (score >= controlTime)
			{
				score = controlTime;
				complete = true;
				PlayGame.singleton.AddControlScore(owningTeam, 1);
				//TODO: event?
			}
		}
	}
	
	[Server]
	void ScoreTickDown(int team, int amount)
	{
		score -= amount;
		if (score <= 0)
		{
			PlayGame.singleton.AddControlScore(owningTeam, -1);		
			owningTeam = -1;
			score = 0;
			complete = false;
			//TODO: event?
		}
	}
	
	[ClientCallback]
	void OnGUI()
	{
		Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
		
		// draw the name with a shadow (colored for buf)	
		GUI.color = Color.black;
		GUI.Label(new Rect(pos.x-50, Screen.height - pos.y - 122, 200, 30), "Score:" + score + "/" + controlTime + " team:" + owningTeam);
		GUI.color = Color.white;
		GUI.Label(new Rect(pos.x-51, Screen.height - pos.y - 121, 200, 30), "Score:" + score + "/" + controlTime + " team:" + owningTeam);
	}
}
