using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TankCombat : NetworkBehaviour
{
	public GameObject turret;
	public Transform muzzle;
	public GameObject missilePrefab;
	public TankType tt;
	
	public delegate void TakeDamageDelegate(int side, int damage);
	public delegate void DieDelegate();
	public delegate void RespawnDelegate();
	
	[SyncEvent(channel=1)]
	public event TakeDamageDelegate EventTakeDamage;
	
	[SyncEvent]
	public event DieDelegate EventDie;
	
	[SyncEvent]
	public event RespawnDelegate EventRespawn;
	
	[SyncVar]
	public int health = 100;
	
	[SyncVar]
	public int heat = 0;
	
	[SyncVar]
	public int ammunitionTurret = 100;
	
	[SyncVar]
	public int ammunitionMG = 1000;
	
	[SyncVar]
	public bool alive = true;
	
	[SyncVar]
	public bool firingMG = false;
	
	[SyncVar]
	public float armorFront = 1.0f;
	
	[SyncVar]
	public float armorBack = 1.0f;
	
	[SyncVar]
	public float armorLeft = 1.0f;
	
	[SyncVar]
	public float armorRight = 1.0f;
	
	[SyncVar]
	public string tankName;
	
	[SyncVar]
	public string tankType;
	
	[SyncVar]
	public int team;
	
	float fireTurretTimer;	
	float regenTimer;
	float machineGunTimer;
	float deathTimer;
	
	[Server]
	void Respawn()
	{
		InitializeFromTankType(tt);
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		alive = true;
		EventRespawn();
	}
	
	[Server]
	public void InitializeFromTankType(TankType newTT)
	{
		tt = newTT;
		tankType = tt.tankTypeName;
		health = tt.maxHealth;
		heat = 0;
		ammunitionTurret = tt.maxAmmunitionTurret;
		ammunitionMG = tt.maxAmmunitionMG;
		armorFront = tt.armor*2;
		armorBack = tt.armor*0.8f;
		armorLeft = tt.armor;
		armorRight = tt.armor;
		team = (int)(Time.time) % 2;
		
		GetComponent<SpriteRenderer>().sprite = tt.skinBody;
		turret.GetComponent<SpriteRenderer>().sprite = tt.skinTurret;
	}
	
	public override void OnStartClient()
	{
		if (NetworkServer.active)
			return;
			
		//TODO
		TankType found = TankTypeManager.Lookup(tankType);
		tt = found;

		GetComponent<SpriteRenderer>().sprite = tt.skinBody;
		turret.GetComponent<SpriteRenderer>().sprite = tt.skinTurret;
	}
	
	[ServerCallback]
	void Update()
	{
		if (!alive)
		{
			if (Time.time > deathTimer)
			{
				Respawn();
			}
			return;
		}
		
		// heat dissapates over time
		if (Time.time > regenTimer)
		{
			if (heat > 1) {
				heat -= tt.heatRegen;
			}
			regenTimer = Time.time + 1.0f;
		}
		
		
		if (Time.time > machineGunTimer && firingMG)
		{
			// cast ray to hit stuff
			ammunitionMG -= 1;
			machineGunTimer += 0.33f;
		}
	}

	public bool CanFireTurret()
	{
		if (heat >= (int)(tt.maxHeat*0.9f))
			return false;
			
		if (ammunitionTurret <= 0)
			return false;
			
		if (Time.time < fireTurretTimer)
			return false;
		
		if (!alive)
			return false;
			
		return true;
	}
	
	
	float HitArmour(int side, float amount)
	{
		switch (side)
		{
		case 0: 
			armorFront -= amount; 
			return armorFront;
			
		case 1: 
			armorRight -= amount;
			return armorRight;
			
		case 2: 
			armorBack -= amount;
			return armorBack;
			
		case 3:
			armorLeft -= amount;
			return armorLeft;
		}
		return 0.0f;
	}
		
	[Server]
	public void GotHitByMissile(int side, int damage, int team)
	{
		if (this.team == team)
		{
			return;
		}
		
		EventTakeDamage(side, damage);
		float armor = HitArmour(side, damage/100.0f);
		if (armor <= 0.0f)		
			TakeDamage(damage);
		else
			TakeDamage(damage/10);
	}
	
	[Server]
	public void GotHitByMachineGun()
	{
		TakeDamage(2);
	}
		
	[Server]
	void TakeDamage(int amount)
	{
		if (!alive)
			return;
			
		if (health > amount) {
			health -= amount;
		}
		else
		{
			health = 0;
			alive = false;
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			// play explosion!
			// set death timer for respawn
			EventDie();
			deathTimer = Time.time + 5.0f;
		}
	}
	
	[Command]
	public void CmdFireTurret()
	{
		if (PlayGame.GetComplete())
			return;
			
		if (!CanFireTurret())
			return;
			
		heat += tt.fireHeat;
		ammunitionTurret -= 1;
		fireTurretTimer = Time.time + tt.fireRateTurret;
			
		GameObject missile = (GameObject)GameObject.Instantiate(missilePrefab, muzzle.position, muzzle.rotation);
		missile.GetComponent<Rigidbody2D>().velocity = muzzle.right*10;
		missile.GetComponent<Missile>().damage = tt.turretDamage;
		missile.GetComponent<Missile>().team = team;
		NetworkServer.Spawn(missile);
	}
	
	[Command]
	public void CmdBeginFireMachineGun()
	{
		if (PlayGame.GetComplete())
			return;
		
		if (!alive)
			return;
			
		if (ammunitionMG > 0)
		{
			firingMG = true;
		}
	}
	
	[Command]
	public void CmdStopFireMachineGun()
	{
		firingMG = false;
	}
	
	[Command]
	public void CmdSetName(string name)
	{
		tankName = name;
	}
	
	[Command]
	public void CmdKillSelf()
	{
		TakeDamage(1000000);
	}
	
	public override void OnStartLocalPlayer()
	{
		//CmdSetName(Manager.singleton.tankName + "-" + tt.tankTypeName);
	}
}
