using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Bomb : MonoBehaviour {

	public int team;
	public GameObject explosion;
	public GameObject scorchMark;
	
	float timer;
	int count;
	
	void Update ()
	{
		if (Time.time > timer)
		{
			transform.localScale *= 0.9f;
			timer = Time.time + 0.1f;
			count += 1;
			if (count == 8)
			{
				GameObject.Destroy(gameObject);
				
				GameObject scorch = (GameObject)GameObject.Instantiate(scorchMark, transform.position, Quaternion.identity);
				GameObject.Destroy(scorch, 1);
				
				for (int i=0; i < 3; i++)
				{
					Vector3 pos = transform.position;
					pos.x += UnityEngine.Random.Range(-10,10)/10.0f;
					pos.y += UnityEngine.Random.Range(-10,10)/10.0f;
					GameObject exp = (GameObject)GameObject.Instantiate(explosion, pos, Quaternion.identity);
					GameObject.Destroy(exp, 1);
				}
				
				if (NetworkServer.active)
				{
					Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.0f);
					foreach (var hit in hits)
					{
						TankCombat hitTc = hit.GetComponent<TankCombat>();
						if (hitTc != null)
						{
							int side = Missile.GetHitSide(transform.position, hit.transform.position, hit.transform.right);
							hitTc.GotHitByMissile(side, 50, team);
						}
					}
				}
				
			}
		}
	}
}
