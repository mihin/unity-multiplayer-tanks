using UnityEngine;
using System.Collections;

public class CombatEffects : MonoBehaviour
{
	public TankCombat tc;

	public GameObject deathExplosion;
	public GameObject scorchMark;
	
	void Start()
	{
		tc.EventDie += OnDeath;
		tc.EventRespawn += OnRespawn;
	}
	
	
	public void OnRespawn()
	{
		gameObject.GetComponent<SpriteRenderer>().enabled = true;
		tc.turret.GetComponent<SpriteRenderer>().enabled = true;
	}
	
	public void OnDeath()
	{
	
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		tc.turret.GetComponent<SpriteRenderer>().enabled = false;
		
		GameObject scorch = (GameObject)GameObject.Instantiate(scorchMark, transform.position, Quaternion.identity);
		GameObject.Destroy(scorch, 10);
			
		for (int i=0; i < 10; i++)
		{
			Vector3 pos = transform.position;
			pos.x += UnityEngine.Random.Range(-20,20)/10.0f;
			pos.y += UnityEngine.Random.Range(-20,20)/10.0f;
			GameObject exp = (GameObject)GameObject.Instantiate(deathExplosion, pos, Quaternion.identity);
			GameObject.Destroy(exp, 1);
		}
	}
}
