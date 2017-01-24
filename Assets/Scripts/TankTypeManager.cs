using UnityEngine;
using System.Collections;

public class TankTypeManager : MonoBehaviour
{
	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
		singleton = this;
	}
	
	static TankTypeManager singleton;

	public TankType[] tanks;
	
	static public TankType Random()
	{
		int index = UnityEngine.Random.Range(0,singleton.tanks.Length);
		return singleton.tanks[index];
	}
	
	static public TankType Lookup(string name)
	{
		foreach (var tt in singleton.tanks)
		{
			if (tt.tankTypeName == name)
				return tt;
		}
		return null;
	}
}
