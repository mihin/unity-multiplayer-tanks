using UnityEngine;
using System.Collections;

[System.Serializable]
public class TankType : ScriptableObject 
{
	public string tankTypeName;

	public int maxHealth = 100;
	public int maxHeat = 100;
	public int maxAmmunitionTurret = 100;
	public int maxAmmunitionMG = 1000;
	public float fireRateTurret = 0.5f;
	public int fireHeat = 7;
	public float armor = 1.0f;
	public int turretDamage = 20;
	public int heatRegen = 2;
	
	public float rotateSpeed = 80f;
	public 	float acceleration = 0.04f;
	public float topSpeed = 3.0f;
	public float bottomSpeed = -1.5f;
	public float speed = 0.0f;
	
	public Sprite skinBody;
	public Sprite skinTurret;
	
}
