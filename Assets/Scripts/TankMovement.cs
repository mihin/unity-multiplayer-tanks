using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TankMovement : NetworkBehaviour 
{

	float speed = 0.0f;
	
	// for client movement command throttling
	float oldMoveForce = 0;
	float oldSpin = 0;
	
	// server movement
	float thrusting;
	float spin;
	
	bool firingMachineGun;
	bool m_focus = true;
	
	[SyncVar]
	public float turretAngle;
	
	public Transform turret;
	public GameObject explosion;	
	public GameObject tracks;
	public TankCombat tc;
	public ParticleSystem machineGun;
	
	public float trackTimer;
	
	float turrentSendTimer = 0.0f;
	float turrentSendDelay = 0.1f;
	
	void Update() 
	{
		if (NetworkServer.active)
			UpdateServer();
		
		if (NetworkClient.active)
			UpdateClient();
	}
	
	void UpdateServer()
	{
		//NOTE: frame rate dependent
	
		// update rotation 
		float tmpRotation = tc.tt.rotateSpeed;
		if (Mathf.Abs(speed) < 0.5f) {
			tmpRotation *= 2;
		}
		float rotate = spin * tc.tt.rotateSpeed;
		GetComponent<Rigidbody2D>().angularVelocity = rotate;	
		
		// update thrust
		if (thrusting > 0) {
			speed += tc.tt.acceleration;
			if (speed >= tc.tt.topSpeed) {
				speed = tc.tt.topSpeed;
			}
		}
		else if (thrusting < 0)
		{
			speed -= tc.tt.acceleration*2;
			if (speed <= tc.tt.bottomSpeed) {
				speed = tc.tt.bottomSpeed;
			}
		}
		else 
		{
			speed *= 0.98f;
		}

		float rot = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
		Vector2 d = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot)) * speed;
		//Debug.Log ("rot: " + rot + " " + d + "thrusting: " + thrusting + "speed:" + speed);
		GetComponent<Rigidbody2D>().velocity = d;
		

	}
	
	void OnApplicationFocus(bool value)
	{
		m_focus = value;
	}
	
	void UpdateClient()
	{
		if (tc.alive && Time.time > trackTimer && GetComponent<Rigidbody2D>().velocity.magnitude > 0.001f)
		{
			GameObject footprint = (GameObject)GameObject.Instantiate(tracks, transform.position, transform.rotation);
			GameObject.Destroy(footprint, 1.5f);
			trackTimer = Time.time + 0.25f; 
		}
			
		if (!isLocalPlayer)
		{
			Vector3 trotationVector = new Vector3 (0, 0, turretAngle);
			turret.transform.rotation = Quaternion.Euler(trotationVector);
			return;
		}
		
		if (!m_focus)
			return;
		
		
		/*if (Manager.singleton.GetComponent<BatchModeStartup>().batchMode)
		{
			AutoMove();
			return;
		}*/
		
		HandlePlayerMovement();
		
		if (Input.GetMouseButtonDown(0))
		{
			tc.CmdFireTurret();
		}
		
		if (Input.GetMouseButtonDown(1))
		{
			if (!firingMachineGun)
			{
				machineGun.Play();
				firingMachineGun = true;
				tc.CmdBeginFireMachineGun();
			}
		}
		
		if (Input.GetMouseButtonUp(1))
		{
			if (firingMachineGun)
			{
				machineGun.Stop();
				firingMachineGun = false;
				tc.CmdStopFireMachineGun();
			}
		}
		
		if (Input.GetKey(KeyCode.F1))
		{
			tc.CmdKillSelf();
		}
			
		// keep camera on me
		Vector3 cpos = transform.position;
		cpos.z = Camera.main.transform.position.z;
		Camera.main.transform.position = cpos;
		
		// point turret at mouse
		if (Time.time > turrentSendTimer)
		{
			Vector3 mouse_pos = Input.mousePosition;
			mouse_pos.z = 0.0f; 
			Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
			mouse_pos.x = mouse_pos.x - object_pos.x;
			mouse_pos.y = mouse_pos.y - object_pos.y;
			float angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
			Vector3 rotationVector = new Vector3 (0, 0, angle);
			turret.transform.rotation = Quaternion.Euler(rotationVector);
			CmdRotateTurret(angle);
			turrentSendTimer = Time.time + turrentSendDelay;
		}
	}
	
	void HandlePlayerMovement()
	{
		int spin = 0;
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			spin += 1;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			spin -= 1;
		}
		float moveForce = Input.GetAxis("Vertical");
		if (moveForce > 0) { moveForce = 1; }
		if (moveForce < 0) { moveForce = -1; }
		
		if (oldMoveForce != moveForce || oldSpin != spin)
		{
			CmdThrust(moveForce, spin);
			oldMoveForce = moveForce;
			oldSpin = spin;
		}
	}

	[Command]
	public void CmdRotateTurret(float angle)
	{
		if (!tc.alive)
			return;
			
		if (PlayGame.GetComplete())
			return;
			
		Vector3 rotationVector = new Vector3 (0, 0, angle);
		turret.transform.rotation = Quaternion.Euler(rotationVector);
		turretAngle = angle;	
	}
	

	
	[Command]
	public void CmdThrust(float thrusting, int spin)
	{	
		if (PlayGame.GetComplete() || !tc.alive) {
			this.thrusting = 0;
			this.spin = 0;
			return;
		}
			
		this.thrusting = thrusting;
		this.spin = spin;
	}
	
	///////////////////////////////////////////////////////////////
	
	float autoMoveTimer = 0;
	float autoTurret;
	
	void AutoMove()
	{
		if (Time.time > autoMoveTimer)
		{
			autoTurret += Random.Range(-90,90);
			CmdRotateTurret(autoTurret);
			
			float thrust = (Random.Range(-10,10) * 0.1f);
			int spin = Random.Range(-1,1);
			CmdThrust(thrust, spin);
			autoMoveTimer = Time.time + 1;
			
			if (Random.Range(0,4) == 1)
			{
				tc.CmdFireTurret();
			}
		}
	}
	
}
