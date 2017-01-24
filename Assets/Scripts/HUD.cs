using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class HUD : NetworkBehaviour {

	Texture box;
	
	public TankCombat tc;
	
	public Texture front;
	public Texture back;
	public Texture left;
	public Texture right;
	
	float damageTimer;
	int damageAmount;
	int damageSide;

	void Start()
	{
		box = (Texture)Resources.Load ("box");
		tc.EventTakeDamage += OnTakeDamage;
	}
	
	void OnTakeDamage(int side, int damage)
	{
		damageAmount = damage;
		damageSide = side;
		damageTimer = Time.time + 3;
	}
	
	void DrawBar(Color c, Vector3 pos, int offsetY, int current, int max)
	{
		GUI.color = Color.grey;
		GUI.DrawTexture (new Rect(pos.x-max/4 - 1, Screen.height - pos.y - offsetY, max/2 + 2, 7), box);
		
		GUI.color = c;
		GUI.DrawTexture (new Rect(pos.x-max/4, Screen.height - pos.y - offsetY + 1, current/2, 5), box);
	}
	
	void OnGUI()
	{
		Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
		
		int myTeam = -1;
		if (NetworkClient.active && ClientScene.localPlayers.Count > 0 && ClientScene.localPlayers[0].gameObject != null)
		{
			TankCombat localPlayerTc = ClientScene.localPlayers[0].gameObject.GetComponent<TankCombat>();
			myTeam = localPlayerTc.team;
		}
		
		// your team is green, other teams are red
		Color teamColor = Color.white;
		if (myTeam == tc.team)
		{
			teamColor = Color.green;
		}
		else
		{
			teamColor = Color.red;
		}
		
		// draw the name with a shadow (colored for buf)
		GUI.color = Color.black;
		GUI.Label(new Rect(pos.x-50, Screen.height - pos.y - 62, 150, 30), tc.tankName);
		GUI.color = teamColor;
		GUI.Label(new Rect(pos.x-51, Screen.height - pos.y - 61, 150, 30), tc.tankName);
		
		if (tc.alive && tc.tt != null)
		{
			DrawBar (teamColor, pos, 40, tc.health, tc.tt.maxHealth);
			DrawBar (Color.yellow, pos, 34, tc.heat, tc.tt.maxHeat);
		}
		else
		{
			GUI.color = Color.black;
			GUI.Label(new Rect(pos.x-40, Screen.height - pos.y - 42, 150, 30), "YOU ARE DEAD");
			GUI.color = Color.white;
			GUI.Label(new Rect(pos.x-41, Screen.height - pos.y - 41, 150, 30), "YOU ARE DEAD");
		}

		if (!isLocalPlayer)
			return;
		
		// draw direction damage indicators	
		DrawArmour(new Rect(20, 100, 64, 32), front, tc.armorFront);
		DrawArmour(new Rect(20, 210, 64, 32), back, tc.armorBack);
		DrawArmour(new Rect(20, 136, 24, 72), left, tc.armorLeft);
		DrawArmour(new Rect(60, 136, 24, 72), right, tc.armorRight);
		
		if (damageTimer > Time.time)
		{
			Rect r = new Rect(0,0,0,0);
			if (damageSide == 0) {
				r = new Rect(40, 80, 40, 40);
			} else if (damageSide == 1) {
				r = new Rect(100, 160, 40, 40);
			} else if (damageSide == 2) {
				r = new Rect(40, 260, 40, 40);
			} else if (damageSide == 3) {
				r = new Rect(2, 160, 40, 40);
			}
			GUI.color = Color.red;
			GUI.Label(r, "[" + damageAmount + "]");
			
		}
		
		GUI.color = Color.white;
		GUI.Label(new Rect(5, 5, 180, 60), "Arrows to move, mouse buttons to fire. Escape to exit. Destroy three barrels.");
		
		GUI.Label(new Rect(10, 250, 200, 20), "Turret Ammo: " + tc.ammunitionTurret + "/" + tc.tt.maxAmmunitionTurret );
		GUI.Label(new Rect(10, 270, 200, 20), "Flame Ammo: " + tc.ammunitionMG + "/" + tc.tt.maxAmmunitionMG);
		
		GUI.Label (new Rect(200, 10, 200, 20), "Barrels: " + PlayGame.GetBarrelScore());
		
		if (myTeam == 0) {
			GUI.color = Color.green;
		} else {
			GUI.color = Color.red;
		}
		GUI.Label (new Rect(200, 30, 200, 20), "Control(0): " + PlayGame.GetControlScore(0));
		
		if (myTeam == 1) {
			GUI.color = Color.green;
		} else {
			GUI.color = Color.red;
		}
		GUI.Label (new Rect(200, 50, 200, 20), "Control(1): " + PlayGame.GetControlScore(1));
		
		
		GUI.color = Color.white;
		if (NetworkClient.active)
		{
			GUI.Label(new Rect(5, 5, 180, 60), "RTT: " + NetworkClient.allClients[0].GetRTT());
		}
		
		GUI.color = Color.white;
		if (PlayGame.GetComplete())
		{
			GUI.Label (new Rect(Screen.width/2 - 60, Screen.height/2-100, 200, 40), "--- Level Complete ---\nPRESS SPACE TO RESTART");
		}
	}
	
	void DrawArmour(Rect pos, Texture t, float value)
	{
		if (value >= 0.8f)	{
			GUI.color = Color.green;
		} else if (value >= 0.4f) {
			GUI.color = Color.yellow;
		} else if (value >= 0.1f) { 
			GUI.color = Color.red;
		} else {
			GUI.color = Color.gray;
		}
		
		GUI.DrawTexture (pos, t);
	}
	
	void Update()
	{
		if (!isLocalPlayer)
			return;
		
		if (PlayGame.GetComplete())
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				CmdFinishLevel();
			}
		}
	}
	
	[Command]
	public void CmdFinishLevel()
	{
		NetworkManager.singleton.ServerChangeScene("forest");
		//Manager.singleton.SwitchLevel("forest");
	}
	
}
